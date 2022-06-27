using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace KCSG
{
    public class GenStep_TerrainNoPatches : GenStep_Terrain
    {
        public override int SeedPart => 262606459;

        public override void Generate(Map map, GenStepParams parms)
        {
            BeachMaker.Init(map);

            MethodInfo genRiver = AccessTools.Method(typeof(GenStep_Terrain), "GenerateRiver", new Type[] { typeof(Map) });
            RiverMaker river = (RiverMaker)genRiver.Invoke(this, new object[] { map });

            List<IntVec3> nearCells = new List<IntVec3>();
            MapGenFloatGrid elevation = MapGenerator.Elevation;
            MapGenFloatGrid fertility = MapGenerator.Fertility;
            MapGenFloatGrid caves = MapGenerator.Caves;

            foreach (IntVec3 allCell in map.AllCells)
            {
                Building edifice = allCell.GetEdifice(map);
                TerrainDef newTerr = edifice != null && edifice.def.Fillage == FillCategory.Full || (double)caves[allCell] > 0.0 ? TerrainFrom(allCell, map, elevation[allCell], fertility[allCell], river, true) : TerrainFrom(allCell, map, elevation[allCell], fertility[allCell], river, false);
                if (newTerr.IsRiver && edifice != null)
                {
                    nearCells.Add(edifice.Position);
                    edifice.Destroy(DestroyMode.Vanish);
                }
                map.terrainGrid.SetTerrain(allCell, newTerr);
            }
            river?.ValidatePassage(map);

            MethodInfo remIslands = AccessTools.Method(typeof(GenStep_Terrain), "RemoveIslands", new Type[] { typeof(Map) });
            remIslands.Invoke(this, new object[] { map });

            RoofCollapseCellsFinder.RemoveBulkCollapsingRoofs(nearCells, map);
            BeachMaker.Cleanup();
        }

        private TerrainDef TerrainFrom(IntVec3 c, Map map, float elevation, float fertility, RiverMaker river, bool preferSolid)
        {
            TerrainDef terrainDef1 = null;
            if (river != null)
                terrainDef1 = river.TerrainAt(c, true);

            if (terrainDef1 == null & preferSolid)
                return GenStep_RocksFromGrid.RockDefAt(c).building.naturalTerrain;

            TerrainDef terrainDef2 = BeachMaker.BeachTerrainAt(c, map.Biome);
            if (terrainDef2 == TerrainDefOf.WaterOceanDeep)
                return terrainDef2;

            if (terrainDef1 != null && terrainDef1.IsRiver)
                return terrainDef1;

            if (terrainDef2 != null)
                return terrainDef2;

            if (terrainDef1 != null)
                return terrainDef1;

            if (elevation > 0.550000011920929 && elevation < 0.610000014305115)
                return TerrainDefOf.Gravel;

            if (elevation >= 0.610000014305115)
                return GenStep_RocksFromGrid.RockDefAt(c).building.naturalTerrain;

            TerrainDef terrainDef4 = TerrainThreshold.TerrainAtValue(map.Biome.terrainsByFertility, fertility);
            if (terrainDef4 != null)
                return terrainDef4;

            return TerrainDefOf.Sand;
        }
    }
}