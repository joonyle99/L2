public sealed class HeroInstance : ItemInstance<HeroData>
{
    public HeroInstance(HeroData data, int level = 1) : base(data)
    {
        CurrAttack = data.BaseAttack;
        CurrHealth = data.BaseHealth;
        CurrLevel = level;
    }

    public int CurrAttack { get; set; }
    public int CurrHealth { get; set; }
    public int CurrLevel { get; private set; }
    
    public void LevelUp()
    {
        CurrLevel++;
    }
    
    public void TakeDamage(int damage)
    {
        CurrHealth -= damage;
    }
}
