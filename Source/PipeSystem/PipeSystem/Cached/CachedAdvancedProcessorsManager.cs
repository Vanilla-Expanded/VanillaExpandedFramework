using System.Collections.Generic;
using RuntimeAudioClipLoader;
using Verse;

namespace PipeSystem
{
    [StaticConstructorOnStartup]
    public static class CachedAdvancedProcessorsManager
    {
        private static readonly Dictionary<int, AdvancedProcessorsManager> managerCache = new Dictionary<int, AdvancedProcessorsManager>();

        /// <summary>
        /// Get the AdvancedProcessorsManager of a map
        /// </summary>
        /// <param name="map">Map from wich we want the manager</param>
        /// <returns>AdvancedProcessorsManager</returns>
        public static AdvancedProcessorsManager GetFor(Map map)
        {
            if (!managerCache.ContainsKey(map.uniqueID))
            {
                var manager = map.GetComponent<AdvancedProcessorsManager>();
                managerCache.Add(map.uniqueID, manager);
                return manager;
            }
            return managerCache[map.uniqueID];
        }

        /// <summary>
        /// Clear dictionnary
        /// </summary>
        public static void Clear() => managerCache.Clear();
    }
}