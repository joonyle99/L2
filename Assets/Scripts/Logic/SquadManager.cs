using System;
using UnityEngine;
using System.Collections.Generic;

public class SquadManager : IBenchProvider<HeroInstance>
{
    private readonly SquadConfig _squadConfig;

    private HeroInstance[] _heroBench;
    public HeroInstance[] Bench => _heroBench;
    public event Action OnBenchChanged;

    public SquadManager(SquadConfig squadConfig)
    {
        _squadConfig = squadConfig;
        _heroBench = new HeroInstance[_squadConfig.capacity];
    }

    public void Initialize()
    {

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
        if (idx < 0 || idx >= _heroBench.Length) return false;
        if (_heroBench[idx] != null) return false;

        _heroBench[idx] = heroInstance;
        if (!silent) OnBenchChanged?.Invoke();

        return true;
    }

    public HeroInstance TakeFromBench(int idx, bool silent = false)
    {
        if (idx < 0 || idx >= _heroBench.Length) return null;

        var heroInstance = _heroBench[idx];
        if (heroInstance == null) return null;

        _heroBench[idx] = null;
        if (!silent) OnBenchChanged?.Invoke();

        return heroInstance;
    }

    public bool MoveHero(int srcIdx, int dstIdx)
    {
        if (srcIdx < 0 || srcIdx >= _heroBench.Length) return false;
        if (dstIdx < 0 || dstIdx >= _heroBench.Length) return false;
        if (srcIdx == dstIdx) return false;
        if (_heroBench[srcIdx] == null) return false;

        (_heroBench[dstIdx], _heroBench[srcIdx]) = (_heroBench[srcIdx], _heroBench[dstIdx]);
        OnBenchChanged?.Invoke();

        return true;
    }

    /// <summary>
    /// 지정한 슬롯(idx)에 영웅을 끼워 넣습니다.
    /// 해당 슬롯이 이미 차 있으면, pushDir 방향으로 빈 슬롯을 찾아 기존 영웅들을 한 칸씩 밀어낸 뒤 삽입합니다.
    /// 빈 슬롯이 없으면 삽입하지 않고 false를 반환합니다.
    /// </summary>
    /// <param name="hero">삽입할 영웅 인스턴스</param>
    /// <param name="targetIdx">삽입할 목표 슬롯 인덱스</param>
    /// <param name="pushDir">빈 슬롯을 탐색할 방향 (+1: 오른쪽, -1: 왼쪽)</param>
    /// <returns>삽입 성공 시 true, 밀 공간이 없으면 false</returns>
    public bool InsertHero(HeroInstance hero, int targetIdx, int pushDir)
    {
        var emptyIdx = FindEmptyInDirection(targetIdx, pushDir);
        if (emptyIdx == -1) return false;

        ShiftRange(targetIdx, emptyIdx, pushDir);
        
        _heroBench[targetIdx] = hero;
        OnBenchChanged?.Invoke();

        return true;
    }

    /// <summary>
    /// 영웅을 srcIdx에서 꺼내어 targetIdx로 이동시킵니다.
    /// targetIdx가 차 있으면 pushDir 방향으로 영웅들을 밀어 공간을 만든 뒤 이동합니다.
    /// 공간을 만들 수 없으면 원래 자리에 되돌려 놓고 false를 반환합니다.
    /// </summary>
    /// <param name="srcIdx">이동할 영웅이 있는 슬롯 인덱스</param>
    /// <param name="dstIdx">이동할 목표 슬롯 인덱스</param>
    /// <param name="pushDir">공간을 만들 방향 (+1: 오른쪽, -1: 왼쪽)</param>
    /// <returns>이동 성공 시 true, 공간 확보 실패 시 false</returns>
    public bool MoveHeroWithPush(int srcIdx, int dstIdx, int pushDir)
    {
        if (srcIdx == dstIdx) return false;

        var hero = _heroBench[srcIdx];
        _heroBench[srcIdx] = null;

        var isSuccess = InsertHero(hero, dstIdx, pushDir);
        if (isSuccess == false)
        {
            // InsertHero 호출 전에 srcIdx를 null로 비웠으므로, 실패 시 원래 자리에 되돌려야 데이터 손실이 없음
            _heroBench[srcIdx] = hero;
            return false;
        }

        return true;
    }

    /// <summary>
    /// pushDir 방향으로 빈 슬롯이 존재하는지 확인합니다. 실제 삽입 전 사전 검사에 사용합니다.
    /// </summary>
    public bool CanInsertHero(int targetIdx, int pushDir)
        => FindEmptyInDirection(targetIdx, pushDir) != -1;

    /// <summary>
    /// startIdx부터 pushDir 방향으로 순서대로 탐색하여 처음 만나는 빈 슬롯의 인덱스를 반환합니다.
    /// 배열 범위를 벗어날 때까지 빈 슬롯이 없으면 -1을 반환합니다.
    /// </summary>
    /// <param name="startIdx">탐색을 시작할 슬롯 인덱스</param>
    /// <param name="pushDir">탐색 방향 (+1: 오른쪽, -1: 왼쪽)</param>
    /// <returns>빈 슬롯의 인덱스, 없으면 -1</returns>
    private int FindEmptyInDirection(int startIdx, int pushDir)
    {
        for (int i = startIdx; i >= 0 && i < _heroBench.Length; i += pushDir)
        {
            if (_heroBench[i] == null)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// fromIdx ~ toIdx 범위의 영웅들을 pushDir 방향으로 한 칸씩 밀어냅니다.
    /// toIdx는 반드시 비어 있는 슬롯이어야 하며, 이동 후 fromIdx는 null(빈 슬롯)이 됩니다.
    /// 예) pushDir = +1, [A][B][C][_] → fromIdx=0, toIdx=3 → [_][A][B][C]
    /// </summary>
    /// <param name="fromIdx">밀기 시작 위치 (이동 후 비워질 슬롯)</param>
    /// <param name="toIdx">밀기 끝 위치 (현재 비어 있는 슬롯)</param>
    /// <param name="pushDir">밀기 방향 (+1: 오른쪽, -1: 왼쪽)</param>
    private void ShiftRange(int fromIdx, int toIdx, int pushDir)
    {
        // toIdx 방향으로 한 칸씩 밀기 (toIdx는 비어있는 슬롯)
        for (int i = toIdx; i != fromIdx; i -= pushDir)
        {
            _heroBench[i] = _heroBench[i - pushDir];
        }
        
        // fromIdx를 빈 슬롯으로 만든다
        _heroBench[fromIdx] = null;
    }
}
