using System.Collections.Generic;
using System.Xml.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace KCSG
{
    public class Dialog_ExportWindow : Window
    {
        // Save stuff in between exports
        public static HashSet<string> Tags = new HashSet<string>();
        public static List<string> AlreadyExported = new List<string>();

        private readonly List<IntVec3> cells = new List<IntVec3>();
        private readonly HashSet<string> tags = new HashSet<string>();
        private readonly Area area;
        private readonly Map map;

        private readonly Dictionary<IntVec3, List<Thing>> pairsCellThingList = new Dictionary<IntVec3, List<Thing>>();
        private string tempTagToAdd = "";
        private string defName = "";
        private bool exportNatural = false;
        private bool roofClearance = false;
        private bool spawnConduits = true;
        private bool exportFilth = false;
        private bool exportPlant = true;
        private bool forceRoof = false;
        private bool storage = false;

        private readonly int bottomBH = 30;
        private readonly int tagBH = 30;

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

            StartupActions.CreateSymbols();
            if (Tags.Count > 0)
                tags = Tags;

            pairsCellThingList = ExportUtils.FillCellThingsList(cells, map);
        }

        public override Vector2 InitialSize => new Vector2(610f, 520f);

        public override void DoWindowContents(Rect inRect)
        {
            var font = Text.Font;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;

            Widgets.Label(new Rect(0, 0, 500, 30), "Structure export menu");
            if (Widgets.ButtonImage(new Rect(530, 0, 30, 30), Textures.helpIcon))
            {
                System.Diagnostics.Process.Start("https://github.com/AndroidQuazar/VanillaExpandedFramework/wiki/Exporting-buildings");
            }

            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = font;

            Rect rect = new Rect(inRect.x, inRect.y + 40f, inRect.width, inRect.height - 90f);

            var lst = new Listing_Standard();
            lst.Begin(rect);

            lst.Label("Structure defName:");
            defName = lst.TextEntry(defName);
            lst.Gap();

            lst.CheckboxLabeled("Auto-conduit:", ref spawnConduits, "Spawn conduits under impassables and doors automatically");
            lst.GapLine();

            lst.CheckboxLabeled("Export filth:", ref exportFilth);
            lst.Gap(5);

            lst.CheckboxLabeled("Export plants:", ref exportPlant);
            lst.Gap(5);

            lst.CheckboxLabeled("Export natural terrain:", ref exportNatural);
            lst.GapLine();

            lst.CheckboxLabeled("Stockpile:", ref storage, "Generate random items inside when used with SettlementLayoutDef");
            lst.Gap(5);

            lst.CheckboxLabeled("Force generate roofs:", ref forceRoof, "Alway generate exported roof");
            lst.Gap(5);

            lst.CheckboxLabeled("Need roof clearance:", ref roofClearance, "Need to be placed in a rect free of roofs");
            lst.GapLine();

            lst.Label("Structure tags:", tooltip: "Tags are used with SettlementLayoutDef");
            tempTagToAdd = lst.TextEntry(tempTagToAdd);

            // Tag
            var tCount = tags.Count;
            var height = lst.CurHeight + 5;

            Widgets.Label(new Rect(0, height, 190, tagBH), $"{tCount} tag(s) used");

            if (Widgets.ButtonText(new Rect(200, height, 190, tagBH), "Add tag"))
            {
                if (tempTagToAdd.Length > 0)
                    tags.Add(tempTagToAdd);
                else
                    Messages.Message("Cannot add empty tag.", MessageTypeDefOf.NegativeEvent);
            }

            if (Widgets.ButtonText(new Rect(400, height, 190, tagBH), "Remove tag..."))
            {
                var floatMenuOptions = new List<FloatMenuOption>();
                if (tCount > 0)
                {
                    foreach (var tag in tags)
                    {
                        floatMenuOptions.Add(new FloatMenuOption(tag, () => tags.Remove(tag)));
                    }
                }
                else
                {
                    floatMenuOptions.Add(new FloatMenuOption("Nothing to remove", null));
                }
                Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
            }
            lst.End();
            DrawFooter(inRect);
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DrawFooter(Rect inRect)
        {
            if (Widgets.ButtonText(new Rect(0, inRect.height - bottomBH, 190, bottomBH), "Copy structure"))
            {
                if (defName.Length > 0)
                {
                    GUIUtility.systemCopyBuffer = CreateLayout().ToString();
                    Messages.Message("Copied to clipboard.", MessageTypeDefOf.TaskCompletion);
                    Tags = tags;
                }
                else
                {
                    Messages.Message("Cannot use empty defName.", MessageTypeDefOf.NegativeEvent);
                }
            }

            if (Widgets.ButtonText(new Rect(200, inRect.height - bottomBH, 190, bottomBH), "Copy symbols"))
            {
                var allSymbols = ExportUtils.CreateSymbolIfNeeded(cells, map, pairsCellThingList, area);
                if (allSymbols.Count is int count && count > 0)
                {
                    var output = "";
                    for (int i = 0; i < count; i++)
                    {
                        var symb = allSymbols[i];
                        var toStr = symb.ToString();
                        if (!AlreadyExported.Contains(toStr))
                        {
                            output += toStr + "\n\n";
                            AlreadyExported.Add(toStr);
                        }
                    }

                    GUIUtility.systemCopyBuffer = output;
                    Messages.Message("Copied to clipboard.", MessageTypeDefOf.TaskCompletion);
                }
                else
                {
                    Messages.Message("No new symbols needed.", MessageTypeDefOf.TaskCompletion);
                }
            }

            if (Widgets.ButtonText(new Rect(400, inRect.height - bottomBH, 190, bottomBH), "Close"))
            {
                Close();
            }
        }

        private XElement CreateLayout()
        {
            XElement structureL = ExportUtils.CreateStructureDef(cells, map, pairsCellThingList, area, exportFilth, exportNatural, exportPlant);

            structureL.AddFirst(new XElement("defName", defName));

            if (storage)
                structureL.Add(new XElement("isStorage", true));

            if (!spawnConduits)
                structureL.Add(new XElement("spawnConduits", false));

            if (forceRoof)
                structureL.Add(new XElement("forceGenerateRoof", true));

            if (roofClearance)
                structureL.Add(new XElement("needRoofClearance", true));

            if (tags.Count > 0)
            {
                XElement temp = new XElement("tags");
                foreach (var item in tags)
                {
                    temp.Add(new XElement("li", item));
                }
                structureL.Add(temp);
            }

            return structureL;
        }
    }
}