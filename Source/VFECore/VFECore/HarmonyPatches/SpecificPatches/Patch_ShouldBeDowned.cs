using HarmonyLib;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "ShouldBeDowned")]
    public static class Patch_ShouldBeDowned
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