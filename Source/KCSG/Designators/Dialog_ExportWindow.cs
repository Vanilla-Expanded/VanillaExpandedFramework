using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using UnityEngine;
using Verse;

namespace KCSG
{
    class Dialog_ExportWindow : Window
    {
		// Export
		private XElement structureL;
		private List<XElement> symbols;
		private Map map;
		private List<IntVec3> cells = new List<IntVec3>();
		private Dictionary<IntVec3, List<Thing>> pairsCellThingList = new Dictionary<IntVec3, List<Thing>>();
		// Colors
		private Color boxColor = new Color(0.13f, 0.14f, 0.16f);
		// Settings
		private string defname = "Placeholder";
		private bool needRoyalty = false;
		private bool isStorage = false;
		private List<string> tags = new List<string>();
		private string tempTagToAdd = "Optional, see tutorial";
		private string symbolPrefix = "Required";
		// Size
		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(800f, 800f);
			}
		}

		public Dialog_ExportWindow(Map map, List<IntVec3> cells)
		{
			this.map = map;
			this.cells = cells;
			KCSG_Utilities.FillCellThingsList(cells, this.map, this.pairsCellThingList);
			// Window settings
			this.forcePause = true;
			this.doCloseX = false;
			this.doCloseButton = false;
			this.closeOnClickedOutside = false;
			this.absorbInputAroundWindow = true;
		}

		private void CreateLayoutSymbols()
        {
			this.symbols.Clear();
			// Base
			this.symbols = KCSG_Utilities.CreateSymbolIfNeeded(this.cells, this.map, symbolPrefix, pairsCellThingList);
			this.structureL = KCSG_Utilities.CreateStructureDef(this.cells, this.map, symbolPrefix, KCSG_Utilities.FillpairsSymbolLabel(), pairsCellThingList);
			// Defname change
			structureL.SetElementValue("defName", defname);
			// Royalty change
			if (this.needRoyalty)
			{
				if (structureL.Element("requireRoyalty") == null)
				{
					structureL.Add(new XElement("requireRoyalty", true));
				}
			}
			else
			{
				if (structureL.Element("requireRoyalty") != null)
				{
					structureL.Element("requireRoyalty").Remove();
				}
			}
			// isStorage change
			if (this.isStorage)
			{
				if (structureL.Element("isStorage") == null)
				{
					structureL.Add(new XElement("isStorage", true));
				}
			}
			else
			{
				if (structureL.Element("isStorage") != null)
				{
					structureL.Element("isStorage").Remove();
				}
			}
			// Tags changes
			if (tags.Count > 0)
            {
				XElement temp1 = new XElement("tags");
				foreach (var item in this.tags)
				{
					temp1.Add(new XElement("li", item));
				}
				structureL.Add(temp1);
			}
		}

		private void DrawHeader()
        {
			Text.Font = GameFont.Medium;
			Text.Anchor = TextAnchor.MiddleCenter;

			Widgets.DrawBoxSolid(new Rect(0, 0, 700, 50), boxColor);
			Widgets.Label(new Rect(0, 0, 700, 50), "Custom Structure Generation - Export Menu");

			Widgets.DrawBoxSolid(new Rect(710, 0, 50, 50), boxColor);
			if (Widgets.ButtonImage(new Rect(715, 5, 40, 40), TextureLoader.helpIcon))
            {
				System.Diagnostics.Process.Start("https://github.com/AndroidQuazar/VanillaExpandedFramework/wiki/Exporting-your-own-structures");
            }

			Text.Anchor = TextAnchor.UpperLeft;
		}

		private void DrawFooter(Rect inRect)
        {
			int bHeight = 35;

			if (Widgets.ButtonText(new Rect(0, inRect.height - bHeight, 340, bHeight), "Copy structure def"))
			{
				if (this.symbolPrefix.Length == 0 || this.symbolPrefix == "Required")
				{
					Messages.Message("Custom symbols prefix is required.", MessageTypeDefOf.NegativeEvent);
				}
				else
				{
					this.CreateLayoutSymbols();
					GUIUtility.systemCopyBuffer = structureL.ToString();
					Messages.Message("Copied to clipboard.", MessageTypeDefOf.TaskCompletion);
				}
			}
			if (Widgets.ButtonText(new Rect(350, inRect.height - bHeight, 340, bHeight), "Copy symbol(s) def(s)"))
			{
				if (this.symbolPrefix.Length == 0 || this.symbolPrefix == "Required")
				{
					Messages.Message("Custom symbols prefix is required.", MessageTypeDefOf.NegativeEvent);
				}
				else
				{
					this.CreateLayoutSymbols();

					if (this.symbols.Count > 0)
					{
						string toCopy = "";
						foreach (XElement item in this.symbols)
						{
							toCopy += item.ToString() + "\n";
						}
						GUIUtility.systemCopyBuffer = toCopy;
						Messages.Message("Copied to clipboard.", MessageTypeDefOf.TaskCompletion);
					}
					else Messages.Message("No new symbols needed.", MessageTypeDefOf.TaskCompletion);
				}
			}
			if (Widgets.ButtonText(new Rect(700, inRect.height - bHeight, 60, bHeight), "Close"))
			{
				this.Close();
			}
		}

		private void DrawSymbolPrefix(Rect inRect)
        {
			Widgets.Label(new Rect(10, inRect.height - 90, 200, 35), "Symbols defName/symbol prefix:");
			symbolPrefix = Widgets.TextField(new Rect(220, inRect.height - 90, 480, 35), symbolPrefix);
		}

		private void DrawDefNameChanger(float y)
        {
			Widgets.Label(new Rect(10, y, 200, 35), "Structure defName:");
			Text.Anchor = TextAnchor.MiddleCenter;
			defname = Widgets.TextField(new Rect(220, y, 480, 35), defname);
			Text.Anchor = TextAnchor.UpperLeft;
		}
		
		private void DrawRoyaltyChanger(float y)
        {
			Widgets.Label(new Rect(10, y, 200, 35), "Structure need royalty dlc:");
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Checkbox(220, y, ref this.needRoyalty);
			Text.Anchor = TextAnchor.UpperLeft;
		}
		
		private void DrawStorageChanger(float y)
        {
			Widgets.Label(new Rect(10, y, 200, 35), "Structure is stockpile:");
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Checkbox(220, y, ref this.isStorage);
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private void DrawTagsEditing(float y)
        {
			Widgets.Label(new Rect(10, y, 200, 35), "Structure tags:");
			tempTagToAdd = Widgets.TextField(new Rect(220, y, 270, 35), tempTagToAdd);
			Text.Anchor = TextAnchor.MiddleCenter;
			if (Widgets.ButtonText(new Rect(500, y, 200, 35), "Add tag"))
			{
				this.tags.Add(tempTagToAdd);
			}

			float tagY = y + 40f;
			foreach (string tag in this.tags)
            {
				Widgets.Label(new Rect(220, tagY, 270, 35), tag);
				if (Widgets.ButtonText(new Rect(500, tagY, 200, 35), "Remove tag"))
				{
					this.tags.Remove(tag);
					break;
				}
				tagY += 40;
			}
			Text.Anchor = TextAnchor.UpperLeft;
		}

		public override void DoWindowContents(Rect inRect)
		{
			this.DrawHeader();
			Text.Font = GameFont.Small;

			this.DrawDefNameChanger(90);
			this.DrawRoyaltyChanger(130);
			this.DrawStorageChanger(170);
			this.DrawTagsEditing(210);

			this.DrawSymbolPrefix(inRect);
			this.DrawFooter(inRect);
		}
	}
}
