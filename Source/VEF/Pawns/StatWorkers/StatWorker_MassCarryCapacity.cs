using RimWorld;
using Verse;

namespace VEF.Pawns
{
    public class StatWorker_MassCarryCapacity : StatWorker
    {
        public override float GetBaseValueFor(StatRequest request)
        {
            float __result = base.GetBaseValueFor(request);
            if (request.Thing is Pawn pawn)
            {
                VanillaExpandedFramework_MassUtility_Capacity_Patch.includeStatWorkerResult = false;
                __result += MassUtility.Capacity(pawn);
                VanillaExpandedFramework_MassUtility_Capacity_Patch.includeStatWorkerResult = true;
            }
            return __result;
        }
    }
}