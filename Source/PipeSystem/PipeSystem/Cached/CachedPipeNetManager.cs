using System.Collections.Generic;
using Verse;

namespace PipeSystem
{
    public static class CachedPipeNetManager
    {
        private static readonly Dictionary<int, PipeNetManager> managerCache = new Dictionary<int, PipeNetManager>();

        /// <summary>
        /// Get the PipeNetManager of a map
        /// </summary>
        /// <param name="map">Map from wich we want the manager</param>
        /// <returns>PipeNetManager</returns>
        public static PipeNetManager GetFor(Map map)
        {
            if (!managerCache.ContainsKey(map.uniqueID))
            {
                var manager = map.GetComponent<PipeNetManager>();
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
