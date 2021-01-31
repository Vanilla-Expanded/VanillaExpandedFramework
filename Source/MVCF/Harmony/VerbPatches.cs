using HarmonyLib;
using MVCF.Comps;
using MVCF.Utilities;
using Verse;

namespace MVCF.Harmony
{
    [HarmonyPatch(typeof(Verb))]
    public class VerbPatches
    {
        // ReSharper disable once InconsistentNaming

        [HarmonyPatch("OrderForceTarget")]
        [HarmonyPrefix]
        public static void Prefix_OrderForceTarget(LocalTargetInfo target, Verb __instance)
        {
            if (__instance.verbProps.IsMeleeAttack || !__instance.CasterIsPawn)
                return;
            var man = __instance.CasterPawn.Manager();
            if (man == null) return;
            if (man.debugOpts.VerbLogging)
                Log.Message("Changing CurrentVerb of " + __instance.CasterPawn + " to " + __instance);
            man.CurrentVerb = __instance;
            var mv = man.GetManagedVerbForVerb(__instance);
            if (mv != null) mv.Enabled = true;
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
                case HediffComp_VerbGiver _:
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