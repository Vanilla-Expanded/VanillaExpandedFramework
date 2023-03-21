using System.Linq;
using HarmonyLib;
using MVCF.Utilities;
using RimWorld;
using Verse;

namespace MVCF;

public class Patch_Pawn_TryGetAttackVerb
{
    public static Patch GetPatch() =>
        new(AccessTools.Method(typeof(Pawn), "TryGetAttackVerb"), AccessTools.Method(typeof(Patch_Pawn_TryGetAttackVerb), nameof(Prefix)),
            AccessTools.Method(typeof(Patch_Pawn_TryGetAttackVerb), nameof(Postfix)));

    public static Verb AttackVerb(Pawn pawn, Thing target, bool allowManualCastWeapons = false)
    {
        var manager = pawn.Manager();
        var job = pawn.CurJob;

        MVCF.LogFormat($"AttackVerb of {pawn} on target {target} with job {job} that has target {job?.targetA} and CurrentVerb {manager.CurrentVerb}",
            LogLevel.Important);

        if (manager.CurrentVerb != null && manager.CurrentVerb.Available() &&
            (target == null || manager.CurrentVerb.CanHitTarget(target)) &&
            (job is not { targetA: { IsValid: true, Cell: var cell } } || cell == pawn.Position || !cell.InBounds(pawn.Map) ||
             manager.CurrentVerb.CanHitTarget(job.targetA)))
            return manager.CurrentVerb;

        var verbs = manager.CurrentlyUseableRangedVerbs;
        if (!allowManualCastWeapons && job != null && job.def == JobDefOf.Wait_Combat)
            verbs = verbs.Where(v => !v.Verb.verbProps.onlyManualCast);

        var verbsToUse = verbs.ToList();
        var usedTarget = target ?? job?.targetA ?? LocalTargetInfo.Invalid;

        MVCF.LogFormat($"Getting best verb for target {target} or {job?.targetA} which is {usedTarget} from {verbsToUse.Count} choices", LogLevel.Important);

        if (!usedTarget.IsValid || !usedTarget.Cell.InBounds(pawn.Map)) return null;
        if (verbsToUse.Count == 0) return null;
        if (verbsToUse.Count == 1) return verbsToUse[0].Verb;

        return pawn.BestVerbForTarget(usedTarget, verbsToUse);
    }

    public static bool Prefix(ref Verb __result, Pawn __instance, Thing target,
        out bool __state, bool allowManualCastWeapons = false)
    {
        __result = AttackVerb(__instance, target, allowManualCastWeapons);
        return __state = __result == null;
    }

    public static void Postfix(ref Verb __result, bool __state)
    {
        // Just in case Vanilla chooses a disabled Verb, make sure it doesn't
        if (__state && __result?.Managed(false) is { Enabled: false }) __result = null;
    }
}
