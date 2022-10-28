using System.Linq;
using HarmonyLib;
using MVCF.Utilities;
using RimWorld;
using Verse;

namespace MVCF;

public class Patch_Pawn_TryGetAttackVerb
{
    public static Patch GetPatch() =>
        Patch.Prefix(AccessTools.Method(typeof(Pawn), "TryGetAttackVerb"), AccessTools.Method(typeof(Patch_Pawn_TryGetAttackVerb), nameof(Prefix)));

    public static Verb AttackVerb(Pawn pawn, Thing target, bool allowManualCastWeapons = false)
    {
        var manager = pawn.Manager();
        var job = pawn.CurJob;

        MVCF.Log($"AttackVerb of {pawn} on target {target} with job {job} that has target {job?.targetA} and CurrentVerb {manager.CurrentVerb}",
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

        if (verbsToUse.Count == 0) return null;

        var usedTarget = target ?? job?.targetA ?? LocalTargetInfo.Invalid;
        MVCF.Log($"Getting best verb for target {target} or {job?.targetA} which is {usedTarget}", LogLevel.Important);
        if (!usedTarget.IsValid || !usedTarget.Cell.InBounds(pawn.Map)) return null;
        return pawn.BestVerbForTarget(usedTarget, verbsToUse);
    }

    public static bool Prefix(ref Verb __result, Pawn __instance, Thing target,
        out bool __state, bool allowManualCastWeapons = false)
    {
        __result = AttackVerb(__instance, target, allowManualCastWeapons);
        return __state = __result == null;
    }
}