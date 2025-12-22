using HarmonyLib;
using Verse;

namespace VEF.Buildings;


[HarmonyPatch(typeof(Verb), "Available")]
public static class Verb_Available_Patch
{
    public static void Postfix(Verb __instance, ref bool __result)
    {
        if (!__result)
        {
            return;
        }
        var dualFuelComp = __instance.caster?.TryGetComp<CompRefuelable_DualFuel>();
        if (dualFuelComp != null)
        {
            if (!dualFuelComp.HasFuel || !dualFuelComp.HasSecondaryFuel)
            {
                __result = false;
            }
        }
    }
}