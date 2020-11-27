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

        public bool unfogBuilding = true;
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

            StructureLayoutDef structureLayoutDef = chooseFrom.RandomElement();
            if (VFECore.VFEGlobal.settings.enableLog) Log.Message("ScenPart_AddStartingStructure - Structure choosen: " + structureLayoutDef.defName);

            KCSG_Utilities.HeightWidthFromLayout(structureLayoutDef, out int h, out int w);
            CellRect cellRect = this.CreateCellRect(map, h, w);

            int count = 0;
            foreach (List<string> item in structureLayoutDef.layouts)
            {
                KCSG_Utilities.GenerateRoomFromLayout(item, cellRect, map, structureLayoutDef);
                if (VFECore.VFEGlobal.settings.enableLog) Log.Message("ScenPart_AddStartingStructure - Layout " + count++.ToString() + " generation - PASS");
            }

            if (this.unfogBuilding) this.UnfogBuildingsInRect(map, cellRect);
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
            if (c.CloseToEdge(map, 10)) return false;
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

        private void UnfogBuildingsInRect(Map map, CellRect rect)
        {
            foreach (IntVec3 i in rect.ExpandedBy(5))
            {
                if (i.Fogged(map) && (i.GetThingList(map).Any((t) => !t.def.mineable))) map.fogGrid.Unfog(i);
            }
        }
    }
}