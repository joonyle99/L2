using UnityEngine;

public class BattleHero
{
    public HeroInstance Source; // 원본 참조 (능력, 데이터 조회용)
    public int SlotIndex; // Bench 상의 원래 슬롯 (0~4)
    public bool IsPlayerSide;

    public int CurrAttack; // 전투 중 변하는 값 (Source.CurrAttack에서 복사)
    public int CurrHealth; // 전투 중 변하는 값
    public int MaxHealth;

    public bool IsAlive => CurrHealth > 0;

    public BattleHero(HeroInstance hero, int slot, bool isPlayer)
    {
        Source = hero;
        SlotIndex = slot;
        IsPlayerSide = isPlayer;
        CurrAttack = hero.Attack;
        CurrHealth = hero.Health;
        MaxHealth = hero.Health;
    }
}
