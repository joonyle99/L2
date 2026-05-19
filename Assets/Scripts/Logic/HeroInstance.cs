public sealed class HeroInstance : ItemInstance<HeroData>
{
    public HeroInstance(HeroData data, int level = 1) : base(data)
    {
        Attack = data.BaseAttack;
        Health = data.BaseHealth;
        Level = level;
    }

    public int Attack { get; set; }
    public int Health { get; set; }
    public int Level { get; private set; }
    
    public void LevelUp()
    {
        Level++;
    }
}
