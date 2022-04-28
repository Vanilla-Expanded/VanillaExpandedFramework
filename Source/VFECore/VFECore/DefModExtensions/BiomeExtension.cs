using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VFECore
{
    public class SwapTerrainDef
    {
        public TerrainDef from;
        public TerrainDef to;
    }
    public class BiomeExtension : DefModExtension
    {
        public List<SwapTerrainDef> terrainsToSwap;
        public List<GenStepDef> skipGenSteps;
    }
}
