using HarmonyLib;
using RimWorld;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(GenStep_Terrain), "TerrainFrom")]
    public static class GenStep_Terrain_TerrainFrom_Patch
    {
        public static void Postfix(Map map, ref TerrainDef __result)
        {
            var extension = map.Biome.GetModExtension<BiomeExtension>();
            if (extension?.terrainsToSwap != null)
            {
                foreach (var terrainData in extension.terrainsToSwap)
                {
                    if (terrainData.from == __result)
                    {
                        __result = terrainData.to;
                    }
                }
            }
        }
    }
}
