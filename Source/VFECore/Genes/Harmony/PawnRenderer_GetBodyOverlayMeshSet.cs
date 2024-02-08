
using Verse;
using HarmonyLib;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;
using RimWorld;
using System;
using System.Reflection;
using UnityEngine;
using VFECore;
using Verse.Noise;

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
                if (gene.Active)
                {
                    var ext = gene.def.GetModExtension<GeneExtension>();
                    if (ext != null)
                    {
                        factorX *= ext.bodyScaleFactor.x;
                        factorY *= ext.bodyScaleFactor.y;
                    }
                }
            }
            if (ScaleCache.GetScaleCache(___pawn) is SizeData data)
            {
                factorX *= data.bodyRenderSize;
                factorY *= data.bodyRenderSize;
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
            var headOffset = AccessTools.Field(typeof(BodyTypeDef),"headOffset");
            var pawn = AccessTools.Field(typeof(PawnRenderer),"pawn");
            var bodyScaleFactor = AccessTools.Method(typeof(PawnRenderer_BaseHeadOffsetAt_Patch),"LifeStageFactorUpdated");
            var codes = instructions.ToList();
            bool skip = false;
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].LoadsField(headOffset))
                {
                    skip = true;
                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, pawn);
                    yield return new CodeInstruction(OpCodes.Call, bodyScaleFactor);
                }
                else if (skip && codes[i].opcode == OpCodes.Stloc_0)
                {
                    skip = false;
                    yield return codes[i];
                }
                else if (skip)
                {
                    //do nothing
                }
                else
                {
                    yield return codes[i];
                }
            }
        }
        public static Vector2 LifeStageFactorUpdated(Vector2 offset, Pawn pawn)
        {
            var genes = pawn.genes;
            offset *= Mathf.Sqrt(pawn.ageTracker.CurLifeStage.bodySizeFactor);
            if (ModsConfig.BiotechActive && genes != null)
            {
                foreach (var gene in genes.GenesListForReading)
                {
                    if (gene.Active)
                    {
                        var ext = gene.def.GetModExtension<GeneExtension>();
                        if (ext != null)
                        {
                            offset.x *= ext.headScaleFactor.x;
                            offset.y *= ext.headScaleFactor.y;
                        }
                    }
                }
            }
            if (ScaleCache.GetScaleCache(pawn) is SizeData data)
            {
                offset *= data.headRenderSize;
            }
            return offset;
        }
    }
}
