using HarmonyLib;
using System.Linq;
using Verse;
using VEF.Things;

namespace VEF.Pawns
{
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class VanillaExpandedFramework_Pawn_Kill
    {
       
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