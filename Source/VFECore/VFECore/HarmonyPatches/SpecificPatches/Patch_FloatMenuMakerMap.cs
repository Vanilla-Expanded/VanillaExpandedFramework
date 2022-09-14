using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using HarmonyLib;

namespace VFECore
{
    public static class Patch_FloatMenuMakerMap
    {
        // This fix replaces the transpiler code below
        // Note that the EquipWarningShieldUnusableWithWeapon does not work anymore with this fix
        [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
        public static class AddHumanlikeOrders_Fix
        {
            public static void Postfix(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> opts)
            {
                var shield = GridsUtility.GetThingList(IntVec3.FromVector3(clickPos), pawn.Map).OfType<Apparel_Shield>().FirstOrDefault();
                if (shield != null)
                {
                    TaggedString toCheck = "ForceWear".Translate(shield.LabelCap, shield);
                    FloatMenuOption floatMenuOption = opts.FirstOrDefault((FloatMenuOption x) => x.Label.Contains
                    (toCheck));
                    if (floatMenuOption != null)
                    {
                        opts.Remove(floatMenuOption);
                    }
                }
    
                IntVec3 c = IntVec3.FromVector3(clickPos);
                if (pawn.equipment != null)
                {
                    List<Thing> thingList = c.GetThingList(pawn.Map);
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        if (thingList[i].TryGetComp<CompEquippable>() != null)
                        {
                            var equipment = (ThingWithComps) thingList[i];
                            TaggedString toCheck = "Equip".Translate(equipment.LabelShort);
                            FloatMenuOption floatMenuOption = opts.FirstOrDefault((FloatMenuOption x) => x.Label.Contains
                            (toCheck));
                            if (floatMenuOption != null && pawn.equipment != null && !equipment.def.UsableWithShields() && pawn.OffHandShield() is Apparel_Shield oldShield)
                            {
                                floatMenuOption.Label += $" {"VanillaFactionsExpanded.EquipWarningShieldUnusableWithWeapon".Translate(oldShield.def.label)}";
                            }
                            
                            AddShieldFloatMenuOption(pawn, equipment, ref opts);
                            break;
                        }
                    }
                }
            }
        }
    
        public static void AddShieldFloatMenuOption(Pawn pawn, Thing equipment, ref List<FloatMenuOption> opts)
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
                else if (!pawn.CanReach(equipment, PathEndMode.ClosestTouch, Danger.Deadly, false, false,TraverseMode.ByPawn))
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
                        pawn.jobs.TryTakeOrderedJob(new Job(VFEDefOf.VFEC_EquipShield, equipment), JobTag.Misc);
                        FleckMaker.Static(equipment.DrawPos, equipment.MapHeld, FleckDefOf.FeedbackEquip, 1f);
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.EquippingWeapons, KnowledgeAmount.Total);
                    }, MenuOptionPriority.High, null, null, 0f, null, null), pawn, equipment, "ReservedBy");
                }
                opts.Add(shieldOption);
            }
        }
    }

}
