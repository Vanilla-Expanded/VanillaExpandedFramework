using System.Linq;
using HarmonyLib;
using MVCF.Utilities;
using RimWorld;
using Verse;

namespace MVCF
{
    public class Patch_Pawn_TryGetAttackVerb
    {
        public static Patch GetPatch() =>
            new(AccessTools.Method(typeof(Pawn), "TryGetAttackVerb"), AccessTools.Method(typeof(Patch_Pawn_TryGetAttackVerb), "Prefix"), AccessTools.Method(
                typeof(Patch_Pawn_TryGetAttackVerb), "Postfix"));

        public static Verb AttackVerb(Pawn pawn, Thing target, bool allowManualCastWeapons = false)
        {
            var manager = pawn.Manager();
            var job = pawn.CurJob;

            if (manager.debugOpts.VerbLogging)
                Log.Message($"[MVCF] AttackVerb of {pawn} on target {target} with job {job} that has target {job?.targetA} and CurrentVerb {manager.CurrentVerb}");

            if (manager.CurrentVerb != null && manager.CurrentVerb.Available() &&
                (target == null || manager.CurrentVerb.CanHitTarget(target)) &&
                (job is not {targetA: {IsValid: true, Cell: var cell}} || cell == pawn.Position || !cell.InBounds(pawn.Map) ||
                 manager.CurrentVerb.CanHitTarget(job.targetA)))
                return manager.CurrentVerb;

            var verbs = manager.CurrentlyUseableRangedVerbs;
            if (!allowManualCastWeapons && job != null && job.def == JobDefOf.Wait_Combat)
                verbs = verbs.Where(v => !v.Verb.verbProps.onlyManualCast);

            var verbsToUse = verbs.ToList();

            if (verbsToUse.Count == 0) return null;


            var usedTarget = target ?? job?.targetA ?? LocalTargetInfo.Invalid;
            if (manager.debugOpts.ScoreLogging)
                Log.Message($"[MVCF] Getting best verb for target {target} or {job?.targetA} which is {usedTarget}");
            if (!usedTarget.IsValid || !usedTarget.Cell.InBounds(pawn.Map)) return null;
            return pawn.BestVerbForTarget(usedTarget, verbsToUse);
        }

        public static bool Prefix(ref Verb __result, Pawn __instance, Thing target,
            out bool __state, bool allowManualCastWeapons = false)
        {
            __result = AttackVerb(__instance, target, allowManualCastWeapons);
            return __state = __result == null;
        }

        public static void Postfix(ref Verb __result, bool __state, Pawn __instance)
        {
            if (__result?.verbProps?.label.NullOrEmpty() ?? true) return;
            if (__result.verbProps.label == Base.SearchLabel && __state)
            {
                if (__instance.Manager().debugOpts.VerbLogging)
                    Log.Message("Overwriting SearchVerb with null");
                __result = null;
            }
        }
    }
}