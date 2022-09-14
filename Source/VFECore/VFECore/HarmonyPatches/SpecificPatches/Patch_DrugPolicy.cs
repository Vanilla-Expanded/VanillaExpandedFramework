using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VFECore
{
	/// <summary>
	/// This adds new drugs to existing drug policies after load.
	/// </summary>
	internal static class Patch_DrugPolicy
	{
		[HarmonyPatch(typeof(DrugPolicy), nameof(DrugPolicy.ExposeData))]
		public class ExposeData
		{
			[HarmonyPostfix]
			internal static void Prefix(DrugPolicy __instance, List<DrugPolicyEntry> ___entriesInt)
			{
				if (Scribe.mode != LoadSaveMode.PostLoadInit) return;

				var allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
				foreach (var t in allDefsListForReading)
				{
					if (t.category == ThingCategory.Item && t.IsDrug && !___entriesInt.Exists(e=>e.drug == t))
					{
						DrugPolicyEntry drugPolicyEntry = new DrugPolicyEntry {drug = t, allowedForAddiction = true};
						___entriesInt.Add(drugPolicyEntry);
						//Log.Message($"Added {t.label} to drug policy {__instance.label}.");
					}
				}

				___entriesInt.RemoveAll(e => e?.drug?.GetCompProperties<CompProperties_Drug>() == null);
				___entriesInt.SortBy(e => e.drug.GetCompProperties<CompProperties_Drug>().listOrder);
			}
		}
	}
}
