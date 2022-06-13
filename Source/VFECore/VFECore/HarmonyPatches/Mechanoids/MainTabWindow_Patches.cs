using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using VFECore;
using VFEMech;

namespace VFE.Mechanoids.HarmonyPatches
{
	[HarmonyPatch(typeof(MainTabWindow_Inspect), nameof(MainTabWindow_Inspect.DoInspectPaneButtons))]
	public static class MainTabWindow_Inspect_Renaming
	{
		/// Show a rename button in the inspect pane when a single drone is selected.
		public static void Postfix(Rect rect, ref float lineEndWidth)
		{
			// Only functioning drones belonging to the player faction can be renamed.
			if (Find.Selector.NumSelected != 1 || !(Find.Selector.SingleSelectedThing is Machine drone) ||
			    drone.health.Dead || drone.Faction != Faction.OfPlayer)
			{
				return;
			}

			const float renameSize = 30f;
			// See MainTabWindow_Inspect.DoInspectPaneButtons for the initial value of x.
			float x = rect.width - 48f - renameSize;

			Rect renameArea = new Rect(x, 0.0f, renameSize, renameSize);
			TooltipHandler.TipRegionByKey(renameArea, "Rename");
			if (Widgets.ButtonImage(renameArea, TexButton.Rename))
			{
				Find.WindowStack.Add(new Dialog_NameDrone(drone));
			}

			lineEndWidth += renameSize;
		}
	}
}