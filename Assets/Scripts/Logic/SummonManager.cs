using System;
using UnityEngine;

public class SummonManager : IBenchProvider<HeroInstance>
{
    private HeroDatabase _heroDatabase;
    private SummonConfig _summonConfig;
    private SummonTable _summonTable;

    private HeroInstance[] _heroBench;
    public HeroInstance[] Bench => _heroBench;
    public event Action OnBenchChanged;

    private Func<int, bool> _trySpendGold;
    private Action<int, int> _onSummonCostChanged;

    private int _currCost;
    public int CurrCost => _currCost;

    public SummonManager(HeroDatabase heroDatabase, SummonConfig summonConfig, SummonTable summonTable)
    {
        _heroDatabase = heroDatabase;
        _summonConfig = summonConfig;
        _summonTable = summonTable;

        _heroBench = new HeroInstance[_summonConfig.capacity];
        var prevCost = _summonConfig.baseCost;
        var currCost = _summonConfig.baseCost;
        _currCost = currCost;
        _onSummonCostChanged?.Invoke(prevCost, currCost);
    }

    public void Initialize(Func<int, bool> trySpendGold, Action<int, int> onSummonCostChanged)
    {
        _trySpendGold = trySpendGold;
        _onSummonCostChanged = onSummonCostChanged;
    }

    // === 소환 ===

    public bool TrySummon(int round)
    {
        if (IsBenchFull()) return false;
        if (!_trySpendGold(_currCost)) return false;

        var grade = PickGrade(round);
        var heroData = PickHero(grade);
        if (heroData == null) return false;

        var heroInstance = new HeroInstance(heroData);
        AddToBench(heroInstance, -1);
        var prevCost = _currCost;
        var currCost = prevCost + _summonConfig.costIncrement;
        _currCost = currCost;
        _onSummonCostChanged?.Invoke(prevCost, currCost);

        return true;
    }

    public void ResetCost()
    {
        var prevCost = _currCost;
        var currCost = _summonConfig.baseCost;
        _currCost = currCost;
        _onSummonCostChanged?.Invoke(prevCost, currCost);
    }

    private HeroGrade PickGrade(int round)
    {
        var gradeChanceEntries = _summonTable.GetGradeChanceEntries(round);
        var randomChance = UnityEngine.Random.Range(0f, 1f);

        var cumulativeChance = 0f;

        foreach (var gradeChanceEntry in gradeChanceEntries)
        {
            cumulativeChance += gradeChanceEntry.chance;

            if (randomChance <= cumulativeChance)
            {
                return gradeChanceEntry.grade;
            }
        }

        return gradeChanceEntries[gradeChanceEntries.Length - 1].grade;
    }

    private HeroData PickHero(HeroGrade grade)
    {
        var pool = _heroDatabase.GetHeroesByGrade(grade);
        if (pool == null || pool.Count == 0) return null;

        int idx = UnityEngine.Random.Range(0, pool.Count);
        return pool[idx];
    }

    // === 관리 ===

    public bool IsBenchFull()
    {
        for (int i = 0; i < _heroBench.Length; i++)
        {
            if (_heroBench[i] == null)
            {
                return false;
            }
        }

        return true;
    }

    public bool AddToBench(HeroInstance heroInstance, int idx, bool silent = false)
    {
        for (int i = 0; i < _heroBench.Length; i++)
        {
            if (_heroBench[i] == null)
            {
                _heroBench[i] = heroInstance;
                if (!silent) OnBenchChanged?.Invoke();

                return true;
            }
        }

        return false;
    }

    public HeroInstance TakeFromBench(int idx, bool silent = false)
    {
        if (idx < 0 || idx >= _summonConfig.capacity) return null;

        var heroInstance = _heroBench[idx];
        if (heroInstance == null) return null;

        _heroBench[idx] = null;
        if (!silent) OnBenchChanged?.Invoke();

        return heroInstance;
    }
}
