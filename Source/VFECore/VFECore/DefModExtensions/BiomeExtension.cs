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
        public Color?               fogColor = null;
        public List<ThingDef>       forceRockTypes;
        public List<ThingDef>       uniqueRockTypes;
        public List<ThingDef>       disallowRockTypes;
        public bool                 onlyAllowForcedRockTypes = true;
        public bool                 forceUniqueRockTypes     = true;
    }
}
