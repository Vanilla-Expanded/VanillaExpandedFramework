using System.Collections.Generic;
using System.Xml.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace KCSG
{
    internal class Dialog_ExportWindow : Window
    {
        private readonly Area area;
        private readonly List<IntVec3> cells = new List<IntVec3>();
        private readonly Map map;
        private readonly List<string> tags = new List<string>();
        private readonly List<string> mods = new List<string>();

        private Dictionary<IntVec3, List<Thing>> pairsCellThingList = new Dictionary<IntVec3, List<Thing>>();
        private Color boxColor = new Color(0.13f, 0.14f, 0.16f);
        private string defname = "Placeholder";
        private bool isStorage = false;
        private bool spawnConduits = true;
        private bool exportFilth = false;
        private bool exportNatTer = false;
        private bool forceGenerateRoof = false;

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
            forcePause = true;
            doCloseX = false;
            doCloseButton = false;
            closeOnClickedOutside = false;
            absorbInputAroundWindow = true;

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
            DrawHeader();
            Text.Font = GameFont.Small;
            Rect scrollRect = new Rect(inRect.x, inRect.y + 80f, inRect.width, inRect.height - 150f);
            Rect viewRect = new Rect(inRect.x, inRect.y + 80f, inRect.width - 20f, scrollRect.height);
            Listing_Standard lst = new Listing_Standard();
            if (viewRect.height < viewRect.height + mods.Count * 12) viewRect.height += mods.Count * 12;
            if (viewRect.height < viewRect.height + tags.Count * 12) viewRect.height += tags.Count * 12;

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.BeginScrollView(scrollRect, ref scrollPosition, viewRect);
            lst.Begin(viewRect);

            DrawDefNameChanger(lst);
            DrawStorageChanger(lst);
            DrawSpawnConduitChanger(lst);
            DrawExportFilth(lst);
            DrawExportNaturalTerrain(lst);
            DrawForceGenerateRoof(lst);
            DrawStructurePrefix(lst);
            DrawTagsEditing(lst);
            DrawModsEditing(lst);

            lst.End();
            Widgets.EndScrollView();
            DrawFooter(inRect);
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private XElement CreateLayout()
        {
            XElement structureL = ExportUtils.CreateStructureDef(cells, map, pairsCellThingList, area, exportFilth, exportNatTer);
            // Defname change
            XElement defName = new XElement("defName", structurePrefix + defname);
            structureL.AddFirst(defName);
            // isStorage change
            if (isStorage)
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
            if (!spawnConduits)
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
            if (forceGenerateRoof)
            {
                if (structureL.Element("forceGenerateRoof") == null)
                {
                    structureL.Add(new XElement("forceGenerateRoof", true));
                }
            }
            else
            {
                structureL.Element("forceGenerateRoof")?.Remove();
            }
            // Tags changes
            if (tags.Count > 0)
            {
                XElement temp = new XElement("tags");
                foreach (var item in tags)
                {
                    temp.Add(new XElement("li", item));
                }
                structureL.Add(temp);
            }
            // Mods
            if (mods.Count > 0)
            {
                XElement temp = new XElement("modRequirements");
                foreach (var item in mods)
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
                if (structurePrefix.Length == 0 || structurePrefix == "Required")
                {
                    Messages.Message("Structure prefix required.", MessageTypeDefOf.NegativeEvent);
                }
                else
                {
                    pairsCellThingList = ExportUtils.FillCellThingsList(cells, map);
                    GUIUtility.systemCopyBuffer = CreateLayout().ToString();
                    Messages.Message("Copied to clipboard.", MessageTypeDefOf.TaskCompletion);
                }
            }
            if (Widgets.ButtonText(new Rect(350, inRect.height - bHeight, 340, bHeight), "Copy symbol(s) def(s)"))
            {
                pairsCellThingList = ExportUtils.FillCellThingsList(cells, map);
                symbols = ExportUtils.CreateSymbolIfNeeded(cells, map, pairsCellThingList, area);
                if (symbols.Count > 0)
                {
                    string toCopy = "";
                    foreach (XElement item in symbols)
                    {
                        toCopy += item.ToString() + "\n\n";
                    }
                    GUIUtility.systemCopyBuffer = toCopy;
                    Messages.Message("Copied to clipboard.", MessageTypeDefOf.TaskCompletion);
                }
                else
                {
                    Messages.Message("No new symbols needed.", MessageTypeDefOf.TaskCompletion);
                }
            }
            if (Widgets.ButtonText(new Rect(700, inRect.height - bHeight, 60, bHeight), "Close"))
            {
                Close();
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
            if (Widgets.ButtonImage(infoRect, Textures.helpIcon))
            {
                System.Diagnostics.Process.Start("https://github.com/AndroidQuazar/VanillaExpandedFramework/wiki/Exporting-your-own-structures");
            }

            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DrawStorageChanger(Listing_Standard lst)
        {
            lst.CheckboxLabeled("Structure is stockpile:", ref isStorage, "If this is on, random resources will be generated inside this structure when it's generated");
            lst.Gap();
        }

        private void DrawSpawnConduitChanger(Listing_Standard lst)
        {
            lst.CheckboxLabeled("Spawn conduit under impassable buildings and doors when generating:", ref spawnConduits, "If this is on, conduit will be spawned under impassable buildings and doors of this structure when it's generated (if faction techlevel >= Industrial)");
            lst.Gap();
        }

        private void DrawForceGenerateRoof(Listing_Standard lst)
        {
            lst.CheckboxLabeled("Force generate roofs:", ref forceGenerateRoof, "By default constructed roof will only be constructed if the cell isn't roofed at all. Thin roof will be on cell with no roof or constructed roof. Thick roof override everything. Enable to alway generate exported roof.");
            lst.Gap();
        }

        private void DrawExportFilth(Listing_Standard lst)
        {
            lst.CheckboxLabeled("Export filth:", ref exportFilth);
            lst.Gap();
        }

        private void DrawExportNaturalTerrain(Listing_Standard lst)
        {
            lst.CheckboxLabeled("Export natural terrain:", ref exportNatTer);
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
                tags.Add(tempTagToAdd);
            }

            if (tags.Count > 0)
            {
                foreach (string tag in tags)
                {
                    if (lst.ButtonTextLabeled(tag, "Remove tag"))
                    {
                        tags.Remove(tag);
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
                mods.Add(modIdToAdd);
            }

            if (mods.Count > 0)
            {
                foreach (string mod in mods)
                {
                    if (lst.ButtonTextLabeled(mod, "Remove mod"))
                    {
                        mods.Remove(mod);
                        break;
                    }
                }
            }
        }
    }
}