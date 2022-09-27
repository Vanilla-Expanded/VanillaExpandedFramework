using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace VanillaGenesExpanded
{

    [HarmonyPatch(typeof(GeneUIUtility))]
    [HarmonyPatch("DrawGeneBasics")]
    public static class VanillaGenesExpanded_GeneUIUtility_DrawGeneBasics_Patch
    {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            var loadsField = AccessTools.Field(typeof(GeneUIUtility), "GeneBackground_Endogene");

            var loadsFieldTwo = AccessTools.Field(typeof(GeneUIUtility), "GeneBackground_Xenogene");

            var codes = instructions.ToList();

            for (var i = 0; i < codes.Count; i++)
            {

                var code = codes[i];
                if (codes[i].opcode == OpCodes.Ldsfld && codes[i].LoadsField(loadsField))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(codes[i]);
                    yield return new CodeInstruction(OpCodes.Call, typeof(VanillaGenesExpanded_GeneUIUtility_DrawGeneBasics_Patch).GetMethod("ChooseEndogeneBackground"));

                }
                else if (codes[i].opcode == OpCodes.Ldsfld && codes[i].LoadsField(loadsFieldTwo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(codes[i]);
                    yield return new CodeInstruction(OpCodes.Call, typeof(VanillaGenesExpanded_GeneUIUtility_DrawGeneBasics_Patch).GetMethod("ChooseXenogeneBackground"));
                }

                else
                {
                    yield return code;
                }
            }
        }

        public static CachedTexture ChooseEndogeneBackground(GeneDef gene)
        {
            if (gene.GetModExtension<GeneExtension>() != null)
            {
                return new CachedTexture(gene.GetModExtension<GeneExtension>().backgroundPathEndogenes);
            }
            else { return GraphicsCache.GeneBackground_Endogene; }
        }

        public static CachedTexture ChooseXenogeneBackground(GeneDef gene)
        {
            if (gene.GetModExtension<GeneExtension>() != null)
            {
                return new CachedTexture(gene.GetModExtension<GeneExtension>().backgroundPathXenogenes);
            }
            else { return GraphicsCache.GeneBackground_Xenogene; }
        }
    }
}