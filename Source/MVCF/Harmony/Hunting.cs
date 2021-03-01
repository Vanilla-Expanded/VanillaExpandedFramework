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
        public static void DoPatches(HarmonyLib.Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(WorkGiver_HunterHunt), "HasHuntingWeapon"),
                postfix: new HarmonyMethod(typeof(Hunting), "HasHuntingWeapon"));
            harm.Patch(AccessTools.Method(typeof(Toils_Combat), "TrySetJobToUseAttackVerb"),
                new HarmonyMethod(typeof(Hunting), "TrySetJobToUseAttackVerb"));
            harm.Patch(AccessTools.Method(typeof(JobDriver_Hunt), "MakeNewToils"),
                postfix: new HarmonyMethod(typeof(Hunting), "MakeNewToils"));
        }

        public static void HasHuntingWeapon(Pawn p, ref bool __result)
        {
            if (__result) return;
            var man = p.Manager();
            if (man.ManagedVerbs.Any(mv =>
                !mv.Verb.IsMeleeAttack && mv.Verb.HarmsHealth() && !mv.Verb.UsesExplosiveProjectiles() &&
                mv.Enabled && mv.Verb.Available()))
                __result = true;
        }

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
                var verb = actor.BestVerbForTarget(actor.jobs.curJob.GetTarget(targetInd),
                               man.CurrentlyUseableRangedVerbs, man) ??
                           actor.TryGetAttackVerb(actor.jobs.curJob.GetTarget(targetInd).Thing, !actor.IsColonist);
                if (verb == null)
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                else
                    actor.jobs.curJob.verbToUse = verb;
            };
            __result = toil;
            return false;
        }

        public static IEnumerable<Toil> MakeNewToils(IEnumerable<Toil> __result, JobDriver_Hunt __instance)
        {
            var list = __result.ToList();
            var setVerb = list[1];
            list.Insert(4, Toils_Jump.JumpIf(setVerb, () => !__instance.job.verbToUse.Available()));
            return list;
        }
    }
}