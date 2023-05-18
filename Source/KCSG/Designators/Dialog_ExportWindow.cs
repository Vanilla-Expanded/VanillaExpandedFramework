using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace KCSG
{
    [StaticConstructorOnStartup]
    public class Dialog_ExportWindow : Window
    {
        public static readonly Texture2D helpIcon = ContentFinder<Texture2D>.Get("UI/CSG/help");

        // Save stuff in between exports
        public static HashSet<string> exportedSymbolsName = new HashSet<string>();
        public static List<SymbolDef> exportedSymbolsDef = new List<SymbolDef>();
        public static Dictionary<string, StructureLayoutDef> exportedLayouts = new Dictionary<string, StructureLayoutDef>();

        public static string defName = "";

        public static Dictionary<IntVec3, List<Thing>> pairsCellThingList = new Dictionary<IntVec3, List<Thing>>();
        public static List<IntVec3> cells = new List<IntVec3>();
        public static HashSet<string> tags = new HashSet<string>();

        public static bool exportNatural = false;
        public static bool exportFilth = false;
        public static bool exportPlant = false;
        public static bool needRoofClearance = false;
        public static bool spawnConduits = false;
        public static bool forceGenerateRoof = false;
        public static bool isStorage = false;
        public static bool randomizeWallStuffAtGen = false;

        private readonly Area area;
        private readonly Map map;

        private static readonly int bottomBH = 30;
        private static readonly int tagBH = 30;
        private string tempTagToAdd = "";

        public Dialog_ExportWindow(Map map, List<IntVec3> rCells, Area area)
        {
            this.map = map;
            this.area = area;
            cells = rCells;
            // Window settings
            forcePause = true;
            doCloseX = false;
            doCloseButton = false;
            closeOnClickedOutside = false;
            absorbInputAroundWindow = true;

            cells.Sort((x, y) => x.z.CompareTo(y.z));

            StartupActions.CreateSymbols();
            pairsCellThingList = ExportUtils.FillCellThingsList(map);
            exportedSymbolsDef = ExportUtils.CreateSymbolIfNeeded(area);
        }

        public override Vector2 InitialSize => new Vector2(650f, 520f);

        public override void DoWindowContents(Rect inRect)
        {
            var font = Text.Font;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;

            Widgets.Label(new Rect(0, 0, 500, 30), "Structure export menu");
            if (Widgets.ButtonImage(new Rect(530, 0, 30, 30), helpIcon))
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

            lst.CheckboxLabeled("Stockpile:", ref isStorage, "Generate random items inside when used with SettlementLayoutDef");
            lst.Gap(5);

            lst.CheckboxLabeled("Force generate roofs:", ref forceGenerateRoof, "Alway generate exported roof");
            lst.Gap(5);

            lst.CheckboxLabeled("Need roof clearance:", ref needRoofClearance, "Need to be placed in a rect free of roofs");
            lst.Gap(5);

            lst.CheckboxLabeled("Randomize wall stuff:", ref randomizeWallStuffAtGen, "Randomize wall stuff at generation");
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

                tempTagToAdd = "";
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
                    var sld = ExportUtils.CreateStructureDef(map, area);

                    if (exportedLayouts.ContainsKey(defName))
                        exportedLayouts[defName] = sld;
                    else
                        exportedLayouts.Add(defName, sld);

                    GUIUtility.systemCopyBuffer = sld.ToXMLString();
                    Messages.Message("Copied to clipboard.", MessageTypeDefOf.TaskCompletion);
                }
                else
                {
                    Messages.Message("Cannot use empty defName.", MessageTypeDefOf.NegativeEvent);
                }
            }

            if (Widgets.ButtonText(new Rect(200, inRect.height - bottomBH, 190, bottomBH), "Copy symbols"))
            {
                var count = exportedSymbolsDef.Count;
                if (count > 0)
                {
                    var output = "";
                    for (int i = 0; i < count; i++)
                    {
                        output += exportedSymbolsDef[i].ToXMLString() + "\n\n";
                    }

                    GUIUtility.systemCopyBuffer = output.TrimEndNewlines();
                    Messages.Message($"Copied {count} symbols to clipboard.", MessageTypeDefOf.TaskCompletion);
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
    }
}