using System.Collections.Generic;
using MVCF.Reloading;
using MVCF.Utilities;
using Reloading;
using RimWorld;
using Verse;
using Verse.AI;
using JobGiver_Reload = Reloading.JobGiver_Reload;

namespace MVCF.FloatMenuOptionProviders;

public class FloatMenuOptionProvider_Reload : FloatMenuOptionProvider
{
    protected override bool Drafted => true;
    protected override bool Undrafted => true;
    protected override bool Multiselect => false;

    public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
    {
        // We could probably make it support any selected pawn... But it may require changing translation keys around a bit to include which pawn it affects.

        var pawn = context.FirstSelectedPawn;

        foreach (var comp in clickedThing.AllReloadComps())
        {
            var text = $"{"Reloading.Unload".Translate(clickedThing.Named("GEAR"))} ({comp.ShotsRemaining}/{comp.Props.MaxShots})";
            if (comp.ShotsRemaining == 0)
            {
                text += ": " + "Reloading.NoAmmo".Translate();
                yield return new FloatMenuOption(text, null);
            }
            else
                yield return new FloatMenuOption(text, () =>
                {
                    var job = JobMaker.MakeJob(ReloadingDefOf.Unload, clickedThing);
                    job.verbToUse = comp.parent.Verb;
                    pawn.jobs.TryTakeOrderedJob(job);
                });
        }

        foreach (var reloadable in pawn.AllReloadComps())
        {
            if (reloadable.CanReloadFrom(clickedThing))
            {
                var text = "Reloading.Reload".Translate(
                               reloadable.parent.Verb.Label(reloadable.parent.Props).Named("GEAR"),
                               clickedThing.def.Named("AMMO")) + " (" + reloadable.ShotsRemaining + "/" +
                           reloadable.Props.MaxShots + ")";
                var failed = false;
                var ammo = new List<Thing>();
                if (!pawn.CanReach(clickedThing, PathEndMode.ClosestTouch, Danger.Deadly))
                {
                    text += ": " + "NoPath".Translate().CapitalizeFirst();
                    failed = true;
                }
                else if (!reloadable.NeedsReload())
                {
                    text += ": " + "ReloadFull".Translate();
                    failed = true;
                }
                else if ((ammo = JobGiver_Reload.FindAmmo(pawn, context.ClickedCell, reloadable)).NullOrEmpty())
                {
                    text += ": " + "ReloadNotEnough".Translate();
                    failed = true;
                }

                if (failed) yield return new FloatMenuOption(text, null);
                else
                    yield return RimWorld.FloatMenuUtility.DecoratePrioritizedTask(
                        new FloatMenuOption(text,
                            () => pawn.jobs.TryTakeOrderedJob(
                                JobGiver_Reload.MakeReloadJob(reloadable, ammo))), pawn, clickedThing);
            }
            else if (clickedThing == pawn)
                foreach (var item in pawn.inventory.innerContainer)
                    if (reloadable.CanReloadFrom(item))
                    {
                        var text = "Reloading.Reload".Translate(
                                       reloadable.parent.Verb.Label(reloadable.parent.Props).Named("GEAR"),
                                       item.def.Named("AMMO")) + " (" + reloadable.ShotsRemaining + "/" +
                                   reloadable.Props.MaxShots + ")";
                        if (!reloadable.NeedsReload())
                            yield return new FloatMenuOption(text + ": " + "ReloadFull".Translate(), null);
                        else
                            yield return 
                                new FloatMenuOption(text,
                                    () => pawn.jobs.TryTakeOrderedJob(
                                        JobGiver_ReloadFromInventory.MakeReloadJob(reloadable, item)));
                    }
        }
    }
}