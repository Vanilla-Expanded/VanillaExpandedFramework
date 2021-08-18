using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace KCSG
{
    internal class ScenPart_AddStartingStructure : ScenPart
    {
        /* Generation option */
        public bool allowFoggedPosition = false;
        public List<StructureLayoutDef> chooseFrom = new List<StructureLayoutDef>();
        public bool nearMapCenter;
        public bool spawnPartOfEnnemyFaction = false;
        public string structureLabel;
        /* Clear filth, buildings, chunks, remove non-natural terrain */
        public bool preGenClear = true;
        /* Clear everything */
        public bool fullClear = false;

        public override void DoEditInterface(Listing_ScenEdit listing)
        {
            var scenPartRect = listing.GetScenPartRect(this, RowHeight * 6);

            var labelRect = new Rect(scenPartRect.x, scenPartRect.y, scenPartRect.width, RowHeight);
            structureLabel = Widgets.TextField(labelRect, structureLabel);

            var addRect = new Rect(scenPartRect.x, scenPartRect.y + RowHeight, scenPartRect.width, RowHeight);
            if (Widgets.ButtonText(addRect, "KCSG.Add".Translate()))
            {
                var floatMenuOptions = new List<FloatMenuOption>();
                if (DefDatabase<StructureLayoutDef>.AllDefs.Count() > 0)
                {
                    foreach (var item in DefDatabase<StructureLayoutDef>.AllDefs)
                    {
                        floatMenuOptions.Add(new FloatMenuOption(item.defName, () => chooseFrom.Add(item)));
                    }
                }
                else
                {
                    floatMenuOptions.Add(new FloatMenuOption("No structure available", null));
                }
                Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
            }

            var removeRect = new Rect(scenPartRect.x, scenPartRect.y + RowHeight * 2, scenPartRect.width, RowHeight);
            if (Widgets.ButtonText(removeRect, "KCSG.Remove".Translate()))
            {
                var floatMenuOptions = new List<FloatMenuOption>();
                if (chooseFrom.Count > 0)
                {
                    foreach (var item in chooseFrom)
                    {
                        floatMenuOptions.Add(new FloatMenuOption(item.defName, () => chooseFrom.Remove(item)));
                    }
                }
                else
                {
                    floatMenuOptions.Add(new FloatMenuOption("Nothing to remove", null));
                }
                Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
            }

            // Generation options
            var nearCenterRect = new Rect(scenPartRect.x, scenPartRect.y + RowHeight * 3, scenPartRect.width, RowHeight);
            Widgets.CheckboxLabeled(nearCenterRect, "KCSG.SpawnNearCenter".Translate(), ref nearMapCenter);

            var foogedRect = new Rect(scenPartRect.x, scenPartRect.y + RowHeight * 4, scenPartRect.width, RowHeight);
            Widgets.CheckboxLabeled(foogedRect, "KCSG.AllowFoggedPosition".Translate(), ref allowFoggedPosition);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<string>(ref structureLabel, "structureLabel");
            Scribe_Values.Look<bool>(ref nearMapCenter, "nearMapCenter");
            Scribe_Values.Look<bool>(ref allowFoggedPosition, "allowFoggedPosition");
            Scribe_Values.Look<bool>(ref spawnPartOfEnnemyFaction, "spawnPartOfEnnemyFaction");
            Scribe_Collections.Look<StructureLayoutDef>(ref chooseFrom, "chooseFrom", LookMode.Def);
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
            if (Find.TickManager.TicksGame > 5f || chooseFrom.Count <= 0 || PrepareCarefully_Util.pcScenariosSave.Count <= 0) return;

            StructureLayoutDef structureLayoutDef;
            if (ModLister.GetActiveModWithIdentifier("EdB.PrepareCarefully") != null)
            {
                structureLayoutDef = PrepareCarefully_Util.pcScenariosSave.First().Key;
                nearMapCenter = PrepareCarefully_Util.pcScenariosSave.First().Value;
            }
            else structureLayoutDef = chooseFrom.RandomElement();

            RectUtils.HeightWidthFromLayout(structureLayoutDef, out int h, out int w);
            CellRect cellRect = this.CreateCellRect(map, h, w);

            if (preGenClear)
                GenUtils.PreClean(map, cellRect, fullClear);

            foreach (List<string> item in structureLayoutDef.layouts)
            {
                GenUtils.GenerateRoomFromLayout(item, cellRect, map, structureLayoutDef);
            }

            if (map.mapPawns.FreeColonistsSpawned.Count > 0)
            {
                FloodFillerFog.DebugRefogMap(map);
            }
        }

        public override string Summary(Scenario scen)
        {
            return ScenSummaryList.SummaryWithList(scen, "MapContain", "KCSG.Mapcontains".Translate());
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

        private bool CanScatterAt(IntVec3 c, Map map, int height, int widht)
        {
            if (c.CloseToEdge(map, height)) return false;
            if (!this.allowFoggedPosition && c.Fogged(map)) return false;
            if (!c.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy)) return false;
            CellRect rect = new CellRect(c.x, c.z, widht, height).ClipInsideMap(map);
            return this.CanPlaceInRange(rect, map);
        }

        private CellRect CreateCellRect(Map map, int height, int widht)
        {
            IntVec3 rectCenter;
            if (nearMapCenter) rectCenter = map.Center;
            else rectCenter = map.AllCells.ToList().FindAll(c => this.CanScatterAt(c, map, height, widht)).InRandomOrder().RandomElement();

            return CellRect.CenteredOn(rectCenter, widht, height);
        }
    }
}