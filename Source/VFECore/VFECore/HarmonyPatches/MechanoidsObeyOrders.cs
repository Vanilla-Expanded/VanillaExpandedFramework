using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using VFE.Mechanoids.Needs;
using VFEMech;

namespace VFE.Mechanoids.HarmonyPatches
{

    [HarmonyPatch(typeof(FloatMenuMakerMap), "CanTakeOrder")]
    public static class MechanoidsObeyOrders
    {
        public static void Postfix(Pawn pawn, ref bool __result)
        {
            if (pawn.drafter != null && pawn is Machine)
                __result = true;
        }
    }

    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddDraftedOrders")]
    public static class AddDraftedOrders_Patch
    {
        public static bool Prefix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            if (!AnimalBehaviours.AnimalCollectionClass.draftable_animals.Contains(pawn) && pawn.RaceProps.IsMechanoid 
                && pawn.needs.TryGetNeed<Need_Power>() is Need_Power need && need.CurLevel <= 0f)
            {
                return false;
            }
            return true;
        }
    }

	[StaticConstructorOnStartup]
	public static class SimpleSidearmsPatch
    {

		public static bool SimpleSidearmsActive;
		static SimpleSidearmsPatch()
        {
			SimpleSidearmsActive = ModsConfig.IsActive("PeteTimesSix.SimpleSidearms");
			if (SimpleSidearmsActive)
            {
				var type = AccessTools.TypeByName("PeteTimesSix.SimpleSidearms.Extensions");
				if (type != null)
				{
					var target = AccessTools.Method(type, "IsValidSidearmsCarrier");
					VFECore.VFECore.harmonyInstance.Patch(target, postfix: new HarmonyMethod(AccessTools.Method(typeof(SimpleSidearmsPatch), nameof(IsValidSidearmsCarrierPostfix))));
					type = AccessTools.TypeByName("SimpleSidearms.rimworld.CompSidearmMemory");
					target = AccessTools.Method(type, "GetMemoryCompForPawn");
					VFECore.VFECore.harmonyInstance.Patch(target, prefix: new HarmonyMethod(AccessTools.Method(typeof(SimpleSidearmsPatch), nameof(GetMemoryCompForPawnPrefix))));
					type = AccessTools.TypeByName("SimpleSidearms.rimworld.Gizmo_SidearmsList");
					target = AccessTools.Method(type, "DrawGizmoLabel");
					VFECore.VFECore.harmonyInstance.Patch(target, prefix: new HarmonyMethod(AccessTools.Method(typeof(SimpleSidearmsPatch), nameof(GizmoLabelFixer))));
				}
				else
                {
					Log.Error("[Vanilla Expanded Framework] Patching Simple Sidearms failed.");
                }
			}
        }
		public static void IsValidSidearmsCarrierPostfix(ref bool __result, Pawn pawn)
        {
			if (!__result)
            {
				var compMachine = pawn.GetComp<CompMachine>();
				if (compMachine != null && compMachine.Props.canPickupWeapons)
                {
					__result = true;
                }
			}
        }

		public static void GizmoLabelFixer(ref string labelText, Rect gizmoRect)
        {
			labelText = labelText.Replace(" (godmode)", "");

		}
		public static IEnumerable<Gizmo> SimpleSidearmsGizmos(Pawn __instance)
		{
			if (PeteTimesSix.SimpleSidearms.Extensions.IsValidSidearmsCarrier(__instance) && __instance.equipment != null && __instance.inventory != null)
			{
				IEnumerable<ThingWithComps> carriedWeapons = PeteTimesSix.SimpleSidearms.Extensions.getCarriedWeapons(__instance, includeEquipped: true, includeTools: true);
				SimpleSidearms.rimworld.CompSidearmMemory pawnMemory = SimpleSidearms.rimworld.CompSidearmMemory.GetMemoryCompForPawn(__instance);
				if (pawnMemory != null)
				{
					List<SimpleSidearms.rimworld.ThingDefStuffDefPair> rangedWeaponMemories = new List<SimpleSidearms.rimworld.ThingDefStuffDefPair>();
					List<SimpleSidearms.rimworld.ThingDefStuffDefPair> meleeWeaponMemories = new List<SimpleSidearms.rimworld.ThingDefStuffDefPair>();
					foreach (SimpleSidearms.rimworld.ThingDefStuffDefPair weapon in pawnMemory.RememberedWeapons)
					{
						if (weapon.thing.IsMeleeWeapon)
						{
							meleeWeaponMemories.Add(weapon);
						}
						else if (weapon.thing.IsRangedWeapon)
						{
							rangedWeaponMemories.Add(weapon);
						}
					}
					yield return new SimpleSidearms.rimworld.Gizmo_SidearmsList(__instance, carriedWeapons, pawnMemory.RememberedWeapons);
				}
			}
		}
		public static bool GetMemoryCompForPawnPrefix(ref object __result, Pawn pawn, bool fillExistingIfCreating = true)
		{
			var compMachine = pawn.GetComp<CompMachine>();
			if (compMachine != null && compMachine.Props.canPickupWeapons)
			{
				__result = pawn.TryGetComp<SimpleSidearms.rimworld.CompSidearmMemory>();
				return false;
			}
			return true;
		}
	}

    [HarmonyPatch(typeof(FloatMenuMakerMap), "ChoicesAtFor")]
    public static class FloatMenuMakerMap_ChoicesAtFor_Patch
    {
        public static void Postfix(ref List<FloatMenuOption> __result, Vector3 clickPos, Pawn pawn, bool suppressAutoTakeableGoto = false)
        {
            if (!pawn.RaceProps.Humanlike)
            {
                var compMachine = pawn.GetComp<CompMachine>();
                if (compMachine != null && compMachine.Props.canPickupWeapons)
				{
					Log.Message(pawn.inventory + " - " + pawn.equipment);
					IntVec3 c = IntVec3.FromVector3(clickPos);
					ThingWithComps equipment = null;
					List<Thing> thingList2 = c.GetThingList(pawn.Map);
					for (int i = 0; i < thingList2.Count; i++)
					{
						if (thingList2[i].TryGetComp<CompEquippable>() != null)
						{
							equipment = (ThingWithComps)thingList2[i];
							break;
						}
					}
					if (equipment != null)
					{
						string labelShort = equipment.LabelShort;
						FloatMenuOption item6;
						string cantReason;
						if (equipment.def.IsWeapon && pawn.WorkTagIsDisabled(WorkTags.Violent))
						{
							item6 = new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "IsIncapableOfViolenceLower".Translate(pawn.LabelShort, pawn), null);
						}
						else if (equipment.def.IsRangedWeapon && pawn.WorkTagIsDisabled(WorkTags.Shooting))
						{
							item6 = new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "IsIncapableOfShootingLower".Translate(pawn), null);
						}
						else if (!pawn.CanReach(equipment, PathEndMode.ClosestTouch, Danger.Deadly))
						{
							item6 = new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
						}
						else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
						{
							item6 = new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "Incapable".Translate(), null);
						}
						else if (equipment.IsBurning())
						{
							item6 = new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "BurningLower".Translate(), null);
						}
						else if (pawn.IsQuestLodger() && !EquipmentUtility.QuestLodgerCanEquip(equipment, pawn))
						{
							item6 = new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "QuestRelated".Translate().CapitalizeFirst(), null);
						}
						else if (!EquipmentUtility.CanEquip(equipment, pawn, out cantReason, checkBonded: false))
						{
							item6 = new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + cantReason.CapitalizeFirst(), null);
						}
						else
						{
							string text4 = "Equip".Translate(labelShort);
							if (equipment.def.IsRangedWeapon && pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.Brawler))
							{
								text4 += " " + "EquipWarningBrawler".Translate();
							}
							if (EquipmentUtility.AlreadyBondedToWeapon(equipment, pawn))
							{
								text4 += " " + "BladelinkAlreadyBonded".Translate();
								TaggedString dialogText = "BladelinkAlreadyBondedDialog".Translate(pawn.Named("PAWN"), equipment.Named("WEAPON"), pawn.equipment.bondedWeapon.Named("BONDEDWEAPON"));
								item6 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text4, delegate
								{
									Find.WindowStack.Add(new Dialog_MessageBox(dialogText));
								}, MenuOptionPriority.High), pawn, equipment);
							}
							else
							{
								item6 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text4, delegate
								{
									string personaWeaponConfirmationText = EquipmentUtility.GetPersonaWeaponConfirmationText(equipment, pawn);
									if (!personaWeaponConfirmationText.NullOrEmpty())
									{
										Find.WindowStack.Add(new Dialog_MessageBox(personaWeaponConfirmationText, "Yes".Translate(), delegate
										{
											Equip();
										}, "No".Translate()));
									}
									else
									{
										Equip();
									}
								}, MenuOptionPriority.High), pawn, equipment);
							}
						}
						__result.Add(item6);
					}
					void Equip()
					{
						equipment.SetForbidden(value: false);
						pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Equip, equipment), JobTag.Misc);
						FleckMaker.Static(equipment.DrawPos, equipment.MapHeld, FleckDefOf.FeedbackEquip);
						PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.EquippingWeapons, KnowledgeAmount.Total);
					}
				}
            }
        }
    }

    [HarmonyPatch(typeof(WanderUtility), "GetColonyWanderRoot")]
    public static class GetColonyWanderRoot_Patch
    {
        public static void Postfix(ref IntVec3 __result, Pawn pawn)
        {
            try
            {
                if (pawn.Map != null && pawn.RaceProps.IsMechanoid && pawn.Faction == Faction.OfPlayer && __result.IsForbidden(pawn) && pawn.playerSettings?.EffectiveAreaRestrictionInPawnCurrentMap.ActiveCells.Count() > 0)
                {
                    __result = pawn.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap.ActiveCells.OrderBy(x => x.DistanceTo(pawn.Position))
                        .Where(x => x.Walkable(pawn.Map) && pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.Deadly)).Take(10).RandomElement();
                }
            }
            catch { }
        }
    }
}
