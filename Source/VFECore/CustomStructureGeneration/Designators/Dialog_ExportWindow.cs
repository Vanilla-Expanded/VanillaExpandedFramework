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
		private XElement structureL;
		private List<XElement> symbols;
		private Map map;

		private Color boxColor = new Color(0.13f, 0.14f, 0.16f);

		private string defname;
		private bool needRoyalty = false;

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(800f, 300f);
			}
		}

		public Dialog_ExportWindow(Map map, XElement structureL, List<XElement> symbols)
		{
			this.map = map;
			this.structureL = structureL;
			this.symbols = symbols;

			this.defname = "Placeholder";

			this.forcePause = true;
			this.doCloseX = false;
			this.doCloseButton = false;
			this.closeOnClickedOutside = false;
			this.absorbInputAroundWindow = false;
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
				GUIUtility.systemCopyBuffer = structureL.ToString();
				Messages.Message("Copied to clipboard.", MessageTypeDefOf.TaskCompletion);
			}
			if (Widgets.ButtonText(new Rect(350, inRect.height - bHeight, 340, bHeight), "Copy symbol(s) def(s)"))
			{
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
			if (Widgets.ButtonText(new Rect(700, inRect.height - bHeight, 60, bHeight), "Close"))
			{
				this.Close();
			}
		}


		private void DrawDefNameChanger()
        {
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(new Rect(10, 100, 200, 35), "Structure defName:");
			defname = Widgets.TextField(new Rect(220, 100, 480, 35), defname);
			structureL.SetElementValue("defName", defname);
			Text.Anchor = TextAnchor.UpperLeft;
		}
		
		private void DrawRoyaltyChanger()
        {
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(new Rect(10, 140, 200, 35), "Structure need royalty dlc:");
			Widgets.Checkbox(220, 140, ref this.needRoyalty);
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
			Text.Anchor = TextAnchor.UpperLeft;
		}

		public override void DoWindowContents(Rect inRect)
		{
			this.DrawHeader();
			Text.Font = GameFont.Small;

			this.DrawDefNameChanger();

			this.DrawRoyaltyChanger();

			this.DrawFooter(inRect);
		}
	}
}
