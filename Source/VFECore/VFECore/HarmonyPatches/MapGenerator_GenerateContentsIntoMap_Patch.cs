using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace VFECore
{
    [HarmonyPatch(typeof(MapGenerator), "GenerateContentsIntoMap")]
    public static class MapGenerator_GenerateContentsIntoMap_Patch
    {
        public static void Prefix(ref IEnumerable<GenStepWithParams> genStepDefs, Map map, int seed)
        {
            var extension = map.Biome.GetModExtension<BiomeExtension>();
            if (extension?.skipGenSteps != null)
            {
                genStepDefs = genStepDefs.Where(x => !extension.skipGenSteps.Contains(x.def)).ToList();
            }
        }
    }
}
