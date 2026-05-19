using System;
using UnityEngine;
using JoonyleGameDevKit;

public sealed class HeroSlotController : BenchSlotController<HeroInstance, HeroView, HeroSlot>
{
    public void Initialize(
        IBenchProvider<HeroInstance> benchProvider,
        Func<HeroInstance, Vector3, Action, bool> onSpawnEffect = null,
        Action<HeroInstance, Vector3> onHoverEnter = null,
        Action onHoverExit = null)
    {
        InitializeBench(benchProvider, onSpawnEffect, onHoverEnter, onHoverExit);
    }

    protected override ItemSlot CreateSlot() => new HeroSlot();
    protected override void SetupView(HeroSlot slot, HeroInstance instance)
    {
        slot.HeroView.Setup(instance, itemOffset);
        slot.HeroView.transform.localPosition = itemOffset.ToVector3();
    }
}
