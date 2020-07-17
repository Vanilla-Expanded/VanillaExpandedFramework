using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFECore
{
	public class Dialog_NewFactionLoading : Window
	{
		private FactionDef faction;
		private static Color colorCoreMod = new Color(125/255f, 97/255f, 51/255f);
		private static Color colorMod = new Color(115/255f, 162/255f, 47/255f);
		private IEnumerator<FactionDef> factionEnumerator;

		public static void OpenDialog(IEnumerator<FactionDef> enumerator)
		{
			Find.WindowStack.Add(new Dialog_NewFactionLoading(enumerator));
		}

		private Dialog_NewFactionLoading(IEnumerator<FactionDef> enumerator)
		{
			doCloseButton = true;
			forcePause = true;
			absorbInputAroundWindow = true;
			factionEnumerator = enumerator;
			faction = enumerator.Current;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.Begin(inRect.AtZero());
			// Icon
			if (faction.FactionIcon)
			{
				var rectIcon = listing_Standard.GetRect(64);
				rectIcon.width = 64;
				GUI.DrawTexture(rectIcon, faction.FactionIcon);
			}
			// Title
			Text.Font = GameFont.Medium;
			Text.Anchor = TextAnchor.MiddleCenter;
			listing_Standard.Label($"New faction: {faction.LabelCap}");
			listing_Standard.GapLine();
			// Description
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			var modName = GetModName();
			listing_Standard.Label($"This faction is added by {modName} and not currently present in your game.");
			if (faction.hidden)
			{
				listing_Standard.Label("This is a hidden faction and won't show up in your faction list.".Colorize(colorCoreMod));
			}
			listing_Standard.Label("\n\nPlease select how to proceed:");
			listing_Standard.Gap(60);
			// Options
			if (listing_Standard.ButtonText("Add the faction with bases")) SpawnWithBases();
			if (listing_Standard.ButtonText("Add only the faction")) SpawnWithoutBases();
			if (listing_Standard.ButtonText("Do nothing")) Skip();
			GUI.color = new Color(1f, 0.3f, 0.35f);
			if (listing_Standard.ButtonText("Don't ask for this faction again")) DontAskAgain();
			GUI.color = Color.white;
			listing_Standard.End();
		}

		private void SpawnWithBases() { }

		private void SpawnWithoutBases() { }

		private void Skip()
		{
			Close();
		}

		private void DontAskAgain() { }

		public override void PostClose()
		{
			if (factionEnumerator.MoveNext())
			{
				OpenDialog(factionEnumerator);
			}
		}

		private string GetModName()
		{
			if (faction?.modContentPack == null) return "an unknown mod";
			if (faction.modContentPack.IsCoreMod) return faction.modContentPack.Name.Colorize(colorCoreMod);
			return faction.modContentPack.Name.Colorize(colorMod);
		}
	}
}