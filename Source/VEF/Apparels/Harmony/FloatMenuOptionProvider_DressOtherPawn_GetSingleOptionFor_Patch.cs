using Verse;
using RimWorld;
using HarmonyLib;

namespace VEF.Apparels
{
    [HarmonyPatch(typeof(FloatMenuOptionProvider_DressOtherPawn), "GetSingleOptionFor")]
    public static class VanillaExpandedFramework_FloatMenuOptionProvider_DressOtherPawn_GetSingleOptionFor_Patch
    {
        public static void Postfix(ref FloatMenuOption __result, Thing clickedThing, FloatMenuContext context)
        {
            if (__result != null && clickedThing is Apparel_Shield shield)
            {
                __result = null;
            }
        }
    }
}
