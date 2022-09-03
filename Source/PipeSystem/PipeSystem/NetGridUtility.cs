using System.Collections.Generic;
using Verse;

namespace PipeSystem
{
    public static class NetGridUtility
    {
        public static Building GetNetTransmitter(this IntVec3 c, Map map, Thing thing)
        {
            List<Thing> thingList = map.thingGrid.ThingsListAt(c);
            var comps = CachedResourceThings.GetFor(thing.def);

            if (comps == null)
                return null;

            for (int i = 0; i < thingList.Count; ++i)
            {
                var oThing = thingList[i];
                var oComps = CachedResourceThings.GetFor(oThing.def);

                if (oComps == null)
                    continue;

                for (int o = 0; o < oComps.Count; o++)
                {
                    if (comps.Any(cp => cp.Resource == oComps[o].Resource))
                        return (Building)oThing;
                }
            }
            return null;
        }
    }
}
