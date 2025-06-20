using Verse;
using RimWorld;
using Verse.AI;

namespace VEF.Apparels
{
    public class FloatMenuOptionProvider_EquipShield : FloatMenuOptionProvider
    {
        protected override bool Drafted => true;

        protected override bool Undrafted => true;

        protected override bool Multiselect => false;

        protected override bool AppliesInt(FloatMenuContext context)
        {
            return context.FirstSelectedPawn.equipment != null;
        }

        protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
        {
            var selPawn = context.FirstSelectedPawn;
            var option = AddShieldFloatMenuOption(selPawn, clickedThing, clickedThing);
            return option;
        }

        public static FloatMenuOption AddShieldFloatMenuOption(Pawn pawn, Thing equipment, Thing owner)
        {
            // Add an extra option to the float menu if the thing is a shield
            if (equipment.IsShield(out CompShield shieldComp) && ApparelUtility.HasPartsToWear(pawn, equipment.def))
            {
                string labelShort = equipment.LabelShort;
                FloatMenuOption shieldOption;

                // Pawn is pacifist
                if (equipment.def.IsWeapon && pawn.WorkTagIsDisabled(WorkTags.Violent))
                    shieldOption = new FloatMenuOption("CannotEquip".Translate(labelShort) + " (" + "IsIncapableOfViolenceLower".Translate(pawn.LabelShort, pawn) + ")", null);

                // Pawn cannot path to shield
                else if (!pawn.CanReach(equipment, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
                    shieldOption = new FloatMenuOption("CannotEquip".Translate(labelShort) + " (" + "NoPath".Translate() + ")", null);

                // Pawn cannot manipulate
                else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || !pawn.CanUseShields())
                    shieldOption = new FloatMenuOption("CannotEquip".Translate(labelShort) + " (" + "Incapable".Translate() + ")", null);

                // Shield is burning
                else if (equipment.IsBurning())
                    shieldOption = new FloatMenuOption("CannotEquip".Translate(labelShort) + " (" + "BurningLower".Translate() + ")", null);

                // Able to equip shield
                else
                {
                    string optionLabel = "VanillaFactionsExpanded.EquipShield".Translate(labelShort);

                    // I seriously doubt this'll ever return true but hey, why not
                    if (equipment.def.IsRangedWeapon && pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.Brawler))
                        optionLabel = optionLabel + " " + "EquipWarningBrawler".Translate();

                    // Primary cannot be used with shields
                    if (pawn.equipment.Primary is ThingWithComps weapon && !weapon.def.UsableWithShields())
                    {
                        optionLabel += $" {"VanillaFactionsExpanded.EquipWarningShieldUnusable".Translate(weapon.def.label)}";
                    }
                    shieldOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(optionLabel, delegate () {
                        equipment.SetForbidden(false, true);
                        pawn.jobs.TryTakeOrderedJob(new Job(VEFDefOf.VFEC_EquipShield, owner, equipment), JobTag.Misc);
                        FleckMaker.Static(owner.DrawPos, owner.MapHeld, FleckDefOf.FeedbackEquip, 1f);
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.EquippingWeapons, KnowledgeAmount.Total);
                    }, MenuOptionPriority.High, null, null, 0f, null, null), pawn, owner);
                }
                return shieldOption;
            }
            return null;
        }
    }
}
