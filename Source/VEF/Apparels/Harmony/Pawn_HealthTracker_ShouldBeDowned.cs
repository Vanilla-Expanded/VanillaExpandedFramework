using HarmonyLib;
using Verse;

namespace VEF.Apparels
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "ShouldBeDowned")]
    public static class VanillaExpandedFramework_Pawn_HealthTracker_ShouldBeDowned_Patch
    {
        private static bool Prefix(Pawn ___pawn)
        {
            if (___pawn?.apparel?.WornApparel != null)
            {
                foreach (var apparel in ___pawn.apparel.WornApparel)
                {
                    var extension = apparel.def.GetModExtension<ApparelExtension>();
                    if (extension != null && extension.preventDowning)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}