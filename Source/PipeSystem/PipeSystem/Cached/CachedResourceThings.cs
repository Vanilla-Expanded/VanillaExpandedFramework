using System.Collections.Generic;
using Verse;

namespace PipeSystem
{
    [StaticConstructorOnStartup]
    public static class CachedResourceThings
    {
        public static readonly Dictionary<ThingDef, List<CompProperties_Resource>> resourceCompsOf = new Dictionary<ThingDef, List<CompProperties_Resource>>();

        static CachedResourceThings()
        {
            var things = DefDatabase<ThingDef>.AllDefsListForReading;
            for (int i = 0; i < things.Count; i++)
            {
                var thing = things[i];
                var comps = thing.comps;

                for (int o = 0; o < comps.Count; o++)
                {
                    var comp = comps[o];
                    if (comp is CompProperties_Resource cpR)
                    {
                        if (!resourceCompsOf.ContainsKey(thing))
                        {
                            resourceCompsOf.Add(thing, new List<CompProperties_Resource>() { cpR });
                        }
                        else
                        {
                            resourceCompsOf[thing].Add(cpR);
                        }
                    }
                }
            }
        }

        public static List<CompProperties_Resource> GetFor(ThingDef thingDef)
        {
            if (resourceCompsOf.ContainsKey(thingDef))
                return resourceCompsOf[thingDef];

            return null;
        }
    }
}
