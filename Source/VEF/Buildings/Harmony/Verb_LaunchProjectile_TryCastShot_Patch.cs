using HarmonyLib;
using Verse;

namespace VEF.Buildings;

[HarmonyPatch(typeof(Verb_LaunchProjectile), "TryCastShot")]
public static class Verb_LaunchProjectile_TryCastShot_Patch
{
    public static void Postfix(Verb_LaunchProjectile __instance, bool __result)
    {
        if (!__result)
        {
            return;
        }
        var dualFuelComp = __instance.caster?.TryGetComp<CompRefuelable_DualFuel>();
        if (dualFuelComp != null)
        {
            dualFuelComp.ConsumeSecondaryFuel(1);
        }
    }
}