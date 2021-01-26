using System.Linq;
using HarmonyLib;
using MVCF.Utilities;
using RimWorld;
using Verse;

// ReSharper disable InconsistentNaming

namespace MVCF.Harmony
{
    [HarmonyPatch(typeof(Pawn), "TryGetAttackVerb")]
    public class Pawn_TryGetAttackVerb
    {
        public static bool Prefix(ref Verb __result, Pawn __instance, Thing target,
            out bool __state, bool allowManualCastWeapons = false)
        {
            var manager = __instance.Manager();
            var job = __instance.CurJob;

            if (manager.debugOpts.VerbLogging)
                Log.Message("TryGetAttackVerb of " + __instance + " on target " + target + " with job " + job +
                            " that has target " + job?.targetA + " and CurrentVerb " + manager.CurrentVerb);

            if (target == null && (job == null || !job.targetA.IsValid || job.def != JobDefOf.AttackStatic ||
                                   !job.targetA.HasThing && (job.targetA.Cell == __instance.Position ||
                                                             !job.targetA.Cell.InBounds(__instance.Map))))
            {
                manager.CurrentVerb = null;
                __result = manager.HasVerbs ? manager.SearchVerb : null;
                __state = __result == null || !__result.Available();
                return __state;
            }

            if (manager.CurrentVerb != null && manager.CurrentVerb.Available() &&
                (target == null || manager.CurrentVerb.CanHitTarget(target)) &&
                (job == null || !job.targetA.IsValid || !job.targetA.HasThing ||
                 job.targetA.Cell == __instance.Position || !job.targetA.Cell.InBounds(__instance.Map) ||
                 manager.CurrentVerb.CanHitTarget(job.targetA)))
            {
                __result = manager.CurrentVerb;
                __state = false;
                return false;
            }

            var verbs = manager.ManagedVerbs.Where(v =>
                !v.Verb.IsMeleeAttack && (v.Props == null || !v.Props.canFireIndependently) && v.Enabled &&
                v.Verb.Available());
            if (!allowManualCastWeapons && job != null && job.def == JobDefOf.Wait_Combat)
                verbs = verbs.Where(v => !v.Verb.verbProps.onlyManualCast);

            var verbsToUse = verbs.ToList();

            if (verbsToUse.Count == 0) return __state = true;

            if (manager.debugOpts.ScoreLogging)
                Log.Message("Getting best verb for target " + target + " or " + job.targetA + " which is " +
                            (target ?? job.targetA));
            var verbToUse = __instance.BestVerbForTarget(target ?? job.targetA, verbsToUse, manager);

            if (verbToUse == null) return __state = true;

            __result = verbToUse;
            __state = false;

            return false;
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