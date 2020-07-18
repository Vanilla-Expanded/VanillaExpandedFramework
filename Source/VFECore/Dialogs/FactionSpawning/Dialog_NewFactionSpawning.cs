using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFECore
{
	public class Dialog_NewFactionSpawning : Window
	{
		private FactionDef factionDef;
		private IEnumerator<FactionDef> factionEnumerator;
		private static Color colorCoreMod = new Color(125/255f, 97/255f, 51/255f);
		private static Color colorMod = new Color(115/255f, 162/255f, 47/255f);

		public static void OpenDialog(IEnumerator<FactionDef> enumerator)
		{
			Find.WindowStack.Add(new Dialog_NewFactionSpawning(enumerator));
		}

		private Dialog_NewFactionSpawning(IEnumerator<FactionDef> enumerator)
		{
			doCloseButton = false;
			forcePause = true;
			absorbInputAroundWindow = true;
			factionEnumerator = enumerator;
			factionDef = enumerator.Current;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.Begin(inRect.AtZero());

			// Icon
			if (factionDef.FactionIcon)
			{
				var rectIcon = listing_Standard.GetRect(64);
				var center = rectIcon.center.x;
				rectIcon.xMin = center - 32;
				rectIcon.xMax = center + 32;
				GUI.DrawTexture(rectIcon, factionDef.FactionIcon);
			}

			// Title
			Text.Font = GameFont.Medium;
			Text.Anchor = TextAnchor.MiddleCenter;
			listing_Standard.Label($"New faction: {factionDef.LabelCap}");
			listing_Standard.GapLine();

			// Description
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			var modName = GetModName();
			listing_Standard.Label($"This faction is added by {modName} and not currently present in your game.");
			if (factionDef.hidden)
			{
				listing_Standard.Label("This is a hidden faction and won't show up in your faction list.".Colorize(colorCoreMod));
				
				if (factionDef.requiredCountAtGameStart > 0)
				{
					listing_Standard.Label($"It is marked as required by {modName}.".Colorize(colorCoreMod));
				}
			}

			listing_Standard.Label("\n\nPlease select how to proceed:");
			listing_Standard.Gap(60);
			
			// Options
			if (factionDef.hidden)
			{
				if (listing_Standard.ButtonText("Add the faction")) SpawnWithoutBases();
			}
			else
			{
				if (listing_Standard.ButtonText("Add the faction with settlements")) SpawnWithBases();
			}

			if (listing_Standard.ButtonText("Do nothing")) Skip();
			GUI.color = new Color(1f, 0.3f, 0.35f);
			if (listing_Standard.ButtonText("Don't ask for this faction again")) Ignore();
			GUI.color = Color.white;

			listing_Standard.End();
		}

		private void SpawnWithBases()
		{
				Dialog_NewFactionSpawningSettlements.OpenDialog(SpawnCallback);

				void SpawnCallback(int amount, int minDistance)
				{
					try
					{
						NewFactionSpawningUtility.SpawnWithSettlements(factionDef, amount, minDistance, out var spawned);
						Messages.Message($"Added {factionDef.label} with {spawned} settlements.", MessageTypeDefOf.TaskCompletion);
						Close();
					}
					catch (Exception e)
					{
						Log.Error($"An error occurred when trying to spawn faction {factionDef?.defName}:\n{e.Message}\n{e.StackTrace}");
						Messages.Message("Failed to add the faction.", MessageTypeDefOf.RejectInput, false);
					}
				}
		}

		private void SpawnWithoutBases()
		{
			try
			{
				NewFactionSpawningUtility.SpawnWithoutSettlements(factionDef);
				Messages.Message($"Added {factionDef.label}.", MessageTypeDefOf.TaskCompletion);
				Close();
			}
			catch (Exception e)
			{
				Log.Error($"An error occurred when trying to spawn faction {factionDef?.defName}:\n{e.Message}\n{e.StackTrace}");
				Messages.Message("Failed to add the faction.", MessageTypeDefOf.RejectInput, false);
			}
		}

		private void Skip()
		{
			Close();
		}

		private void Ignore()
		{
			Find.World.GetComponent<NewFactionSpawningState>().Ignore(factionDef);
			Close();
		}

		public override void PostClose()
		{
			if (factionEnumerator.MoveNext())
			{
				OpenDialog(factionEnumerator);
			}
		}

		private string GetModName()
		{
			if (factionDef?.modContentPack == null) return "an unknown mod";
			if (factionDef.modContentPack.IsCoreMod) return factionDef.modContentPack.Name.Colorize(colorCoreMod);
			return factionDef.modContentPack.Name.Colorize(colorMod);
		}
	}
}