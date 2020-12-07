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
#pragma warning disable 0649
        public string structureLabel;
        public bool nearMapCenter;
#pragma warning restore 0649
        public List<StructureLayoutDef> chooseFrom = new List<StructureLayoutDef>();

        public bool allowFoggedPosition = false;
        public bool spawnPartOfEnnemyFaction = false;

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
            if (ModLister.GetActiveModWithIdentifier("EdB.PrepareCarefully").Active)
            {
                structureLayoutDef = PrepareCarefully_Util.pcScenariosSave.First().Key;
                nearMapCenter = PrepareCarefully_Util.pcScenariosSave.First().Value;
            }
            else structureLayoutDef = chooseFrom.RandomElement();


            KCSG_Utilities.HeightWidthFromLayout(structureLayoutDef, out int h, out int w);
            CellRect cellRect = this.CreateCellRect(map, h, w);

            foreach (List<string> item in structureLayoutDef.layouts)
            {
                KCSG_Utilities.GenerateRoomFromLayout(item, cellRect, map, structureLayoutDef);
            }

            FloodFillerFog.DebugRefogMap(map);
            PrepareCarefully_Util.pcScenariosSave.Remove(structureLayoutDef);
        }

        private CellRect CreateCellRect(Map map, int height, int widht)
        {
            IntVec3 rectCenter;
            if (nearMapCenter) rectCenter = map.Center;
            else rectCenter = map.AllCells.ToList().FindAll(c => this.CanScatterAt(c, map, height, widht)).InRandomOrder().RandomElement();

            Log.Message(nearMapCenter.ToString());
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
                    if (terrainDef.HasTag("River") || terrainDef.HasTag("Road"))
                    {
                        return false;
                    }
                    if (!GenConstruct.CanBuildOnTerrain(ThingDefOf.Wall, c, map, Rot4.North, null, null))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}