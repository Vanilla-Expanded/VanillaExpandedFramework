using HarmonyLib;
using RimWorld;

namespace PipeSystem;

/// <summary>
/// Turret is inactive (won't target or shoot) while <see cref="CompTurretNeedsResource"/> reports
/// its resources are off. Vanilla Building_TurretGun.ClaimableBy also uses this for non-manned turrets.
/// </summary>
[HarmonyPatch(typeof(Building_TurretGun), nameof(Building_TurretGun.Active), MethodType.Getter)]
public static class Building_TurretGun_Active_Postfix
{
    private static bool Prepare() => CompProperties_TurretNeedsResource.AnyDefUsesComp;

    private static void Postfix(Building_TurretGun __instance, ref bool __result)
    {
        if (__result && __instance.ResourceNeed is { ResourceActive: false })
            __result = false;
    }
}

/// <summary>
/// Turret isn't treated as a threat while <see cref="CompTurretNeedsResource"/> reports its resources are off,
/// matching how vanilla handles unpowered turrets.
/// </summary>
[HarmonyPatch(typeof(Building_Turret), nameof(Building_Turret.ThreatDisabled))]
public static class Building_Turret_ThreatDisabled_Postfix
{
    private static bool Prepare() => CompProperties_TurretNeedsResource.AnyDefUsesComp;

    private static void Postfix(Building_Turret __instance, ref bool __result)
    {
        if (!__result && __instance.ResourceNeed is { ResourceActive: false })
            __result = true;
    }
}