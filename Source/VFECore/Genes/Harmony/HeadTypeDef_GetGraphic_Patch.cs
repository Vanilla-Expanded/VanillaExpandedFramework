using System.Collections.Generic;
using Verse;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using System.Reflection.Emit;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(HeadTypeDef), "GetGraphic")]
    public static class HeadTypeDef_GetGraphic_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (codes[i].opcode == OpCodes.Stloc_0)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(HeadTypeDef_GetGraphic_Patch), nameof(TryChangeShader)));
                    yield return new CodeInstruction(OpCodes.Stloc_0);
                }
            }
        }

        public static Shader TryChangeShader(Shader shader, Pawn pawn)
        {
            if (ModsConfig.BiotechActive && pawn?.RaceProps?.Humanlike == true && pawn?.genes != null)
            {
                List<Gene> genes = pawn.genes.GenesListForReading;
                foreach (Gene gene in genes)
                {
                    if (gene.Active)
                    {
                        var extension = gene.def.GetModExtension<GeneExtension>();
                        if (extension?.forceHeadShader != null)
                        {
                            return extension.forceHeadShader.Shader;
                        }
                    }
                }
            }
            return shader;
        }
    }
}
