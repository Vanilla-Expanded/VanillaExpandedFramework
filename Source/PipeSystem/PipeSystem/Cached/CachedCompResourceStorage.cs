using System.Collections.Generic;
using Verse;

namespace PipeSystem
{
    public static class CachedCompResourceStorage
    {
        public static Dictionary<Thing, CompResourceStorage> cachedCompResourceStorage = new Dictionary<Thing, CompResourceStorage>();

        public static void Cache(CompResourceStorage comp)
        {
            var thing = comp.parent;
            if (!cachedCompResourceStorage.ContainsKey(thing))
                cachedCompResourceStorage.Add(thing, comp);
        }

        public static CompResourceStorage GetFor(Thing thing)
        {
            if (!cachedCompResourceStorage.ContainsKey(thing))
            {
                var comp = thing.TryGetComp<CompResourceStorage>();
                cachedCompResourceStorage.Add(thing, comp);
                return comp;
            }

            return cachedCompResourceStorage[thing];
        }
    }
}
