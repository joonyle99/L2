public static class LuckyExtensions
{
    public static string ToDisplayText(this HeroGrade grade) => grade switch
    {
        HeroGrade.Basic     => "일반",
        HeroGrade.Rare      => "레어",
        HeroGrade.Epic      => "영웅",
        HeroGrade.Legendary => "전설",
        HeroGrade.Myth      => "신화",
        _                   => grade.ToString()
    };

    public static string ToDisplayText(this AbilityTrigger trigger) => trigger switch
    {
        AbilityTrigger.OnBattleStart => "전투 시작",
        AbilityTrigger.OnAttack      => "공격 시",
        AbilityTrigger.OnHit         => "피격 시",
        AbilityTrigger.OnAllyDeath   => "아군 사망 시",
        AbilityTrigger.OnSelfDeath   => "자신 사망 시",
        AbilityTrigger.OnSold        => "판매 시",
        AbilityTrigger.OnSummon      => "소환 시",
        AbilityTrigger.OnRoundStart  => "라운드 시작",
        AbilityTrigger.OnRoundEnd    => "라운드 종료",
        _                            => trigger.ToString()
    };

    public static string ToDisplayText(this AbilityTarget target) => target switch
    {
        AbilityTarget.Self        => "자신",
        AbilityTarget.RandomAlly  => "랜덤 아군",
        AbilityTarget.AllAllies   => "전체 아군",
        AbilityTarget.FrontAlly   => "맨 앞 아군",
        AbilityTarget.RandomEnemy => "랜덤 적",
        AbilityTarget.FrontEnemy  => "맨 앞 적",
        AbilityTarget.AllEnemies  => "전체 적",
        _                         => target.ToString()
    };

    public static string ToDisplayText(this AbilityEffect effect, AbilityTarget target, int value)
    {
        var t = target.ToDisplayText();
        return effect switch
        {
            AbilityEffect.BuffAttack => $"{t}의 공격력 +{value}",
            AbilityEffect.BuffHealth => $"{t}의 체력 +{value}",
            AbilityEffect.DealDamage => $"{t}에게 {value} 데미지",
            AbilityEffect.Heal       => $"{t}을 {value} 회복",
            AbilityEffect.SummonUnit => $"유닛 {value}개 소환",
            _                        => effect.ToString()
        };
    }
}
