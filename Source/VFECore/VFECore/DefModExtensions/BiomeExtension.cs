using System.Collections.Generic;
using UnityEngine;
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
        public List<GenStepDef>     skipGenSteps;
        public Color                fogColor = Color.white;
    }
}
