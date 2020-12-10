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

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(1000f, 600f);
			}
		}

		public Dialog_ExportWindow(Map map, XElement structureL, List<XElement> symbols)
		{
			this.map = map;
			this.structureL = structureL;
			this.symbols = symbols;

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

			Widgets.DrawBoxSolid(new Rect(0, 0, 890, 50), boxColor);
			Widgets.Label(new Rect(0, 0, 890, 50), "Custom Structure Generation - Export Menu");

			Widgets.DrawBoxSolid(new Rect(900, 0, 60, 50), boxColor);
			if (Widgets.ButtonImage(new Rect(910, 5, 40, 40), TextureLoader.helpIcon))
            {
				System.Diagnostics.Process.Start("https://github.com/AndroidQuazar/VanillaExpandedFramework/wiki");
            }

			Text.Anchor = TextAnchor.UpperLeft;
		}

		private void DrawFooter(Rect inRect)
        {
			int bHeight = 35;

			if (Widgets.ButtonText(new Rect(0, inRect.height - bHeight, 440, bHeight), "Copy structure def"))
			{
				GUIUtility.systemCopyBuffer = structureL.ToString();
				Messages.Message("Copied to clipboard.", MessageTypeDefOf.TaskCompletion);
			}
			if (Widgets.ButtonText(new Rect(450, inRect.height - bHeight, 440, bHeight), "Copy symbol(s) def(s)"))
			{
                string toCopy = "";
                foreach (XElement item in this.symbols)
                {
                    toCopy += item.ToString() + "\n";
                }
				GUIUtility.systemCopyBuffer = toCopy;
				Messages.Message("Copied to clipboard.", MessageTypeDefOf.TaskCompletion);
			}
			if (Widgets.ButtonText(new Rect(900, inRect.height - bHeight, 60, bHeight), "Close"))
			{
				this.Close();
			}
		}


		private void DrawDefNameChanger()
        {
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(new Rect(10, 100, 200, 35), "Structure defName:");
			defname = Widgets.TextField(new Rect(210, 100, 200, 35), defname);
			structureL.SetElementValue("defName", defname);
			Text.Anchor = TextAnchor.UpperLeft;
		}


		public override void DoWindowContents(Rect inRect)
		{
			this.DrawHeader();
			Text.Font = GameFont.Small;

			this.DrawDefNameChanger();

			this.DrawFooter(inRect);
		}
	}
}
