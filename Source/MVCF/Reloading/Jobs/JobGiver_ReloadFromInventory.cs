using System;
using System.Linq;
using MVCF;
using MVCF.Features;
using MVCF.Reloading;
using MVCF.Reloading.Comps;
using MVCF.Utilities;
using Verse;
using Verse.AI;

namespace Reloading
{
    public class JobGiver_ReloadFromInventory : ThinkNode_JobGiver
    {
        public override float GetPriority(Pawn pawn) => 6.1f;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!Base.GetFeature<Feature_Reloading>().Enabled) return null;
            var comp = pawn.AllReloadComps().FirstOrDefault(r => r.NeedsReload());
            if (comp == null) return null;
            return (from thing in pawn.inventory.GetDirectlyHeldThings()
                where comp.CanReloadFrom(thing)
                select MakeReloadJob(comp, thing)).FirstOrDefault();
        }

        public static Job MakeReloadJob(VerbComp_Reloadable comp, Thing ammo)
        {
            var job = JobMaker.MakeJob(ReloadingDefOf.ReloadFromInventory, ammo);
            job.verbToUse = comp.parent.Verb;
            job.count = Math.Min(ammo.stackCount, comp.Props.ItemsPerShot * (comp.Props.MaxShots - comp.ShotsRemaining));
            return job;
        }
    }
}