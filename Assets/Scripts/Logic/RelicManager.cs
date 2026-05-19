using System;

public class RelicManager : IBenchProvider<RelicInstance>
{
    private readonly RelicDatabase _relicDatabase;
    private readonly RelicConfig _relicConfig;

    private RelicInstance[] _relicBench;
    public RelicInstance[] Bench => _relicBench;
    public event Action OnBenchChanged;

    public RelicManager(RelicDatabase relicDatabase, RelicConfig relicConfig)
    {
        _relicDatabase = relicDatabase;
        _relicConfig = relicConfig;
        _relicBench = new RelicInstance[_relicConfig.capacity];
        
        TryAddRelic(PickRelic());
        TryAddRelic(PickRelic());
    }

    // === 획득 ===

    public bool TryAddRelic(RelicData data)
    {
        if (data == null || IsBenchFull()) return false;

        var relicInstance = new RelicInstance(data);
        return AddToBench(relicInstance, -1);
    }

    private RelicData PickRelic()
    {
        var relics = _relicDatabase.GetAllRelics();
        if (relics == null || relics.Count == 0) return null;
        return relics[UnityEngine.Random.Range(0, relics.Count)];
    }

    // === 관리 ===

    public bool IsBenchFull()
    {
        for (int i = 0; i < _relicBench.Length; i++)
        {
            if (_relicBench[i] == null)
            {
                return false;
            }
        }

        return true;
    }

    public bool AddToBench(RelicInstance item, int idx, bool silent = false)
    {
        for (int i = 0; i < _relicBench.Length; i++)
        {
            if (_relicBench[i] == null)
            {
                _relicBench[i] = item;
                if (!silent) OnBenchChanged?.Invoke();

                return true;
            }
        }

        return false;
    }

    public RelicInstance TakeFromBench(int idx, bool silent = false)
    {
        if (idx < 0 || idx >= _relicBench.Length) return null;

        var relicInstance = _relicBench[idx];
        if (relicInstance == null) return null;

        _relicBench[idx] = null;
        if (!silent) OnBenchChanged?.Invoke();

        return relicInstance;
    }
}
