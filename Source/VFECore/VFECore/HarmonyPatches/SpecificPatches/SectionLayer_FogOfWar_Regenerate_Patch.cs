using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(SectionLayer_FogOfWar), nameof(SectionLayer_FogOfWar.Regenerate))]
    public static class SectionLayer_FogOfWar_Regenerate_Patch
    {
        private static readonly Dictionary<Map, Color?> cache = new Dictionary<Map, Color?>();

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var info  = AccessTools.Constructor(typeof(Color32), new[] {typeof(byte), typeof(byte), typeof(byte), typeof(byte)});
            var idx   = codes.FindIndex(ins => ins.opcode == OpCodes.Newobj && ins.OperandIs(info));
            codes.InsertRange(idx + 1, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SectionLayer), "section")),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Section),      "map")),
                CodeInstruction.Call(typeof(SectionLayer_FogOfWar_Regenerate_Patch), nameof(GetFogColorFor))
            });
            return codes;
        }

        public static Color32 GetFogColorFor(Color32 oldColor, Map map)
        {
            if (!cache.TryGetValue(map, out var newColor)) newColor = map.Biome.GetModExtension<BiomeExtension>()?.fogColor;

            if (newColor is Color fogColor)
            {
                oldColor.r = (byte) (fogColor.r * oldColor.r);
                oldColor.g = (byte) (fogColor.g * oldColor.g);
                oldColor.b = (byte) (fogColor.b * oldColor.b);
            }

            return oldColor;
        }
    }
}
