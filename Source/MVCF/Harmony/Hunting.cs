using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MVCF.Utilities;
using RimWorld;
using Verse;
using Verse.AI;

namespace MVCF.Harmony
{
    [HarmonyPatch]
    public class Hunting
    {
        [HarmonyPatch(typeof(WorkGiver_HunterHunt), "HasHuntingWeapon")]
        [HarmonyPostfix]
        public static void HasHuntingWeapon(Pawn p, ref bool __result)
        {
            if (__result) return;
            var man = p.Manager();
            if (man.ManagedVerbs.Any(mv =>
                !mv.Verb.IsMeleeAttack && mv.Verb.HarmsHealth() && !mv.Verb.UsesExplosiveProjectiles() &&
                mv.Enabled && mv.Verb.Available()))
                __result = true;
        }

        [HarmonyPatch(typeof(Toils_Combat), "TrySetJobToUseAttackVerb")]
        [HarmonyPrefix]
        public static bool TrySetJobToUseAttackVerb(ref Toil __result, TargetIndex targetInd)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                if (!actor.jobs.curJob.GetTarget(targetInd).IsValid)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }

                var man = actor.Manager();
                var verbs = man.ManagedVerbs.Where(mv =>
                    !mv.Verb.IsMeleeAttack && mv.Enabled &&
                    (!actor.IsColonist || !mv.Verb.verbProps.onlyManualCast) &&
                    (mv.Props == null || !mv.Props.canFireIndependently) && mv.Verb.Available());
                var verb = actor.BestVerbForTarget(actor.jobs.curJob.GetTarget(targetInd), verbs, man);
                if (verb == null)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }

                actor.jobs.curJob.verbToUse = verb;
            };
            __result = toil;
            return false;
        }

        [HarmonyPatch(typeof(RimWorld.JobDriver_Hunt), "MakeNewToils")]
        [HarmonyPostfix]
        public static IEnumerable<Toil> MakeNewToils(IEnumerable<Toil> __result, JobDriver_Hunt __instance)
        {
            var list = __result.ToList();
            var setVerb = list[1];
            list.Insert(4, Toils_Jump.JumpIf(setVerb, () => !__instance.job.verbToUse.Available()));
            return list;
        }
    }
}