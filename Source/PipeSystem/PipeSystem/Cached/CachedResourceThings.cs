using System.Collections.Generic;
using Verse;

namespace PipeSystem
{
    [StaticConstructorOnStartup]
    public static class CachedResourceThings
    {
        public static readonly Dictionary<ThingDef, CompProperties_Resource> firstCompOf = new Dictionary<ThingDef, CompProperties_Resource>();

        static CachedResourceThings()
        {
            var things = DefDatabase<ThingDef>.AllDefsListForReading;
            for (int i = 0; i < things.Count; i++)
            {
                var thing = things[i];
                if (thing.GetCompProperties<CompProperties_Resource>() is CompProperties_Resource cpR)
                {
                    firstCompOf.Add(thing, cpR);
                }
            }
        }
    }
}
