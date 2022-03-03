using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Reloading
{
    internal class JobGiver_Reload : ThinkNode_JobGiver
    {
        public override float GetPriority(Pawn pawn)
        {
            return 5.9f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            var comp = pawn.AllReloadComps().FirstOrDefault(reloadable => reloadable.NeedsReload());
            if (comp == null) return null;
            if (comp is CompChangeableAmmo) return null;
            var list = FindAmmo(pawn, pawn.Position, comp);
            return list.NullOrEmpty() ? null : MakeReloadJob(comp, list);
        }

        public static Job MakeReloadJob(IReloadable comp, List<Thing> ammo)
        {
            var job = JobMaker.MakeJob(ReloadingDefOf.BetterReload, comp.Thing);
            job.targetQueueB = ammo.Select(t => new LocalTargetInfo(t)).ToList();
            job.count = Math.Min(ammo.Sum(t => t.stackCount),
                comp.ItemsPerShot * (comp.MaxShots - comp.ShotsRemaining));
            return job;
        }

        public static List<Thing> FindAmmo(Pawn pawn, IntVec3 root, IReloadable comp)
        {
            if (comp == null) return null;
            var desired = new IntRange(comp.ItemsPerShot,
                comp.ItemsPerShot * (comp.MaxShots - comp.ShotsRemaining));
            return RefuelWorkGiverUtility.FindEnoughReservableThings(pawn, root, desired, comp.CanReloadFrom);
        }

        public static CompReloadable FindAnyReloadableWeapon(Pawn pawn)
        {
            if (pawn?.equipment == null) return null;
            foreach (var thing in pawn.equipment.AllEquipmentListForReading)
                if (thing.TryGetComp<CompReloadable>() is CompReloadable comp)
                    return comp;
            return null;
        }
    }
}