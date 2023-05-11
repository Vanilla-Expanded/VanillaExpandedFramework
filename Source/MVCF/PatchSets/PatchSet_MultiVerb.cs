using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MVCF.Comps;
using MVCF.ModCompat;
using MVCF.Utilities;
using RimWorld;
using Verse;

namespace MVCF.PatchSets;

public class PatchSet_MultiVerb : PatchSet
{
    public override IEnumerable<Patch> GetPatches()
    {
        yield return new Patch(AccessTools.Method(typeof(Pawn), "TryGetAttackVerb"),
            AccessTools.Method(GetType(), nameof(TryGetAttackVerb_Prefix)),
            AccessTools.Method(GetType(), nameof(TryGetAttackVerb_Postfix)));
        yield return Patch.Prefix(AccessTools.Method(typeof(Verb), nameof(Verb.OrderForceTarget)),
            AccessTools.Method(GetType(), nameof(Prefix_OrderForceTarget)));
        yield return Patch.Prefix(AccessTools.PropertyGetter(typeof(Verb), nameof(Verb.EquipmentSource)),
            AccessTools.Method(GetType(), nameof(Prefix_EquipmentSource)));
        yield return Patch.Postfix(AccessTools.Method(typeof(BreachingUtility), nameof(BreachingUtility.FindVerbToUseForBreaching)),
            AccessTools.Method(GetType(), nameof(FindVerbToUseForBreaching)));
        yield return Patch.Postfix(AccessTools.Method(typeof(SlaveRebellionUtility), "CanApplyWeaponFactor"),
            AccessTools.Method(GetType(), nameof(CanApplyWeaponFactor)));
        yield return Patch.Prefix(AccessTools.Method(typeof(Targeter), "GetTargetingVerb"), AccessTools.Method(GetType(), nameof(Prefix_GetTargetingVerb)));
    }

    public static bool Prefix_GetTargetingVerb(Pawn pawn, Targeter __instance, ref Verb __result)
    {
        if (pawn.Manager(false) is not { } man) return true;
        __result = man.AllVerbs.FirstOrDefault(verb => verb.verbProps == __instance.targetingSource.GetVerb.verbProps);
        return false;
    }

    public static bool Prefix_OrderForceTarget(LocalTargetInfo target, Verb __instance)
    {
        if (__instance.verbProps.IsMeleeAttack || !__instance.CasterIsPawn)
            return true;
        if (MVCF.IsIgnoredMod(__instance.EquipmentSource == null
                ? __instance.HediffCompSource?.parent?.def?.modContentPack?.Name
                : __instance.EquipmentSource.def?.modContentPack?.Name)) return true;
        var man = __instance.CasterPawn.Manager();
        if (man == null) return true;
        var mv = __instance.Managed(false);
        if (mv != null) mv.Enabled = true;

        if (mv != null && !mv.SetTarget(target)) return false;

        if (DualWieldCompat.Active && __instance.CasterPawn.RaceProps.Humanlike && __instance.CasterPawn.GetOffHand() is { } eq
         && eq.TryGetComp<CompEquippable>().PrimaryVerb is { } verb &&
            verb == __instance) return true;

        MVCF.LogFormat($"Changing CurrentVerb of {__instance.CasterPawn} to {__instance}", LogLevel.Info);
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

    public static void CanApplyWeaponFactor(ref bool __result, Pawn pawn)
    {
        if (!__result && (pawn.Manager()?.AllVerbs.Except(pawn.verbTracker.AllVerbs).Any() ?? false)) __result = true;
    }

    public static void FindVerbToUseForBreaching(ref Verb __result, Pawn pawn)
    {
        if (__result == null && pawn.Manager() is { } man)
            __result = man.AllVerbs.FirstOrDefault(v => v.Available() && v.HarmsHealth() && v.verbProps.ai_IsBuildingDestroyer);
    }

    public static bool TryGetAttackVerb_Prefix(ref Verb __result, Pawn __instance, Thing target,
        out bool __state, bool allowManualCastWeapons = false)
    {
        __result = __instance.GetAttackVerb(target, allowManualCastWeapons);
        return __state = __result == null;
    }

    public static void TryGetAttackVerb_Postfix(ref Verb __result, bool __state)
    {
        // Just in case Vanilla chooses a disabled Verb, make sure it doesn't
        if (__state && __result?.Managed(false) is { Enabled: false }) __result = null;
    }
}
