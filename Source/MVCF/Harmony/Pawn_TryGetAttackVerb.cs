using System.Linq;
using HarmonyLib;
using MVCF.Utilities;
using RimWorld;
using Verse;

// ReSharper disable InconsistentNaming

namespace MVCF.Harmony
{
    public class Pawn_TryGetAttackVerb
    {
        public static void DoPatches(HarmonyLib.Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(Pawn), "TryGetAttackVerb"),
                new HarmonyMethod(typeof(Pawn_TryGetAttackVerb), "Prefix"),
                new HarmonyMethod(typeof(Pawn_TryGetAttackVerb), "Postfix"));
        }

        public static Verb AttackVerb(Pawn pawn, Thing target, bool allowManualCastWeapons = false)
        {
            var manager = pawn.Manager();
            var job = pawn.CurJob;

            if (manager.debugOpts.VerbLogging)
                Log.Message("TryGetAttackVerb of " + pawn + " on target " + target + " with job " + job +
                            " that has target " + job?.targetA + " and CurrentVerb " + manager.CurrentVerb +
                            " and OverrideVerb " + manager.OverrideVerb);

            if (manager.OverrideVerb != null) return manager.OverrideVerb;

            if (target == null && (job == null || !job.targetA.IsValid || job.def != JobDefOf.AttackStatic ||
                                   !job.targetA.HasThing && (job.targetA.Cell == pawn.Position ||
                                                             !job.targetA.Cell.InBounds(pawn.Map))))
            {
                manager.CurrentVerb = null;
                return manager.HasVerbs && manager.SearchVerb != null && manager.SearchVerb.Available()
                    ? manager.SearchVerb
                    : null;
            }

            if (manager.CurrentVerb != null && manager.CurrentVerb.Available() &&
                (target == null || manager.CurrentVerb.CanHitTarget(target)) &&
                (job == null || !job.targetA.IsValid || !job.targetA.HasThing ||
                 job.targetA.Cell == pawn.Position || !job.targetA.Cell.InBounds(pawn.Map) ||
                 manager.CurrentVerb.CanHitTarget(job.targetA)))
                return manager.CurrentVerb;

            var verbs = manager.ManagedVerbs.Where(v =>
                !v.Verb.IsMeleeAttack && (v.Props == null || !v.Props.canFireIndependently) && v.Enabled &&
                v.Verb.Available());
            if (!allowManualCastWeapons && job != null && job.def == JobDefOf.Wait_Combat)
                verbs = verbs.Where(v => !v.Verb.verbProps.onlyManualCast);

            var verbsToUse = verbs.ToList();

            if (verbsToUse.Count == 0) return null;

            if (manager.debugOpts.ScoreLogging)
                Log.Message("Getting best verb for target " + target + " or " + job.targetA + " which is " +
                            (target ?? job.targetA));

            return pawn.BestVerbForTarget(target ?? job.targetA, verbsToUse, manager);
        }

        public static bool Prefix(ref Verb __result, Pawn __instance, Thing target,
            out bool __state, bool allowManualCastWeapons = false)
        {
            __result = AttackVerb(__instance, target, allowManualCastWeapons);
            return __state = __result == null;
        }

        public static void Postfix(ref Verb __result, bool __state, Pawn __instance)
        {
            if (__result == null) return;
            if (__result.verbProps.label == Base.SearchLabel && __state)
            {
                if (__instance.Manager().debugOpts.VerbLogging)
                    Log.Message("Overwriting SearchVerb with null");
                __result = null;
            }
        }
    }
}