using System;
using System.Linq;
using MVCF.Features;
using MVCF.Reloading;
using MVCF.Reloading.Comps;
using MVCF.Utilities;
using Verse;
using Verse.AI;

namespace Reloading;

public class JobGiver_ReloadFromInventory : ThinkNode_JobGiver
{
    public override float GetPriority(Pawn pawn) => 99f;

    protected override Job TryGiveJob(Pawn pawn) => MVCF.MVCF.GetFeature<Feature_Reloading>().Enabled ? TryGiveReloadJob(pawn) : null;

    public static Job TryGiveReloadJob(Pawn pawn)
    {
        var comp = pawn.AllReloadComps().FirstOrDefault(r => r.NeedsReload() && r.ReloadItemInInventory != null);
        return comp == null ? null : MakeReloadJob(comp, comp.ReloadItemInInventory);
    }

    public static Job MakeReloadJob(VerbComp_Reloadable comp, Thing ammo)
    {
        var job = JobMaker.MakeJob(ReloadingDefOf.ReloadFromInventory, ammo);
        job.verbToUse = comp.parent.Verb;
        job.count = Math.Min(ammo.stackCount, comp.Props.ItemsPerShot * (comp.Props.MaxShots - comp.ShotsRemaining));
        return job;
    }
}
