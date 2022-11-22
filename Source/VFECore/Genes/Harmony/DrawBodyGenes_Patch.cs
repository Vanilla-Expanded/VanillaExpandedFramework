using System.Collections.Generic;
using System.Linq;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace VanillaGenesExpanded
{
    [HarmonyPatch]
    public static class DrawBodyGenes_Patch
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(PawnRenderer), "DrawBodyGenes");
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var pawnField = AccessTools.Field(typeof(PawnRenderer), "pawn");
            var codes = codeInstructions.ToList();
            foreach (var code in codes)
            {
                yield return code;
                if (code.opcode == OpCodes.Stloc_0)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DrawBodyGenes_Patch), nameof(SetBodyScale)));
                    yield return new CodeInstruction(OpCodes.Stloc_0);
                }
            }
        }

        public static Vector2 SetBodyScale(Pawn pawn, Vector2 scale)
        {
            foreach (var g in pawn.genes.GenesListForReading.Where(x => x.Active))
            {
                if (g.Active)
                {
                    var extension = g.def.GetModExtension<GeneExtension>();
                    if (extension != null)
                    {
                        scale *= extension.bodyScaleFactor;
                    }
                }
            }
            return scale;
        }
    }
}
