using System.Collections.Generic;
using UnityEngine;
using VEF.CacheClearing;
using VEF.Maps;
using Verse;

namespace PipeSystem;

public class GenericPipeNetOverlayWorker : CustomOverlayWorker
{
    public Dictionary<Thing, Material> OverlayForThing = [];

    public GenericPipeNetOverlayWorker(CustomOverlayDef def) : base(def)
    {
        // Clear the list of overlays on new game/game load
        ClearCaches.OnClearCache += instances => instances.Add(this);
    }

    public override Material MaterialForThing(Thing thing) => OverlayForThing[thing];
}