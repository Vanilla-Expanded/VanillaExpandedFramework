using System.Collections.Generic;
using Verse;

namespace PipeSystem
{
    public static class CachedCompResourceStorage
    {
        private static readonly Dictionary<Thing, CompResourceStorage> cachedCompResourceStorage = new Dictionary<Thing, CompResourceStorage>();

        /// <summary>
        /// Add CompResourceStorage and it's parent to cache.
        /// </summary>
        /// <param name="comp"></param>
        public static void Cache(CompResourceStorage comp)
        {
            var thing = comp.parent;
            if (!cachedCompResourceStorage.ContainsKey(thing))
                cachedCompResourceStorage.Add(thing, comp);
        }

        /// <summary>
        /// Get CompResourceStorage of a thing from cache.
        /// Faster than iterating over all comps to find CompResourceStorage.
        /// </summary>
        /// <param name="thing">Thing from wich we want the comp</param>
        /// <returns>CompResourceStorage</returns>
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

        /// <summary>
        /// Clear dictionnary
        /// </summary>
        public static void Clear() => cachedCompResourceStorage.Clear();
    }
}
