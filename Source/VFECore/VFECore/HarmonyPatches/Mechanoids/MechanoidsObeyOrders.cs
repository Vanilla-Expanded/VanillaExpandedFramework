using AnimalBehaviours;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.AI;
using VFE.Mechanoids.Needs;
using VFEMech;

namespace VFE.Mechanoids.HarmonyPatches
{
	[StaticConstructorOnStartup]
	public static class MechanoidDraftCompInitializer
	{
		static MechanoidDraftCompInitializer()
		{
			foreach (var pawn in DefDatabase<PawnKindDef>.AllDefs)
			{
				var compPropsMachine = pawn.race.GetCompProperties<CompProperties_Machine>();
				if (compPropsMachine != null && compPropsMachine.violent)
				{
					if (pawn.race.GetCompProperties<CompProperties_Draftable>() is null)
					{
						pawn.race.comps.Add(new CompProperties_Draftable());
					}
				}
			}
		}
	}

	[HarmonyPatch(typeof(MechanitorUtility), "InMechanitorCommandRange")]
	public static class MechanitorUtility_InMechanitorCommandRange_Patch
	{
		public static void Postfix(Pawn mech, ref bool __result)
		{
			if (mech is Machine)
			{
				__result = true;
			}
		}
	}

	[HarmonyPatch(typeof(FloatMenuMakerMap), "CanTakeOrder")]
	public static class MechanoidsObeyOrders
	{
		public static void Postfix(Pawn pawn, ref bool __result)
		{
			if (!__result && pawn.drafter != null && pawn is Machine && pawn.Faction != null && pawn.Faction.IsPlayer)
			{
				__result = true;
			}
		}
	}

	[HarmonyPatch(typeof(Selector), "SelectInsideDragBox")]
	public static class Selector_SelectInsideDragBox_Patch
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
		{
			var codes = codeInstructions.ToList();
			foreach (var code in codes)
			{
				yield return code;
				if (code.opcode == OpCodes.Stloc_3)
				{
					yield return new CodeInstruction(OpCodes.Ldloc_3);
					yield return new CodeInstruction(OpCodes.Call,
						AccessTools.Method(typeof(Selector_SelectInsideDragBox_Patch), nameof(WrappedPredicator)));
					yield return new CodeInstruction(OpCodes.Stloc_3);
				}
			}
		}

		public static Predicate<Thing> WrappedPredicator(Predicate<Thing> predicate)
		{
			bool wrappedPredicate(Thing t)
			{
				bool result = predicate(t);
				if (!result)
				{
					if (t is Pawn pawn)
					{
						if (pawn.Faction == Faction.OfPlayer && pawn is Machine)
						{
							return true;
						}
					}
				}
				return result;
			}
			return wrappedPredicate;
		}
	}

	[HarmonyPatch(typeof(Selector), "SelectAllMatchingObjectUnderMouseOnScreen")]
	public static class Selector_SelectAllMatchingObjectUnderMouseOnScreen_Patch
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
		{
			var codes = codeInstructions.ToList();
			var clickedThingField = typeof(Selector).GetNestedTypes(AccessTools.all).SelectMany(x => x.GetFields(AccessTools.all))
				.FirstOrDefault(x => x.Name == "clickedThing");
			foreach (var code in codes)
			{
				yield return code;
				if (code.opcode == OpCodes.Stloc_3)
				{
					yield return new CodeInstruction(OpCodes.Ldloc_0);
					yield return new CodeInstruction(OpCodes.Ldfld, clickedThingField);
					yield return new CodeInstruction(OpCodes.Ldloc_3);
					yield return new CodeInstruction(OpCodes.Call,
						AccessTools.Method(typeof(Selector_SelectAllMatchingObjectUnderMouseOnScreen_Patch), nameof(WrappedPredicator)));
					yield return new CodeInstruction(OpCodes.Stloc_3);
				}
			}
		}

		public static Predicate<Thing> WrappedPredicator(Thing clickedThing, Predicate<Thing> predicate)
		{
			bool wrappedPredicate(Thing t)
			{
				bool result = predicate(t);
				if (!result)
				{
					if (t is Pawn pawn2 && clickedThing is Pawn pawn1)
					{
						if (pawn2.Faction == Faction.OfPlayer && pawn1.Faction == Faction.OfPlayer && (pawn2 is Machine || pawn1 is Machine))
						{
							return true;
						}
					}
				}
				return result;
			}
			return wrappedPredicate;
		}
	}

	[HarmonyPatch(typeof(FloatMenuMakerMap), "AddDraftedOrders")]
	public static class AddDraftedOrders_Patch
	{
		public static bool Prefix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
		{
			if (pawn is Machine && pawn.needs.TryGetNeed<Need_Power>() is Need_Power need && need.CurLevel <= 0f)
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
				var gizmo = GetSimpleSidearmsGizmo(__instance);
				if (gizmo != null)
				{
					yield return gizmo;
				}
			}
		}

		public static Gizmo GetSimpleSidearmsGizmo(Pawn __instance)
		{
			var carriedWeapons = PeteTimesSix.SimpleSidearms.Extensions.getCarriedWeapons(__instance, includeEquipped: true, includeTools: true);
			var pawnMemory = SimpleSidearms.rimworld.CompSidearmMemory.GetMemoryCompForPawn(__instance);
			if (pawnMemory != null)
			{
				var rangedWeaponMemories = new List<SimpleSidearms.rimworld.ThingDefStuffDefPair>();
				var meleeWeaponMemories = new List<SimpleSidearms.rimworld.ThingDefStuffDefPair>();
				foreach (var weapon in pawnMemory.RememberedWeapons)
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
				return new SimpleSidearms.rimworld.Gizmo_SidearmsList(__instance, carriedWeapons, pawnMemory.RememberedWeapons);
			}
			return null;
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
					var c = IntVec3.FromVector3(clickPos);
					ThingWithComps equipment = null;
					var thingList2 = c.GetThingList(pawn.Map);
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
						else if (!EquipmentUtility.CanEquip(equipment, pawn, out string cantReason, checkBonded: false))
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
								var dialogText = "BladelinkAlreadyBondedDialog".Translate(pawn.Named("PAWN"), equipment.Named("WEAPON"), pawn.equipment.bondedWeapon.Named("BONDEDWEAPON"));
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
						if (SimpleSidearmsPatch.SimpleSidearmsActive)
						{
							AppendSidearmsOptions(pawn, equipment, ref __result);
						}
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

		public static void AppendSidearmsOptions(Pawn pawn, ThingWithComps equipment, ref List<FloatMenuOption> __result)
		{
			try
			{
				string labelShort = equipment.LabelShort;
				if (!pawn.CanReach(new LocalTargetInfo(equipment), PathEndMode.ClosestTouch, Danger.Deadly) || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || !equipment.def.IsWeapon || equipment.IsBurning() || pawn.IsQuestLodger())
				{
					return;
				}
				FloatMenuOption item3;
				if (!PeteTimesSix.SimpleSidearms.Utilities.StatCalculator.canCarrySidearmInstance(equipment, pawn, out string errStr))
				{
					"CannotEquip".Translate();
					item3 = new FloatMenuOption("CannotEquip".Translate(labelShort) + " (" + errStr + ")", null);
					__result.Add(item3);
					return;
				}
				string text2 = "Equip".Translate(labelShort);
				text2 = ((pawn.CombinedDisabledWorkTags & WorkTags.Violent) == 0 && !PeteTimesSix.SimpleSidearms.Extensions.isToolNotWeapon
					(PeteTimesSix.SimpleSidearms.Extensions.toThingDefStuffDefPair(equipment))) ? ((string)(text2 + "AsSidearm".Translate())) : ((string)(text2 + "AsTool".Translate()));
				if (equipment.def.IsRangedWeapon && pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.Brawler))
				{
					text2 = text2 + " " + "EquipWarningBrawler".Translate();
				}
				item3 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text2, delegate
				{
					equipment.SetForbidden(value: false);
					pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(SimpleSidearms.rimworld.SidearmsDefOf.EquipSecondary, equipment), JobTag.Misc);
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(SimpleSidearms.rimworld.SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);
				}, MenuOptionPriority.High), pawn, equipment);
				__result.Add(item3);
			}
			catch (Exception e)
			{
				Log.Error("Exception during SimpleSidearms floatmenumaker intercept. Cancelling intercept. Exception: " + e.ToString());
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
				if (pawn.Map != null && pawn is Machine && pawn.Faction == Faction.OfPlayer && __result.IsForbidden(pawn) && pawn.playerSettings?.EffectiveAreaRestrictionInPawnCurrentMap.ActiveCells.Count() > 0)
				{
					__result = pawn.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap.ActiveCells.OrderBy(x => x.DistanceTo(pawn.Position))
						.Where(x => x.Walkable(pawn.Map) && pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.Deadly)).Take(10).RandomElement();
				}
			}
			catch { }
		}
	}
}
