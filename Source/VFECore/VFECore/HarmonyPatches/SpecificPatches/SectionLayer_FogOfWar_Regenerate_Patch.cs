using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace VFECore
{
    [HarmonyPatch]
    public static class SectionLayer_FogOfWar_Regenerate_Patch
    {
        private static readonly Dictionary<Map, Material> cache = new();

        [HarmonyPatch(typeof(SectionLayer_FogOfWar), nameof(SectionLayer_FogOfWar.Regenerate))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var info = AccessTools.Field(typeof(MatBases), nameof(MatBases.FogOfWar));
            foreach (var instruction in instructions)
            {
                if (instruction.LoadsField(info))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SectionLayer), "section"));
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Section),      "map"));
                    yield return CodeInstruction.Call(typeof(SectionLayer_FogOfWar_Regenerate_Patch), nameof(GetFogMat));
                } else yield return instruction;
            }
        }

        [HarmonyPatch(typeof(MapDeiniter), nameof(MapDeiniter.Deinit))]
        [HarmonyPrefix]
        public static void ClearCache(Map map)
        {
            if (map != null && cache.TryGetValue(map, out var mat) && mat != null)
            {
                Object.Destroy(mat);
                cache.Remove(map);
            }
        }

        public static Material GetFogMat(Map map)
        {
            if (cache.TryGetValue(map, out var mat)) return mat ?? MatBases.FogOfWar;
            var color = map.Biome.GetModExtension<BiomeExtension>()?.fogColor;
            if (color.HasValue)
            {
                mat       = new Material(MatBases.FogOfWar);
                mat.color = color.Value;
                cache.Add(map, mat);
                return mat;
            } else
            {
                cache.Add(map, null);
                return MatBases.FogOfWar;
            }
        }
    }
}
