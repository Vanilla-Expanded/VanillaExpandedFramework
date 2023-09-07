using System.Collections.Generic;
using Verse;

namespace PipeSystem
{
    public static class CachedCompAdvancedProcessor
    {
        private static readonly Dictionary<Thing, CompAdvancedResourceProcessor> cachedCompAdvancedProcessor = new Dictionary<Thing, CompAdvancedResourceProcessor>();

        /// <summary>
        /// Add CompAdvancedResourceProcessor and it's parent to cache.
        /// </summary>
        /// <param name="comp"></param>
        public static void Cache(CompAdvancedResourceProcessor comp)
        {
            var thing = comp.parent;
            if (!cachedCompAdvancedProcessor.ContainsKey(thing))
                cachedCompAdvancedProcessor.Add(thing, comp);
        }

        /// <summary>
        /// Get CompAdvancedResourceProcessor of a thing from cache.
        /// Faster than iterating over all comps to find CompAdvancedResourceProcessor.
        /// </summary>
        /// <param name="thing">Thing from wich we want the comp</param>
        /// <returns>CompAdvancedResourceProcessor</returns>
        public static CompAdvancedResourceProcessor GetFor(Thing thing)
        {
            if (!cachedCompAdvancedProcessor.ContainsKey(thing))
            {
                var comp = thing.TryGetComp<CompAdvancedResourceProcessor>();
                cachedCompAdvancedProcessor.Add(thing, comp);
                return comp;
            }

            return cachedCompAdvancedProcessor[thing];
        }

        /// <summary>
        /// Clear dictionnary
        /// </summary>
        public static void Clear() => cachedCompAdvancedProcessor.Clear();
    }
}
