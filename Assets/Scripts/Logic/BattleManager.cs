using System;
using System.Linq;

public class BattleManager
{
    private BattleSimulator _battleSimulator;
    private BattlePlayer _battlePlayer;
    private Action _onBattleEnd;

    public void Initialize(BattleSimulator battleSimulator, BattlePlayer battlePlayer, Action onBattleEnd)
    {
        _battleSimulator = battleSimulator;
        _battlePlayer = battlePlayer;
        _onBattleEnd = onBattleEnd;
    }

    public void StartBattle(HeroInstance[] playerHeroes, HeroInstance[] enemyHeroes)
    {
        var playerSide = playerHeroes.Select((hero, idx) => new BattleHero(hero, idx, isPlayer: true)).ToArray();
        var enemySide = enemyHeroes.Select((hero, idx) => new BattleHero(hero, idx, isPlayer: false)).ToArray();
        var timeline = _battleSimulator.Simulate(playerSide, enemySide);
        _battlePlayer.Play(timeline, _onBattleEnd);
    }
}
