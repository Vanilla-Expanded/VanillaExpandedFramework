
using RimWorld;
using RimWorld.Utility;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace VEF.Weapons
{
    public class FloatMenuOptionProvider_ReloadWeaponTrait : FloatMenuOptionProvider
    {
        protected override bool Drafted => true;

        protected override bool Undrafted => true;

        protected override bool Multiselect => false;

        public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
        {
            foreach (CompApplyWeaponTraits comp in GetReloadablesUsingAmmo(context.FirstSelectedPawn, clickedThing))
            {
               
                string text = "Reload".Translate(comp.parent.Named("GEAR"), NamedArgumentUtility.Named(comp.AbilityDetailsForWeapon(comp.GetDetails()).ammoDef, "AMMO")) + " (" + comp.LabelRemaining + ")";
                List<Thing> chosenAmmo;
                if (!context.FirstSelectedPawn.CanReach(clickedThing, PathEndMode.ClosestTouch, Danger.Deadly))
                {
                    yield return new FloatMenuOption(text + ": " + "NoPath".Translate().CapitalizeFirst(), null);
                }
                else if (!comp.NeedsReload())
                {
                    yield return new FloatMenuOption(text + ": " + "ReloadFull".Translate(), null);
                }
                else if ((chosenAmmo = FindEnoughAmmo(context.FirstSelectedPawn, clickedThing.Position, comp)) == null)
                {
                    yield return new FloatMenuOption(text + ": " + "ReloadNotEnough".Translate(), null);
                }
                else if (context.FirstSelectedPawn.carryTracker.AvailableStackSpace(comp.AbilityDetailsForWeapon(comp.GetDetails()).ammoDef) < comp.MinAmmoNeeded())
                {
                    yield return new FloatMenuOption(text + ": " + "ReloadCannotCarryEnough".Translate(NamedArgumentUtility.Named(comp.AbilityDetailsForWeapon(comp.GetDetails()).ammoDef, "AMMO")), null);
                }
                else
                {
                    Action action = delegate
                    {
                        context.FirstSelectedPawn.jobs.TryTakeOrderedJob(JobGiver_ReloadWeaponTrait.MakeReloadJob(comp, chosenAmmo), JobTag.Misc);
                    };
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, action), context.FirstSelectedPawn, clickedThing);
                }
            }
        }

        public static List<Thing> FindEnoughAmmo(Pawn pawn, IntVec3 rootCell, CompApplyWeaponTraits comp)
        {
            if (comp == null)
            {
                return null;
            }
            IntRange desiredQuantity = new IntRange(comp.MinAmmoNeeded(), comp.MaxAmmoNeeded());
            return RefuelWorkGiverUtility.FindEnoughReservableThings(pawn, rootCell, desiredQuantity, (Thing t) => t.def == comp.AbilityDetailsForWeapon(comp.GetDetails()).ammoDef);
        }

        private IEnumerable<CompApplyWeaponTraits> GetReloadablesUsingAmmo(Pawn pawn, Thing clickedThing)
        {
            if (pawn.equipment?.Primary != null)
            {
                CompApplyWeaponTraits comp = pawn.equipment.Primary.GetComp<CompApplyWeaponTraits>();
                if (comp?.AbilityDetailsForWeapon(comp.GetDetails()) != null && clickedThing.def == comp?.AbilityDetailsForWeapon(comp.GetDetails()).ammoDef)
                {
                    yield return comp;
                }
            }
           
        }

        public static CompApplyWeaponTraits FindSomeReloadableComponent(Pawn pawn)
        {
            if (pawn.equipment?.Primary != null)
            {
                CompApplyWeaponTraits comp = pawn.equipment.Primary.GetComp<CompApplyWeaponTraits>();
                if (comp?.AbilityDetailsForWeapon(comp.GetDetails()) != null && comp.NeedsReload())
                {
                    return comp;
                }
            }
           
            return null;
        }
    }
}