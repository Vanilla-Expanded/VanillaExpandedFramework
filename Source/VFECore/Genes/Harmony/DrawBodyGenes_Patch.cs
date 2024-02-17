using System.Collections.Generic;
using System.Linq;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using RimWorld;
using VFECore;

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
            MethodInfo geneDrawOffsetAtInfo = AccessTools.Method(typeof(GeneGraphicData), nameof(GeneGraphicData.DrawOffsetAt));


            List<CodeInstruction> codes = codeInstructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                CodeInstruction code = codes[i];
                yield return code;
                if (code.opcode == OpCodes.Stloc_0)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DrawBodyGenes_Patch), nameof(SetBodyScale)));
                    yield return new CodeInstruction(OpCodes.Stloc_0);
                }

                if (code.Calls(geneDrawOffsetAtInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return CodeInstruction.LoadField(typeof(GeneGraphicRecord), nameof(GeneGraphicRecord.sourceGene));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld,   pawnField);
                    yield return new CodeInstruction(codes[i-1]);
                    yield return CodeInstruction.Call(typeof(DrawBodyGenes_Patch), nameof(GeneOffset));
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
                        if (extension.bodyScaleFactorsPerLifestages != null 
                            && extension.bodyScaleFactorsPerLifestages.TryGetValue(pawn.ageTracker.CurLifeStage, out var lifestageScale))
                        {
                            scale *= lifestageScale;
                        }
                    }
                }
            }
            if (PawnDataCache.GetPawnDataCache(pawn) is CachedPawnData data)
            {
                scale *= data.bodyRenderSize;
            }
            return scale;
        }

        public static Vector3 GeneOffset(Vector3 offset, Gene gene, Pawn pawn, Rot4 rot)
        {
            GeneExtension extension = gene.def.GetModExtension<GeneExtension>();
            return extension != null ? 
                       offset + extension.offsets.GetOffset(pawn, rot) : 
                       offset;
        }
    }
}
