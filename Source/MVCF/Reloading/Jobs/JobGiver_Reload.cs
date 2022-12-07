using System;
using System.Collections.Generic;
using System.Linq;
using MVCF.Features;
using MVCF.Reloading;
using MVCF.Reloading.Comps;
using MVCF.Utilities;
using RimWorld;
using Verse;
using Verse.AI;

// ReSharper disable once CheckNamespace
namespace Reloading;

public class JobGiver_Reload : ThinkNode_JobGiver
{
    public override float GetPriority(Pawn pawn) => 5.9f;

    protected override Job TryGiveJob(Pawn pawn)
    {
        if (!MVCF.MVCF.GetFeature<Feature_Reloading>().Enabled) return null;
        var comp = pawn.AllReloadComps().FirstOrDefault(reloadable => reloadable.NeedsReload());
        if (comp == null) return null;
        if (comp is VerbComp_Reloadable_ChangeableAmmo) return null;
        var list = FindAmmo(pawn, pawn.Position, comp);
        return list.NullOrEmpty() ? null : MakeReloadJob(comp, list);
    }

    public static Job MakeReloadJob(VerbComp_Reloadable comp, List<Thing> ammo)
    {
        var job = JobMaker.MakeJob(ReloadingDefOf.BetterReload);
        job.verbToUse = comp.parent.Verb;
        job.targetQueueA = ammo.Select(t => new LocalTargetInfo(t)).ToList();
        job.count = Math.Min(ammo.Sum(t => t.stackCount), comp.Props.ItemsPerShot * (comp.Props.MaxShots - comp.ShotsRemaining));
        return job;
    }

    public static List<Thing> FindAmmo(Pawn pawn, IntVec3 root, VerbComp_Reloadable comp)
    {
        if (comp == null) return null;
        var desired = new IntRange(comp.Props.ItemsPerShot, comp.Props.ItemsPerShot * (comp.Props.MaxShots - comp.ShotsRemaining));
        return RefuelWorkGiverUtility.FindEnoughReservableThings(pawn, root, desired, comp.CanReloadFrom);
    }
}
