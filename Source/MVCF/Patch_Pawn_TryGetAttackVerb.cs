using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MVCF.Utilities;
using RimWorld;
using Verse;

namespace MVCF.HarmonyPatches
{
    public class Patch_Pawn_TryGetAttackVerb
    {
        public static HashSet<JobDef> AggressiveJobs = new();

        public static Patch GetPatch()
        {
            AggressiveJobs.AddRange(new List<JobDef>
            {
                // Vanilla:
                JobDefOf.AttackStatic,
                JobDefOf.AttackMelee,
                JobDefOf.UseVerbOnThing,
                JobDefOf.UseVerbOnThingStatic,
                // Misc. Training:
                DefDatabase<JobDef>.GetNamedSilentFail("ArcheryShootArrows"),
                DefDatabase<JobDef>.GetNamedSilentFail("UseShootingRange"),
                DefDatabase<JobDef>.GetNamedSilentFail("UseShootingRange_NonJoy"),
                DefDatabase<JobDef>.GetNamedSilentFail("UseMartialArtsTarget"),
                DefDatabase<JobDef>.GetNamedSilentFail("UseMartialArtsTarget_NonJoy"),
                // Combat Training and Forked Version:
                DefDatabase<JobDef>.GetNamedSilentFail("TrainOnCombatDummy"),
                // Human Resources:
                DefDatabase<JobDef>.GetNamedSilentFail("TrainWeapon"),
                DefDatabase<JobDef>.GetNamedSilentFail("PlayAtDummy"),
                DefDatabase<JobDef>.GetNamedSilentFail("PlayAtTarget"),
                // Hardcore SK:
                DefDatabase<JobDef>.GetNamedSilentFail("AnimalRangeAttack")
            }.Where(def => def != null));
            return new Patch(AccessTools.Method(typeof(Pawn), "TryGetAttackVerb"), AccessTools.Method(typeof(Patch_Pawn_TryGetAttackVerb), "Prefix"), AccessTools.Method(
                typeof(Patch_Pawn_TryGetAttackVerb), "Postfix"));
        }

        public static Verb AttackVerb(Pawn pawn, Thing target, bool allowManualCastWeapons = false)
        {
            var manager = pawn.Manager();
            var job = pawn.CurJob;

            if (manager.debugOpts.VerbLogging)
                Log.Message("AttackVerb of " + pawn + " on target " + target + " with job " + job +
                            " that has target " + job?.targetA + " and CurrentVerb " + manager.CurrentVerb +
                            " and OverrideVerb " + manager.OverrideVerb);

            if (manager.OverrideVerb != null) return manager.OverrideVerb;

            if (target == null && (job == null || !job.targetA.IsValid || !AggressiveJobs.Contains(job.def) ||
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

            var verbs = manager.CurrentlyUseableRangedVerbs;
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