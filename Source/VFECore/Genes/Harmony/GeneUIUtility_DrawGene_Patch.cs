using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(GeneUIUtility), "DrawGene")]
    public static class GeneUIUtility_DrawGene_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            bool patched = false;
            foreach (var code in codeInstructions)
            {
                yield return code;
                if (code.opcode == OpCodes.Stloc_0 && !patched)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GeneUIUtility_DrawGene_Patch), "ModifyTooltip"));
                    yield return new CodeInstruction(OpCodes.Stloc_0);
                    patched = true;
                }
            }
        }

        public static string ModifyTooltip(string tooltip, Gene gene)
        {
            if (gene is GeneGendered geneGendered && geneGendered.pawn.gender != geneGendered.Extension.forGenderOnly)
            {
                tooltip += "\n\n";
                tooltip += "VGE_ForGenderOnly".Translate(geneGendered.Extension.forGenderOnly.GetLabel().CapitalizeFirst()).Colorize(ColorLibrary.RedReadable);

            }
            return tooltip;
        }
    }
}