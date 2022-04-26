using HarmonyLib;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Patch_Kill
    {
        private static bool Prefix(Pawn __instance)
        {
            if (__instance?.apparel?.WornApparel != null)
            {
                foreach (var apparel in __instance.apparel.WornApparel)
                {
                    var extension = apparel.def.GetModExtension<ApparelExtension>();
                    if (extension != null && extension.preventKilling)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static void Postfix(Pawn __instance)
        {
            if (__instance.Dead)
            {
                var extension = __instance.def.GetModExtension<ThingDefExtension>();
                if (extension != null && extension.destroyCorpse)
                {
                    if (__instance.Corpse != null && !__instance.Corpse.Destroyed)
                    {
                        __instance.Corpse.Destroy();
                    }
                }
            }
        }
    }
}