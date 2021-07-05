using HarmonyLib;
using MVCF.Comps;
using MVCF.Utilities;
using Verse;

// ReSharper disable InconsistentNaming

namespace MVCF.Harmony
{
    public class VerbPatches
    {
        public static void DoPatches(HarmonyLib.Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(Verb), "OrderForceTarget"),
                new HarmonyMethod(typeof(VerbPatches), "Prefix_OrderForceTarget"));
            harm.Patch(AccessTools.Method(typeof(Verb), "get_EquipmentSource"),
                new HarmonyMethod(typeof(VerbPatches), "Prefix_EquipmentSource"));
        }

        public static void DoIndependentPatches(HarmonyLib.Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(Verb), "get_Caster"),
                postfix: new HarmonyMethod(typeof(VerbPatches), "Postfix_get_Caster"));
            harm.Patch(AccessTools.Method(typeof(Verb), "get_CasterPawn"),
                postfix: new HarmonyMethod(typeof(VerbPatches), "Postfix_get_CasterPawn"));
            harm.Patch(AccessTools.Method(typeof(Verb), "get_CasterIsPawn"),
                postfix: new HarmonyMethod(typeof(VerbPatches), "Postfix_get_CasterIsPawn"));
        }

        public static bool Prefix_OrderForceTarget(LocalTargetInfo target, Verb __instance)
        {
            if (__instance.verbProps.IsMeleeAttack || !__instance.CasterIsPawn)
                return true;
            if (Base.IsIgnoredMod(__instance.EquipmentSource == null
                ? __instance.HediffCompSource?.parent?.def?.modContentPack?.Name
                : __instance.EquipmentSource.def?.modContentPack?.Name)) return true;
            var man = __instance.CasterPawn.Manager();
            if (man == null) return true;
            var mv = man.GetManagedVerbForVerb(__instance);
            if (mv != null) mv.Enabled = true;
            if (mv is TurretVerb tv)
            {
                tv.SetTarget(target);
                return false;
            }

            if (man.debugOpts.VerbLogging)
                Log.Message("Changing CurrentVerb of " + __instance.CasterPawn + " to " + __instance);
            man.CurrentVerb = __instance;

            return true;
        }

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

        public static void Postfix_get_Caster(ref Thing __result)
        {
            if (__result is IFakeCaster caster) __result = caster.RealCaster();
        }

        public static void Postfix_get_CasterPawn(ref Pawn __result, Verb __instance)
        {
            if (__instance.caster is IFakeCaster caster) __result = caster.RealCaster() as Pawn;
        }

        public static void Postfix_get_CasterIsPawn(ref bool __result, Verb __instance)
        {
            if (__instance.caster is IFakeCaster caster) __result = caster.RealCaster() is Pawn;
        }
    }
}