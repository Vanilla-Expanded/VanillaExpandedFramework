
using Verse;
using HarmonyLib;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace VanillaGenesExpanded
{
    /// <summary>
    /// Additional patches for Pawn Gene Scaling
    /// I didnt notice a compat issue with HAR here. I think because i'm reversing the vector from mesh
    /// but it might have just been less noticeable here.
    /// First patch is for tattoos
    /// Second is so the head offset matches scaling by modifying the lifestage factor again. Vanilla does it for baby head offset
    /// </summary>

    [HarmonyPatch(typeof(PawnRenderer))]
    [HarmonyPatch("GetBodyOverlayMeshSet")]
    public static class PawnRenderer_GetBodyOverlayMeshSet
    {
        public static void Postfix(ref GraphicMeshSet __result, Pawn ___pawn)
        {
            if (!___pawn.RaceProps.Humanlike || !ModsConfig.BiotechActive)
            {
                return;
            }
            var genes = ___pawn.genes;
            var vector3 = __result.MeshAt(Rot4.North).vertices[2] * 2;
            //x and Z because trying to reverse NewPlaneMesh
            float factorX = vector3.x;
            float factorY = vector3.z;

            if (genes == null) { return; }
            foreach (var gene in genes.GenesListForReading)
            {
                var ext = gene.def.GetModExtension<GeneExtension>();
                if (ext != null && ext.bodyScaleFactor != 1f)
                {
                    factorX *= ext.bodyScaleFactor;
                    factorY *= ext.bodyScaleFactor;
                }
            }
            __result = MeshPool.GetMeshSetForWidth(factorX, factorY);


        }
    }
    //Offset patch
    [HarmonyPatch(typeof(PawnRenderer), "BaseHeadOffsetAt")]
    public static class PawnRenderer_BaseHeadOffsetAt_Patch
    {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var bodySizeFactor = AccessTools.Field("LifeStageDef:bodySizeFactor");
            var pawn = AccessTools.Field("PawnRenderer:pawn");
            var bodyScaleFactor = AccessTools.Method("PawnRenderer_BaseHeadOffsetAt_Patch:LifeStageFactorUpdated");
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].LoadsField(bodySizeFactor))
                {
                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, pawn);
                    yield return new CodeInstruction(OpCodes.Call, bodyScaleFactor);
                }
                else
                {
                    yield return codes[i];
                }
            }
        }
        public static float LifeStageFactorUpdated(float factor, Pawn pawn)
        {
            var genes = pawn.genes;
            if (ModLister.BiotechInstalled && genes != null)
            {
                foreach (var gene in genes.GenesListForReading)
                {
                    var ext = gene.def.GetModExtension<GeneExtension>();
                    if (ext != null && ext.bodyScaleFactor != 1f)
                    {
                        factor *= ext.bodyScaleFactor;
                    }
                }
            }
            return factor;
        }
    }
}
