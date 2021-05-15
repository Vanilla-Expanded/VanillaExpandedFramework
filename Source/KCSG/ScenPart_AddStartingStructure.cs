using RimWorld;
using RimWorld.BaseGen;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace KCSG
{
    internal class ScenPart_AddStartingStructure : ScenPart
    {
        /* Generation option */
        public string structureLabel;
        public List<StructureLayoutDef> chooseFrom = new List<StructureLayoutDef>();
        public bool nearMapCenter;
        public bool allowFoggedPosition = false;
        public bool spawnPartOfEnnemyFaction = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<string>(ref structureLabel, "structureLabel");
            Scribe_Values.Look<bool>(ref nearMapCenter, "nearMapCenter");
            Scribe_Values.Look<bool>(ref allowFoggedPosition, "allowFoggedPosition");
            Scribe_Values.Look<bool>(ref spawnPartOfEnnemyFaction, "spawnPartOfEnnemyFaction");
            Scribe_Collections.Look<StructureLayoutDef>(ref chooseFrom, "chooseFrom", LookMode.Def);
        }

        public override void DoEditInterface(Listing_ScenEdit listing)
        {
            var scenPartRect = listing.GetScenPartRect(this, RowHeight * 6);

            var labelRect = new Rect(scenPartRect.x, scenPartRect.y, scenPartRect.width, RowHeight);
            structureLabel = Widgets.TextField(labelRect, structureLabel);

            var addRect = new Rect(scenPartRect.x, scenPartRect.y + RowHeight, scenPartRect.width, RowHeight);
            if (Widgets.ButtonText(addRect, "KCSG.Add".Translate()))
            {
                var floatMenuOptions = new List<FloatMenuOption>();
                foreach (var item in DefDatabase<StructureLayoutDef>.AllDefs)
                {
                    floatMenuOptions.Add(new FloatMenuOption(item.defName, () => chooseFrom.Add(item)));
                }
                Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
            }

            var removeRect = new Rect(scenPartRect.x, scenPartRect.y + RowHeight * 2, scenPartRect.width, RowHeight);
            if (Widgets.ButtonText(removeRect, "KCSG.Remove".Translate()))
            {
                var floatMenuOptions = new List<FloatMenuOption>();
                foreach (var item in chooseFrom)
                {
                    floatMenuOptions.Add(new FloatMenuOption(item.defName, () => chooseFrom.Remove(item)));
                }
                Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
            }

            // Generation options
            var nearCenterRect = new Rect(scenPartRect.x, scenPartRect.y + RowHeight * 3, scenPartRect.width, RowHeight);
            Widgets.CheckboxLabeled(nearCenterRect, "KCSG.SpawnNearCenter".Translate(), ref nearMapCenter);

            var foogedRect = new Rect(scenPartRect.x, scenPartRect.y + RowHeight * 4, scenPartRect.width, RowHeight);
            Widgets.CheckboxLabeled(foogedRect, "KCSG.AllowFoggedPosition".Translate(), ref allowFoggedPosition);
        }

        public override string Summary(Scenario scen)
        {
            return ScenSummaryList.SummaryWithList(scen, "MapContain", "Map contains");
        }

        public override IEnumerable<string> GetSummaryListEntries(string tag)
        {
            if (tag == "MapContain")
            {
                yield return structureLabel.CapitalizeFirst();
            }
            yield break;
        }

        public override void PostMapGenerate(Map map)
        {
            if (Find.TickManager.TicksGame > 5f) return;

            StructureLayoutDef structureLayoutDef;
            if (ModLister.GetActiveModWithIdentifier("EdB.PrepareCarefully") != null)
            {
                structureLayoutDef = PrepareCarefully_Util.pcScenariosSave.First().Key;
                nearMapCenter = PrepareCarefully_Util.pcScenariosSave.First().Value;
            }
            else structureLayoutDef = chooseFrom.RandomElement();

            KCSG_Utilities.HeightWidthFromLayout(structureLayoutDef, out int h, out int w);
            CellRect cellRect = this.CreateCellRect(map, h, w);

            foreach (List<string> item in structureLayoutDef.layouts)
            {
                GenUtils.GenerateRoomFromLayout(item, cellRect, map, structureLayoutDef);
            }

            FloodFillerFog.DebugRefogMap(map);
        }

        private CellRect CreateCellRect(Map map, int height, int widht)
        {
            IntVec3 rectCenter;
            if (nearMapCenter) rectCenter = map.Center;
            else rectCenter = map.AllCells.ToList().FindAll(c => this.CanScatterAt(c, map, height, widht)).InRandomOrder().RandomElement();

            return CellRect.CenteredOn(rectCenter, widht, height);
        }

        private bool CanScatterAt(IntVec3 c, Map map, int height, int widht)
        {
            if (c.CloseToEdge(map, height)) return false;
            if (!this.allowFoggedPosition && c.Fogged(map)) return false;
            if (!c.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy)) return false;
            CellRect rect = new CellRect(c.x, c.z, widht, height).ClipInsideMap(map);
            return this.CanPlaceInRange(rect, map);
        }

        private bool CanPlaceInRange(CellRect rect, Map map)
        {
            foreach (IntVec3 c in rect.Cells)
            {
                if (c.InBounds(map))
                {
                    TerrainDef terrainDef = map.terrainGrid.TerrainAt(c);
                    if (terrainDef.HasTag("River"))
                    {
                        return false;
                    }
                    if (!GenConstruct.CanBuildOnTerrain(ThingDefOf.Wall, c, map, Rot4.North, null, ThingDefOf.Granite))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}