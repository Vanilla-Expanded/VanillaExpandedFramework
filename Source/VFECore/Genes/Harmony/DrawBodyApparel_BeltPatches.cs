using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using VFECore;

namespace VanillaGenesExpanded
{    
    [HarmonyPatch(typeof(PawnRenderer), "DrawBodyApparel")]
    public static class DrawBodyApparel_BeltPatches
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var pawnField = AccessTools.Field(typeof(PawnRenderer),"pawn");
            var geneScale = AccessTools.Method(typeof(DrawBodyApparel_BeltPatches),"GeneScale");
            var beltOffset = AccessTools.Method(typeof(WornGraphicData),"BeltOffsetAt");
            //var beltScale = AccessTools.Method("WornGraphicData:BeltScaleAt"); Do not need to adjust this as it is already scaled via bodymesh
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(beltOffset))
                {
                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
                    yield return new CodeInstruction(OpCodes.Call, geneScale);                    
                }
                else
                {
                    yield return codes[i];
                }                
            }
        }
        public static Vector2 GeneScale(Vector2 scale, Pawn pawn)
        {
            var genes = pawn.genes;
            if (ModsConfig.BiotechActive && genes != null)
            {
                foreach (var gene in genes.GenesListForReading)
                {
                    if (gene.Active)
                    {
                        var ext = gene.def.GetModExtension<GeneExtension>();
                        if (ext != null)
                        {
                            scale *= ext.bodyScaleFactor;
                        }
                    }
                }
                if (PawnDataCache.GetPawnDataCache(pawn) is CachedPawnData data)
                {
                    scale *= data.bodyRenderSize;
                }
            }
            return scale;
        }
    }
}
