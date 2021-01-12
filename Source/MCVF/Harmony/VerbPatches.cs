using HarmonyLib;
using MVCF.Comps;
using Verse;

namespace MVCF.Harmony
{
    [HarmonyPatch(typeof(Verb))]
    public class VerbPatches
    {
        // ReSharper disable once InconsistentNaming

//    [HarmonyPatch("OrderForceTarget")]
// [HarmonyPrefix]
        public static bool Prefix_OrderForceTarget(LocalTargetInfo target, Verb __instance)
        {
            var num = __instance.verbProps.EffectiveMinRange(target, __instance.CasterPawn);
            if (__instance.verbProps.IsMeleeAttack ||
                __instance.CasterPawn.Position.DistanceToSquared(target.Cell) < num * (double) num &&
                __instance.CasterPawn.Position.AdjacentTo8WayOrInside(target.Cell))
                return true;

            __instance.TryStartCastOn(target);
            return false;
        }

        [HarmonyPatch("get_EquipmentSource")]
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming
        public static void Postfix_EquipmentSource(ref ThingWithComps __result, Verb __instance)
        {
            switch (__instance.DirectOwner)
            {
                case Comp_VerbGiver giver:
                    __result = giver.parent;
                    break;
                case HediffComp_VerbGiver giver2:
                    __result = null;
                    break;
                case Pawn pawn:
                    __result = pawn;
                    break;
                case VerbManager vm:
                    __result = vm.Pawn;
                    break;
            }
        }
    }
}