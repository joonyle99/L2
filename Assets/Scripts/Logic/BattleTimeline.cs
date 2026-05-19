using System.Collections.Generic;

public enum BattleEventType { Attack, Death }

public class BattleEvent
{
    public float Time;
    public BattleEventType Type;
    public BattleHero Attacker;
    public BattleHero Target;
    public int Damage;
}

public class BattleTimeline
{
    public readonly List<BattleEvent> Events = new List<BattleEvent>();
    public bool PlayerWon;
}
