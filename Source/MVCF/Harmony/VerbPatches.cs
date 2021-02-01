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
        public static bool Prefix_OrderForceTarget(LocalTargetInfo target, Verb __instance)
        {
            if (__instance.verbProps.IsMeleeAttack || !__instance.CasterIsPawn)
                return true;
            var man = __instance.CasterPawn.Manager();
            if (man == null) return true;
            if (man.debugOpts.VerbLogging)
                Log.Message("Changing CurrentVerb of " + __instance.CasterPawn + " to " + __instance);
            man.CurrentVerb = __instance;
            var mv = man.GetManagedVerbForVerb(__instance);
            if (mv != null) mv.Enabled = true;
            if (mv is TurretVerb tv)
            {
                tv.SetTarget(target);
                return false;
            }

            return true;
        }

        [HarmonyPatch("get_EquipmentSource")]
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming
        public static bool Prefix_EquipmentSource(ref ThingWithComps __result, Verb __instance)
        {
            if (__instance == null) // Needed to work with A Rimworld of Magic, for some reason
            {
                Log.Warning("[MVCF] Instance in patch is null. This is not supported.");
                __result = null;
                return false;
            }

            switch (__instance.DirectOwner)
            {
                case Comp_VerbGiver giver:
                    __result = giver.parent;
                    return false;
                case HediffComp_VerbGiver _:
                    __result = null;
                    return false;
                case Pawn pawn:
                    __result = pawn;
                    return false;
                case VerbManager vm:
                    __result = vm.Pawn;
                    return false;
            }

            return true;
        }

        [HarmonyPatch("get_Caster")]
        [HarmonyPostfix]
        public static void Postfix_get_Caster(ref Thing __result)
        {
            if (__result is IFakeCaster caster) __result = caster.RealCaster();
        }

        [HarmonyPatch("get_CasterPawn")]
        [HarmonyPostfix]
        public static void Postfix_get_CasterPawn(ref Pawn __result, Verb __instance)
        {
            if (__instance.caster is IFakeCaster caster) __result = caster.RealCaster() as Pawn;
        }

        [HarmonyPatch("get_CasterIsPawn")]
        [HarmonyPostfix]
        public static void Postfix_get_CasterIsPawn(ref bool __result, Verb __instance)
        {
            if (__instance.caster is IFakeCaster caster) __result = caster.RealCaster() is Pawn;
        }
    }
}