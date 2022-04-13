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
                if (t != thingToIgnore && t != null)
                {
                    if ((CachedResourceThings.resourceCompsOf.ContainsKey(t.def)
                         && !Accept(t.def, pipeNet))
                        || (t.def.entityDefToBuild is ThingDef thingDef
                            && CachedResourceThings.resourceCompsOf.ContainsKey(thingDef)
                            && !Accept(thingDef, pipeNet)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool Accept(ThingDef thingDef, PipeNetDef pipeNetDef)
        {
            var props = CachedResourceThings.resourceCompsOf[thingDef];
            for (int i = 0; i < props.Count; i++)
            {
                var prop = props[i];
                if (prop.pipeNet == pipeNetDef)
                    return false;
            }
            return true;
        }
    }
}