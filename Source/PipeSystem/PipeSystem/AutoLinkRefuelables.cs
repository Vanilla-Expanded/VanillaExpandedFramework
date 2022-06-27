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
                if (thing.HasComp(typeof(CompRefuelable)))
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
                if (netDef.linkToRefuelables != null)
                {
                    // Loop through all thing net should refuel
                    for (int l = 0; l < netDef.linkToRefuelables.Count; l++)
                    {
                        LinkOption linkOption = netDef.linkToRefuelables[l];
                        // Loop through all refuelable things
                        for (int t = 0; t < refuelablesThing.Count; t++)
                        {
                            CompProperties_RefillWithPipes compRR = refuelablesThing[t].GetCompProperties<CompProperties_RefillWithPipes>();
                            if (compRR == null || compRR.thing != linkOption.thing)
                            {
                                // If it don't already have a CompProperties_RefillRefuelable set to use the same thing
                                CompProperties_Refuelable comp = refuelablesThing[t].GetCompProperties<CompProperties_Refuelable>();
                                if (comp.fuelFilter.AllowedThingDefs.Contains(linkOption.thing))
                                {
                                    // If fuelfilter can accept the thing. Add a new CompProperties_RefillRefuelable
                                    DefDatabase<ThingDef>.GetNamed(refuelablesThing[t].defName).comps.Add(new CompProperties_RefillWithPipes
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
    }
}
