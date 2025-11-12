
using RimWorld;
using RimWorld.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
namespace VEF.Weapons
{
    public class JobGiver_ReloadWeaponTrait : ThinkNode_JobGiver
    {
        private const bool ForceReloadWhenLookingForWork = false;

        public override float GetPriority(Pawn pawn)
        {
            return 5.9f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                return null;
            }
            CompApplyWeaponTraits comp = FloatMenuOptionProvider_ReloadWeaponTrait.FindSomeReloadableComponent(pawn);
            if (comp == null)
            {
                return null;
            }
            if (pawn.carryTracker.AvailableStackSpace(comp.AbilityDetailsForWeapon(comp.GetDetails()).ammoDef) < comp.MinAmmoNeeded())
            {
                return null;
            }
            List<Thing> list = FloatMenuOptionProvider_ReloadWeaponTrait.FindEnoughAmmo(pawn, pawn.Position, comp);
            if (list.NullOrEmpty())
            {
                return null;
            }
            return MakeReloadJob(comp, list);
        }

        public static Job MakeReloadJob(CompApplyWeaponTraits comp, List<Thing> chosenAmmo)
        {
            Job job = JobMaker.MakeJob(InternalDefOf.VEF_ReloadWeaponTrait, comp.parent);
            job.targetQueueB = chosenAmmo.Select((Thing t) => new LocalTargetInfo(t)).ToList();
            job.count = chosenAmmo.Sum((Thing t) => t.stackCount);
            job.count = Math.Min(job.count, comp.MaxAmmoNeeded());
            return job;
        }
    }
}