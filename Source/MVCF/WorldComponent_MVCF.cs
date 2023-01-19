using System;
using System.Collections.Generic;
using RimWorld.Planet;

namespace MVCF;

public class WorldComponent_MVCF : WorldComponent
{
    public static WorldComponent_MVCF Instance;

    public readonly List<WeakReference<VerbManager>> TickManagers = new();

    public WorldComponent_MVCF(World world) : base(world) => Instance = this;

    public override void WorldComponentTick()
    {
        base.WorldComponentTick();
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < TickManagers.Count; i++)
            if (TickManagers[i].TryGetTarget(out var man))
                man.Tick();
    }
}
