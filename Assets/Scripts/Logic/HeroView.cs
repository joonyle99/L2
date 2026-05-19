using UnityEngine;

public class HeroView : ItemView
{
    [SerializeField] private HeroStats _heroStats;

    private HeroInstance _heroInstance;
    public HeroInstance HeroInstance => _heroInstance;
    public override ItemInstance ItemInstance => _heroInstance;

    public void Setup(HeroInstance heroInstance, Vector3 spawnOffset)
    {
        _heroInstance = heroInstance;
        ApplyItem(heroInstance.Data.Prefab, spawnOffset);
        Refresh();
    }

    public void Refresh()
    {
        _heroStats.SetAttackText(_heroInstance.Attack);
        _heroStats.SetHealthText(_heroInstance.Health);
    }

    public void Clear()
    {
        ClearItem();
        _heroInstance = null;
    }
}
