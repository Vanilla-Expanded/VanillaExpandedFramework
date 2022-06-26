using System.Collections.Generic;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace VFECore.Abilities
{
    [HarmonyPatch(typeof(Caravan), nameof(Caravan.GetGizmos))]
    public static class Caravan_GetGizmos_Patch
    {
        [HarmonyPostfix]
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> gizmos, Caravan __instance)
        {
            foreach (var gizmo in gizmos) yield return gizmo;

            foreach (var pawn in __instance.pawns)
                if (pawn.TryGetComp<CompAbilities>() is CompAbilities comp)
                    foreach (var ability in comp.LearnedAbilities)
                        if (ability.def.showGizmoOnWorldView && ability.ShowGizmoOnPawn())
                            yield return ability.GetGizmo();
        }
    }
}