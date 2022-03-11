using System.Collections.Generic;
using MVCF.Reloading;
using Reloading;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MVCF.Utilities
{
    public static class FloatMenuUtility
    {
        public static void AddWeaponReloadOrders(List<FloatMenuOption> opts, Vector3 clickPos, Pawn pawn)
        {
            var c = IntVec3.FromVector3(clickPos);

            foreach (var thing in c.GetThingList(pawn.Map))
            foreach (var comp in thing.AllReloadComps())
            {
                var text = $"{"Reloading.Unload".Translate(thing.Named("GEAR"))} ({comp.ShotsRemaining}/{comp.Props.MaxShots})";
                if (comp.ShotsRemaining == 0)
                {
                    text += ": " + "Reloading.NoAmmo".Translate();
                    opts.Add(new FloatMenuOption(text, null));
                }
                else
                    opts.Add(new FloatMenuOption(text, () =>
                    {
                        var job = JobMaker.MakeJob(ReloadingDefOf.Unload, thing);
                        job.verbToUse = comp.parent.Verb;
                        pawn.jobs.TryTakeOrderedJob(job);
                    }));
            }

            foreach (var reloadable in pawn.AllReloadComps())
            foreach (var thing in c.GetThingList(pawn.Map))
                if (reloadable.CanReloadFrom(thing))
                {
                    var text = "Reloading.Reload".Translate(
                                   reloadable.parent.Verb.Label(reloadable.parent.Props).Named("GEAR"),
                                   thing.def.Named("AMMO")) + " (" + reloadable.ShotsRemaining + "/" +
                               reloadable.Props.MaxShots + ")";
                    var failed = false;
                    var ammo = new List<Thing>();
                    if (!pawn.CanReach(thing, PathEndMode.ClosestTouch, Danger.Deadly))
                    {
                        text += ": " + "NoPath".Translate().CapitalizeFirst();
                        failed = true;
                    }
                    else if (!reloadable.NeedsReload())
                    {
                        text += ": " + "ReloadFull".Translate();
                        failed = true;
                    }
                    else if ((ammo = JobGiver_Reload.FindAmmo(pawn, c, reloadable)).NullOrEmpty())
                    {
                        text += ": " + "ReloadNotEnough".Translate();
                        failed = true;
                    }

                    if (failed) opts.Add(new FloatMenuOption(text, null));
                    else
                        opts.Add(RimWorld.FloatMenuUtility.DecoratePrioritizedTask(
                            new FloatMenuOption(text,
                                () => pawn.jobs.TryTakeOrderedJob(
                                    JobGiver_Reload.MakeReloadJob(reloadable, ammo))), pawn, thing));
                }
                else if (thing == pawn)
                    foreach (var item in pawn.inventory.innerContainer)
                        if (reloadable.CanReloadFrom(item))
                        {
                            var text = "Reloading.Reload".Translate(
                                           reloadable.parent.Verb.Label(reloadable.parent.Props).Named("GEAR"),
                                           item.def.Named("AMMO")) + " (" + reloadable.ShotsRemaining + "/" +
                                       reloadable.Props.MaxShots + ")";
                            if (!reloadable.NeedsReload())
                                opts.Add(new FloatMenuOption(text + ": " + "ReloadFull".Translate(), null));
                            else
                                opts.Add(
                                    new FloatMenuOption(text,
                                        () => pawn.jobs.TryTakeOrderedJob(
                                            JobGiver_ReloadFromInventory.MakeReloadJob(reloadable, item))));
                        }
        }
    }
}