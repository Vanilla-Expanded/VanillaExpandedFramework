using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PipeSystem
{
    [StaticConstructorOnStartup]
    public static class AutoLinkRefuelables
    {
        static AutoLinkRefuelables()
        {
            List<ThingDef> refuelablesThing = new List<ThingDef>();
            List<ThingDef> things = DefDatabase<ThingDef>.AllDefsListForReading;
            // Get all things that are refuelable
            foreach (var thing in things)
            {
                if (thing.HasComp<CompRefuelable>())
                {
                    refuelablesThing.Add(thing);
                }
            }

            List<PipeNetDef> netDefs = DefDatabase<PipeNetDef>.AllDefsListForReading;
            // Find all nets
            for (int i = 0; i < netDefs.Count; i++)
            {
                PipeNetDef netDef = netDefs[i];
                // If net should refuel anything
                if (netDef.linkToRefuelables == null)
                    continue;

                // Loop through all thing net should refuel
                for (int l = 0; l < netDef.linkToRefuelables.Count; l++)
                {
                    LinkOption linkOption = netDef.linkToRefuelables[l];
                    // Loop through all refuelable things
                    for (int t = 0; t < refuelablesThing.Count; t++)
                    {
                        var extension = refuelablesThing[t].GetModExtension<AutoRefuelableLinkingExtension>();
                        // If refuelable should link with this specific net def
                        if (extension?.disabledAutoLinkingNetDefs != null && extension.disabledAutoLinkingNetDefs.Contains(netDef))
                            continue;

                        // Check if CompProperties_RefillWithPipes exists already that refills the same exact thing
                        var canRefill = true;
                        for (int m = 0; m < refuelablesThing[t].comps.Count; m++)
                        {
                            if (refuelablesThing[t].comps[m] is CompProperties_RefillWithPipes compRR && compRR.thing == linkOption.thing)
                            {
                                canRefill = false;
                                break;
                            }
                        }

                        if (!canRefill)
                            continue;

                        // If it don't already have a CompProperties_RefillRefuelable set to use the same thing
                        CompProperties_Refuelable comp = refuelablesThing[t].GetCompProperties<CompProperties_Refuelable>();
                        if (comp.fuelFilter.AllowedThingDefs.Contains(linkOption.thing))
                        {
                            // If fuelfilter can accept the thing. Add a new CompProperties_RefillRefuelable
                            refuelablesThing[t].comps.Add(new CompProperties_RefillWithPipes
                            {
                                pipeNet = netDef,
                                thing = linkOption.thing,
                                ratio = linkOption.ratio
                            });
                        }
                    }
                }
            }
        }
    }
}
