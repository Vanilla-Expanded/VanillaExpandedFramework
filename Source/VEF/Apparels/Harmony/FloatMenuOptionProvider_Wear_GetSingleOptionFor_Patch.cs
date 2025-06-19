using Verse;
using RimWorld;
using HarmonyLib;

namespace VEF.Apparels
{

    [HarmonyPatch(typeof(FloatMenuOptionProvider_Wear), "GetSingleOptionFor")]
    public static class VanillaExpandedFramework_FloatMenuOptionProvider_Wear_GetSingleOptionFor_Patch
    {
        public static void Postfix(ref FloatMenuOption __result, Thing clickedThing, FloatMenuContext context)
        {
            if (clickedThing is Apparel_Shield shield)
            {
                TaggedString toCheck = "ForceWear".Translate(shield.LabelCap, shield);
                if (__result != null && __result.Label == toCheck)
                {
                    __result = null;
                }
            }
        }
    }
}
