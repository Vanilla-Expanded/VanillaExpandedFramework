using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MVCF.Utilities;
using RimWorld;
using Verse;
using Verse.AI;

namespace MVCF.PatchSets;

public class PatchSet_Hunting : PatchSet
{
    public override IEnumerable<Patch> GetPatches()
    {
        yield return Patch.Prefix(AccessTools.Method(typeof(WorkGiver_HunterHunt), nameof(WorkGiver_HunterHunt.HasHuntingWeapon)),
            AccessTools.Method(GetType(), nameof(HasHuntingWeapon)));
        yield return Patch.Prefix(AccessTools.Method(typeof(Toils_Combat), nameof(Toils_Combat.TrySetJobToUseAttackVerb)),
            AccessTools.Method(GetType(), nameof(TrySetJobToUseAttackVerb)));
        yield return Patch.Postfix(AccessTools.Method(typeof(JobDriver_Hunt), "MakeNewToils"),
            AccessTools.Method(GetType(), nameof(MakeNewToils)));
    }

    public static bool HasHuntingWeapon(Pawn p, ref bool __result)
    {
        var man = p.Manager();
        __result = man.ManagedVerbs.Any(mv =>
            !mv.Verb.IsMeleeAttack && mv.Verb.HarmsHealth() && !mv.Verb.UsesExplosiveProjectiles() &&
            mv.Enabled && mv.Verb.Available());
        return false;
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
            var verb = actor.BestVerbForTarget(actor.jobs.curJob.GetTarget(targetInd), man.CurrentlyUseableRangedVerbs) ??
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
