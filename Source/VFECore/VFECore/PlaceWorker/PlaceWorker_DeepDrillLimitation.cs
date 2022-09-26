using RimWorld;
using Verse;

namespace VFECore
{
    public class PlaceWorker_DeepDrillLimitation : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            var res = DeepDrillUtility.GetNextResource(loc, map);
            var ext = res.GetModExtension<ThingDefExtension>();

            if (ext != null && !ext.allowDeepDrill)
                return (AcceptanceReport)"VFE_DeepDrillNo".Translate(res.label);

            return true;
        }
    }
}
