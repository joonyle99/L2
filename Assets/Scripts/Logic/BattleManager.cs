using UnityEngine;

public class BattleManager
{
    private BattleSimulator _battleSimulator;
    private BattlePlayer _battlePlayer;

    public void Initialize(BattleSimulator battleSimulator, BattlePlayer battlePlayer)
    {
        _battleSimulator = battleSimulator;
        _battlePlayer = battlePlayer;
    }
}
