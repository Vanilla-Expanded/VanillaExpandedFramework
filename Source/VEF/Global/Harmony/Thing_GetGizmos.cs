using System.Collections.Generic;
using HarmonyLib;
using VEF.Things;
using Verse;

namespace VEF;

[HarmonyPatch(typeof(Thing), nameof(Thing.GetGizmos))]
public static class Thing_GetGizmos
{
    private static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> result, Thing __instance)
    {
        // No idea why vanilla extinguish gizmo is under thing gizmos rather than thing
        // comp gizmos, but let's just copy what it does anyway just in case it matters?
        result = CustomFireThingGizmos.GetGizmos(__instance, result);

        return result;
    }
}