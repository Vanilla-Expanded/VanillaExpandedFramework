using RimWorld;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using Verse;

namespace KCSG
{
    internal class Dialog_ExportWindow : Window
    {
        private readonly Area area;
        private readonly List<IntVec3> cells = new List<IntVec3>();
        private readonly Map map;
        private readonly Dictionary<IntVec3, List<Thing>> pairsCellThingList = new Dictionary<IntVec3, List<Thing>>();
        private readonly List<string> tags = new List<string>();
        private readonly List<string> mods = new List<string>();

        private Color boxColor = new Color(0.13f, 0.14f, 0.16f);
        private string defname = "Placeholder";
        private bool isStorage = false;
        private bool spawnConduits = true;
        private bool exportFilth = false;

        private string structurePrefix = "Required";
        private List<XElement> symbols = new List<XElement>();
        private string tempTagToAdd = "Optional";
        private string modIdToAdd = "Optional";

        private Vector2 scrollPosition = Vector2.zero;

        public Dialog_ExportWindow(Map map, List<IntVec3> cells, Area area)
        {
            this.map = map;
            this.cells = cells;
            this.area = area;
            // Window settings
            this.forcePause = true;
            this.doCloseX = false;
            this.doCloseButton = false;
            this.closeOnClickedOutside = false;
            this.absorbInputAroundWindow = true;

            if (!SymbolDefsCreator.defCreated) SymbolDefsCreator.Run();
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(800f, 800f);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            this.DrawHeader();
            Text.Font = GameFont.Small;
            Rect scrollRect = new Rect(inRect.x, inRect.y + 80f, inRect.width, inRect.height - 150f);
            Rect viewRect = new Rect(inRect.x, inRect.y + 80f, inRect.width - 20f, scrollRect.height);
            Listing_Standard lst = new Listing_Standard();
            if (viewRect.height < viewRect.height + mods.Count * 12) viewRect.height += mods.Count * 12;
            if (viewRect.height < viewRect.height + tags.Count * 12) viewRect.height += tags.Count * 12;

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.BeginScrollView(scrollRect, ref scrollPosition, viewRect);
            lst.Begin(viewRect);

            this.DrawDefNameChanger(lst);
            this.DrawStorageChanger(lst);
            this.DrawSpawnConduitChanger(lst);
            this.DrawExportFilth(lst);
            this.DrawStructurePrefix(lst);
            this.DrawTagsEditing(lst);
            this.DrawModsEditing(lst);

            lst.End();
            Widgets.EndScrollView();
            this.DrawFooter(inRect);
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private XElement CreateLayout()
        {
            XElement structureL = LayoutUtils.CreateStructureDef(this.cells, this.map, pairsCellThingList, area, exportFilth);
            // Defname change
            XElement defName = new XElement("defName", structurePrefix + defname);
            structureL.AddFirst(defName);
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
            // spawnConduits
            if (!this.spawnConduits)
            {
                if (structureL.Element("spawnConduits") == null)
                {
                    structureL.Add(new XElement("spawnConduits", false));
                }
            }
            else
            {
                if (structureL.Element("spawnConduits") != null)
                {
                    structureL.Element("spawnConduits").Remove();
                }
            }
            // Tags changes
            if (tags.Count > 0)
            {
                XElement temp = new XElement("tags");
                foreach (var item in this.tags)
                {
                    temp.Add(new XElement("li", item));
                }
                structureL.Add(temp);
            }
            // Mods
            if (mods.Count > 0)
            {
                XElement temp = new XElement("modRequirements");
                foreach (var item in this.mods)
                {
                    temp.Add(new XElement("li", item));
                }
                structureL.Add(temp);
            }
            return structureL;
        }

        private void DrawDefNameChanger(Listing_Standard lst)
        {
            lst.Label("Structure defName:");
            defname = lst.TextEntry(defname);
            lst.Gap();
        }

        private void DrawFooter(Rect inRect)
        {
            int bHeight = 35;

            if (Widgets.ButtonText(new Rect(0, inRect.height - bHeight, 340, bHeight), "Copy structure def"))
            {
                if (this.structurePrefix.Length == 0 || this.structurePrefix == "Required")
                {
                    Messages.Message("Structure prefix required.", MessageTypeDefOf.NegativeEvent);
                }
                else
                {
                    LayoutUtils.FillCellThingsList(cells, this.map, this.pairsCellThingList);
                    GUIUtility.systemCopyBuffer = this.CreateLayout().ToString();
                    Messages.Message("Copied to clipboard.", MessageTypeDefOf.TaskCompletion);
                }
            }
            if (Widgets.ButtonText(new Rect(350, inRect.height - bHeight, 340, bHeight), "Copy symbol(s) def(s)"))
            {
                LayoutUtils.FillCellThingsList(cells, this.map, this.pairsCellThingList);
                this.symbols = SymbolUtils.CreateSymbolIfNeeded(this.cells, this.map, pairsCellThingList, area);
                if (this.symbols.Count > 0)
                {
                    string toCopy = "";
                    foreach (XElement item in this.symbols)
                    {
                        toCopy += item.ToString() + "\n\n";
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

        private void DrawHeader()
        {
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;

            Widgets.DrawBoxSolid(new Rect(0, 0, 700, 50), boxColor);
            Widgets.Label(new Rect(0, 0, 700, 50), "KCSG - Export Menu");

            Widgets.DrawBoxSolid(new Rect(710, 0, 50, 50), boxColor);
            Rect infoRect = new Rect(715, 5, 40, 40);
            if (Widgets.ButtonImage(infoRect, TextureLoader.helpIcon))
            {
                System.Diagnostics.Process.Start("https://github.com/AndroidQuazar/VanillaExpandedFramework/wiki/Exporting-your-own-structures");
            }

            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DrawStorageChanger(Listing_Standard lst)
        {
            lst.CheckboxLabeled("Structure is stockpile:", ref this.isStorage, "If this is on, random resources will be generated inside this structure when it's generated");
            lst.Gap();
        }

        private void DrawSpawnConduitChanger(Listing_Standard lst)
        {
            lst.CheckboxLabeled("Spawn conduit under impassable buildings and doors when generating:", ref this.spawnConduits, "If this is on, conduit will be spawned under impassable buildings and doors of this structure when it's generated (if faction techlevel >= Industrial)");
            lst.Gap();
        }

        private void DrawExportFilth(Listing_Standard lst)
        {
            lst.CheckboxLabeled("Export filth:", ref this.exportFilth);
            lst.Gap();
        }

        private void DrawStructurePrefix(Listing_Standard lst)
        {
            lst.Label("Structure defName prefix:", tooltip: "For example: VFEM_ for Vanilla Faction Expanded Mechanoid");
            structurePrefix = lst.TextEntry(structurePrefix);
            lst.Gap();
        }

        private void DrawTagsEditing(Listing_Standard lst)
        {
            lst.Label("Structure tags:");
            tempTagToAdd = lst.TextEntry(tempTagToAdd);
            if (lst.ButtonText("Add tag"))
            {
                this.tags.Add(tempTagToAdd);
            }

            if (this.tags.Count > 0)
            {
                foreach (string tag in this.tags)
                {
                    if (lst.ButtonTextLabeled(tag, "Remove tag"))
                    {
                        this.tags.Remove(tag);
                        break;
                    }
                }
            }
            lst.Gap();
        }

        private void DrawModsEditing(Listing_Standard lst)
        {
            lst.Label("Additional mod(s) needed:");
            modIdToAdd = lst.TextEntry(modIdToAdd);
            if (lst.ButtonText("Add mod package id"))
            {
                this.mods.Add(modIdToAdd);
            }

            if (this.mods.Count > 0)
            {
                foreach (string mod in this.mods)
                {
                    if (lst.ButtonTextLabeled(mod, "Remove mod"))
                    {
                        this.mods.Remove(mod);
                        break;
                    }
                }
            }
        }
    }
}