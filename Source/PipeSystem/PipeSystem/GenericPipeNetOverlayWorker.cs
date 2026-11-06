using System;
using System.Collections.Generic;
using UnityEngine;
using VEF.CacheClearing;
using VEF.Graphics;
using Verse;

namespace PipeSystem;

public class GenericPipeNetOverlayWorker : CustomOverlayWorker
{
    public Dictionary<Thing, Material> OverlayForThing = [];

    public GenericPipeNetOverlayWorker(CustomOverlayDef def) : base(def)
    {
        // Clear the list of overlays on new game/game load
        ClearCaches.OnClearCache += ClearCache;
    }

    public override Material MaterialForThing(Thing thing) => OverlayForThing[thing];

    public override void OnDefLost()
    {
        base.OnDefLost();
        // If the worker was replaced, we no longer need to worry about clearing caches anymore
        ClearCaches.OnClearCache -= ClearCache;
        OverlayForThing.Clear();
    }

    private void ClearCache(HashSet<object> instances) => instances.Add(this);
}