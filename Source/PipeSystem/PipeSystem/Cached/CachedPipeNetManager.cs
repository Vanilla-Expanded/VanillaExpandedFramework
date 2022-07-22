using System.Collections.Generic;
using Verse;

namespace PipeSystem
{
    public static class CachedPipeNetManager
    {
        public static Dictionary<int, PipeNetManager> managerCache = new Dictionary<int, PipeNetManager>();

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
    }
}
