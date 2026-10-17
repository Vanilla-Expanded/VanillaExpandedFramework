using System.Collections.Generic;
using HarmonyLib;
using VEF.AnimalBehaviours;
using VEF.Weapons;
using Verse;

namespace VEF;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
public static class VanillaExpandedFramework_Pawn_GetGizmos_Patch
{
    private static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> result, Pawn __instance)
    {
        result = AnimalBehavioursPawnGizmos.GetGizmos(__instance, result);
        result = WeaponsPawnGizmos.GetGizmos(__instance, result);

        return result;
    }
}