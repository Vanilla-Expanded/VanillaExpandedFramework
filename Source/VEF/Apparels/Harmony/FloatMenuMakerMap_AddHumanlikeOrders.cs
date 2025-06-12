using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using HarmonyLib;

namespace VEF.Apparels
{
    /*
     * 
    //TODO - Assigned to Taranchuk, sort out shield float patches

    [HarmonyPatch(typeof(FloatMenuOptionProvider_Wear), "GetSingleOptionFor")]
    public static class VanillaExpandedFramework_FloatMenuMakerMap_AddHumanlikeOrders_GetSingleOptionFor_Patch
    {
        public static void Postfix(FloatMenuOption __result, Thing clickedThing, FloatMenuContext context)
        {

        }
    }
    */

    [HarmonyPatch(typeof(Building_OutfitStand), "GetFloatMenuOptions")]
    public static class VanillaExpandedFramework_Building_OutfitStand_GetFloatMenuOptionToWear_Patch
    {
        public static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> __result,
            Building_OutfitStand __instance, Pawn selPawn)
        {
            var list = __result.ToList();
            foreach (Thing item in __instance.HeldItems)
            {
                if (item is Apparel_Shield shield)
                {
                    TaggedString toCheck = "ForceWear".Translate(shield.LabelCap, shield);
                    FloatMenuOption floatMenuOption = list.FirstOrDefault((FloatMenuOption x) =>
                    x.Label.Contains(toCheck));
                    if (floatMenuOption != null)
                    {
                        list.Remove(floatMenuOption);
                    }
                }
                if (item is Apparel equipment && equipment.TryGetComp<CompEquippable>() != null)
                {
                    TaggedString toCheck = "Equip".Translate(equipment.LabelShort);
                    FloatMenuOption floatMenuOption = __result.FirstOrDefault((FloatMenuOption x) => x.Label.Contains
                    (toCheck));
                    if (floatMenuOption != null && selPawn.equipment != null
                        && !equipment.def.UsableWithShields() && selPawn.OffHandShield() is Apparel_Shield oldShield)
                    {
                        floatMenuOption.Label += $" {"VanillaFactionsExpanded.EquipWarningShieldUnusableWithWeapon".Translate(oldShield.def.label)}";
                    }
                    AddShieldFloatMenuOption(selPawn, equipment, ref list);
                }
            }


            return list;
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
                        pawn.jobs.TryTakeOrderedJob(new Job(VEFDefOf.VFEC_EquipShield, equipment), JobTag.Misc);
                        FleckMaker.Static(equipment.DrawPos, equipment.MapHeld, FleckDefOf.FeedbackEquip, 1f);
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.EquippingWeapons, KnowledgeAmount.Total);
                    }, MenuOptionPriority.High, null, null, 0f, null, null), pawn, equipment, "ReservedBy");
                }
                opts.Add(shieldOption);
            }
        }
    }
}
