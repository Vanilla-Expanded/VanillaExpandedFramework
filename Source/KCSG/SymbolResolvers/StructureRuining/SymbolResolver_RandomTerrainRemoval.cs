using RimWorld.BaseGen;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_RandomTerrainRemoval : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;

            foreach (IntVec3 pos in rp.rect)
            {
                if (Rand.Bool && map.terrainGrid.UnderTerrainAt(pos) is TerrainDef terrain && terrain != null)
                {
                    map.terrainGrid.SetTerrain(pos, terrain);
                }
            }
        }
    }
}