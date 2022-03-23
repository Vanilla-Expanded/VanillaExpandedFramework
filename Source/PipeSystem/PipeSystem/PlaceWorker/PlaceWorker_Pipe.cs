using System.Collections.Generic;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Disallow placing resource pipe on a transmitter of the same resource.
    /// </summary>
    public class PlaceWorker_Pipe : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            PipeNetDef pipeNet = ((ThingDef)checkingDef).GetCompProperties<CompProperties_Resource>().pipeNet;

            List<Thing> thingList = loc.GetThingList(map);
            for (int i = 0; i < thingList.Count; i++)
            {
                var t = thingList[i];
                if (CachedResourceThings.firstCompOf.ContainsKey(t.def))
                {
                    var props = CachedResourceThings.firstCompOf[t.def];
                    return props.pipeNet != pipeNet;
                }
                if (t.def.entityDefToBuild is ThingDef thingDef && CachedResourceThings.firstCompOf.ContainsKey(thingDef))
                {
                    var props = CachedResourceThings.firstCompOf[thingDef];
                    return props.pipeNet != pipeNet;
                }
            }
            return true;
        }
    }
}