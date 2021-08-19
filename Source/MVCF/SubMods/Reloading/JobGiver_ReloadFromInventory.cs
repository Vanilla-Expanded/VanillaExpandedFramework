using System;
using System.Linq;
using Verse;
using Verse.AI;

namespace Reloading
{
    internal class JobGiver_ReloadFromInventory : ThinkNode_JobGiver
    {
        public override float GetPriority(Pawn pawn)
        {
            return 6.1f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            var comp = pawn.AllReloadComps().FirstOrDefault(r => r.NeedsReload());
            if (comp == null) return null;
            return (from thing in pawn.inventory.GetDirectlyHeldThings()
                where comp.CanReloadFrom(thing)
                select MakeReloadJob(comp, thing)).FirstOrDefault();
        }

        public static Job MakeReloadJob(IReloadable comp, Thing ammo)
        {
            var job = JobMaker.MakeJob(ReloadingDefOf.ReloadFromInventory, comp.Thing);
            job.targetB = ammo;
            job.count = Math.Min(ammo.stackCount,
                comp.ItemsPerShot * (comp.MaxShots - comp.ShotsRemaining));
            return job;
        }
    }
}