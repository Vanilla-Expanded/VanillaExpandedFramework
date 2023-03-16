using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using VFECore;
using RimWorld;
using Verse;


namespace VanillaGenesExpanded
{
    /// <summary>
    /// Patches to add pawn scaling factors via Genes.
    /// Compat added for HAR by letting HAR methods take priority 
    /// Using HumanlikeMeshPoolUtility:HumanlikeBodyWidthForPawn to get initial bodywidth so other modders have a place to patch
    /// This doesnt apply to HAR compat, might try to add later but first need to see what the final version it looks like
    /// </summary>

    public static class HumanlikeMeshPoolUtility_Patches
    {
        static MethodInfo meshX = AccessTools.Method(typeof(MeshPool), "GetMeshSetForWidth", new Type[] { typeof(float) });
        static MethodInfo meshXY = AccessTools.Method(typeof(MeshPool), "GetMeshSetForWidth", new Type[] { typeof(float), typeof(float) });
        static FieldInfo meshPoolHumanLike = AccessTools.Field(typeof(MeshPool),"humanlikeBodySet");
        static FieldInfo meshPoolHeadHumanLike = AccessTools.Field(typeof(MeshPool), "humanlikeHeadSet");
        static MethodInfo bodyScaleFactor = AccessTools.Method(typeof(HumanlikeMeshPoolUtility_Patches),"GeneScaleFactor");
        static MethodInfo bodyScaleFactorVect2 = AccessTools.Method(typeof(HumanlikeMeshPoolUtility_Patches),"GeneScaleFactorVect2");
        static MethodInfo headScaleFactorVector = AccessTools.Method(typeof(HumanlikeMeshPoolUtility_Patches),"headSizeFactorVector");
        static MethodInfo headScaleVectorFromFactor = AccessTools.Method(typeof(HumanlikeMeshPoolUtility_Patches),"headSizeVectorFromFactor");
        static MethodInfo updatedMeshSet = AccessTools.Method(typeof(HumanlikeMeshPoolUtility_Patches),"GetUpdatedMeshSet");
        static MethodInfo updatedHeadMeshSet = AccessTools.Method(typeof(HumanlikeMeshPoolUtility_Patches),"GetUpdatedHeadMeshSet");
        static MethodInfo updatedMeshSetXY = AccessTools.Method(typeof(HumanlikeMeshPoolUtility_Patches),"GetUpdatedMeshSetXY");


        public static float GeneScaleFactor(float width, Pawn pawn)
        {
            var genes = pawn.genes;
            if (ModsConfig.BiotechActive && genes != null)
            {
                foreach (var gene in genes.GenesListForReading)
                {
                    var ext = gene.def.GetModExtension<GeneExtension>();
                    if (ext != null)
                    {
                        width *= ext.bodyScaleFactor.x;
                    }
                }
            }
            return width;
        }
        //Used when HAR is loaded
        public static Vector2 GeneScaleFactorVect2(float width, Pawn pawn)
        {
            var genes = pawn.genes;
            Vector2 scaling = new(width, width);
            if (ModsConfig.BiotechActive && genes != null)
            {
                foreach (var gene in genes.GenesListForReading)
                {
                    if (gene.Active)
                    {
                        var ext = gene.def.GetModExtension<GeneExtension>();
                        if (ext != null)
                        {
                            scaling = new Vector2(scaling.x * ext.bodyScaleFactor.x, scaling.y * ext.bodyScaleFactor.y);
                        }
                    }
                }
            }
            return scaling;
        }

        //Used when HAR is loaded
        public static Vector2 headSizeFactorVector(Vector2 headSize, Pawn pawn)
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
                            headSize = new Vector2(headSize.x * ext.headScaleFactor.x, headSize.y * ext.headScaleFactor.y);
                        }
                    }
                }
            }
            return headSize;
        }
        //Used when HAR is loaded
        public static Vector2 headSizeVectorFromFactor(float headSize, Pawn pawn)
        {
            var genes = pawn.genes;
            Vector2 scaling = new(headSize, headSize);
            if (ModsConfig.BiotechActive && genes != null)
            {
                foreach (var gene in genes.GenesListForReading)
                {
                    if (gene.Active)
                    {
                        var ext = gene.def.GetModExtension<GeneExtension>();
                        if (ext != null)
                        {
                            scaling = new Vector2(scaling.x * ext.headScaleFactor.x, scaling.y * ext.headScaleFactor.y);
                        }
                    }
                }
            }
            return scaling;
        }
        //Used when HAR is not loaded
        public static GraphicMeshSet GetUpdatedMeshSet(float factor, Pawn pawn)
        {
            //This is being added for easy compat with other mods. To allow them to a method to patch that will automatically work without transpilers            
            var bodyWidth = HumanlikeMeshPoolUtility.HumanlikeBodyWidthForPawn(pawn);
            var genes = pawn.genes;
            if(bodyWidth != factor)
            {
                factor = bodyWidth;
            }
            Vector2 width = new Vector2(factor, factor);
            if (ModsConfig.BiotechActive && genes != null)
            {
                foreach (var gene in genes.GenesListForReading)
                {
                    if (gene.Active)
                    {
                        var ext = gene.def.GetModExtension<GeneExtension>();
                        if (ext != null)
                        {
                            width = new Vector2(width.x * ext.bodyScaleFactor.x, width.y * ext.bodyScaleFactor.y);
                        }
                    }
                }
            }
            return MeshPool.GetMeshSetForWidth(width.x, width.y);
        }

        //Used when HAR is not loaded
        public static GraphicMeshSet GetUpdatedHeadMeshSet(float factor,Pawn pawn)
        {
            var genes = pawn.genes;
            var bodyWidth = HumanlikeMeshPoolUtility.HumanlikeBodyWidthForPawn(pawn);
            if (bodyWidth != factor)
            {
                factor = bodyWidth;
            }
            Vector2 width = new Vector2(factor, factor);
            if (ModsConfig.BiotechActive && genes != null)
            {
                foreach (var gene in genes.GenesListForReading)
                {
                    if (gene.Active)
                    {
                        var ext = gene.def.GetModExtension<GeneExtension>();
                        if (ext != null)
                        {
                            width = new Vector2(width.x * ext.headScaleFactor.x, width.y * ext.headScaleFactor.y);
                        }
                    }
                }
            }
            return MeshPool.GetMeshSetForWidth(width.x, width.y);
        }
        //Used when HAR is not loaded
        //Also only used with hair/beard so head scale factor
        public static GraphicMeshSet GetUpdatedMeshSetXY(float x, float y, Pawn pawn)
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
                            x *= ext.headScaleFactor.x;
                            y *= ext.headScaleFactor.y;
                        }
                    }
                }
            }
            return MeshPool.GetMeshSetForWidth(x, y);
        }
        //removed patch on BodyWidth for pawn due to ludeon not using it and it being only a single factor
        //This leaves it open to be used by other mods to feed their width into our transpilers to avoid further transpiler fighting between mods
        //[HarmonyPatch(typeof(HumanlikeMeshPoolUtility), "HumanlikeBodyWidthForPawn")]

        [HarmonyPatch(typeof(HumanlikeMeshPoolUtility), "GetHumanlikeBodySetForPawn")]
        [HarmonyAfter("rimworld.erdelf.alien_race.main")]

        public static class GetHumanlikeBodySetForPawn_Patch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                for (int i = 0; i < codes.Count; i++)
                {
                    if (ModCompatibilityCheck.HumanAlienRace)
                    {
/*                        if (codes[i].opcode == OpCodes.Box)//Removing HAR Boxes as we will box the vect 2 instead
                        {
                            continue;
                        }
                        if (codes[i].Calls(AccessTools.Method("AlienRace.HarmonyPatches:GetHumanlikeBodySetForPawnHelper")) && codes[i - 3].opcode == OpCodes.Call)
                        {
                            yield return new CodeInstruction(OpCodes.Call, bodyScaleFactorVect2);
                            yield return new CodeInstruction(OpCodes.Box, typeof(Vector2));
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                        }
                        if (codes[i].opcode == OpCodes.Ldc_R4)
                        {
                            yield return codes[i];
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, bodyScaleFactorVect2);
                            yield return new CodeInstruction(OpCodes.Box, typeof(Vector2));
                        }
                        else
                        {
                            yield return codes[i];
                        }*/
                        yield return codes[i];
                    }
                    else
                    {
                        if (codes[i].Calls(meshX))
                        {
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, updatedMeshSet); //changed to not call original meshsetforwidth
                        }
                        else if (codes[i].LoadsField(meshPoolHumanLike))
                        {
                            codes[i].opcode = OpCodes.Nop;
                            yield return codes[i];
                            yield return new CodeInstruction(OpCodes.Ldc_R4, 1.5f);
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, updatedMeshSet);
                        }
                        else
                        {
                            yield return codes[i];
                        }
                    }


                }
            }
        }
        [HarmonyPatch(typeof(HumanlikeMeshPoolUtility), "GetHumanlikeHeadSetForPawn")]
        [HarmonyAfter("rimworld.erdelf.alien_race.main")]
        public static class GetHumanlikeHeadSetForPawn_Patch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                for (int i = 0; i < codes.Count; i++)
                {
                    if (ModCompatibilityCheck.HumanAlienRace)
                    {
/*                        if (codes[i].opcode == OpCodes.Box) //Removing HAR Boxes as we will box the vect 2 instead
                        {
                            continue;
                        }
                        if (codes[i].Calls(AccessTools.Method("AlienRace.HarmonyPatches:GetHumanlikeHeadSetForPawnHelper")) && codes[i - 3].opcode == OpCodes.Call)
                        {
                            yield return new CodeInstruction(OpCodes.Call, headScaleVectorFromFactor);
                            yield return new CodeInstruction(OpCodes.Box, typeof(Vector2));
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                        }
                        if (codes[i].opcode == OpCodes.Ldc_R4)
                        {
                            yield return codes[i];
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, headScaleVectorFromFactor);
                            yield return new CodeInstruction(OpCodes.Box, typeof(Vector2));
                        }
                        else
                        {
                            yield return codes[i];
                        }*/
                        yield return codes[i];
                    }
                    else
                    {
                        if (codes[i].Calls(meshX))
                        {                            
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, updatedHeadMeshSet);
                        }
                        else if (codes[i].LoadsField(meshPoolHeadHumanLike))
                        {
                            codes[i].opcode = OpCodes.Nop;
                            yield return codes[i];
                            yield return new CodeInstruction(OpCodes.Ldc_R4, 1.5f);
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, updatedHeadMeshSet);
                        }
                        else
                        {
                            yield return codes[i];
                        }
                    }


                }
            }
        }
        [HarmonyPatch(typeof(HumanlikeMeshPoolUtility), "GetHumanlikeHairSetForPawn")]
        [HarmonyAfter("rimworld.erdelf.alien_race.main")]
        public static class GetHumanlikeHairSetForPawn_Patch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                bool found = false;
                bool foundXY = false;
                for (int i = 0; i < codes.Count; i++)
                {
                    if (ModCompatibilityCheck.HumanAlienRace)
                    {
/*                        var helper = AccessTools.Method("AlienRace.HarmonyPatches:GetHumanlikeHairSetForPawnHelper");
                        if (helper != null)
                        {
                            if (codes[i].Calls(helper) && !found)
                            {
                                found = true;
                                codes.Insert(i, new CodeInstruction(OpCodes.Call, headScaleFactorVector));
                                codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                            }
                        }*/
                    }
                    else
                    {
                        if (codes[i].Calls(meshXY) && !foundXY)
                        {
                            foundXY = true;
                            codes[i].opcode = OpCodes.Nop;
                            codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                            codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, updatedMeshSetXY));
                        }
                    }

                }
                foreach (var code in codes)
                    yield return code;
            }
        }
        [HarmonyPatch(typeof(HumanlikeMeshPoolUtility), "GetHumanlikeBeardSetForPawn")]
        [HarmonyAfter("rimworld.erdelf.alien_race.main")]
        public static class GetHumanlikeBeardSetForPawn_Patch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                bool found = false;
                bool foundXY = false;

                for (int i = 0; i < codes.Count; i++)
                {
                    if (ModCompatibilityCheck.HumanAlienRace)
                    {
/*                        var helper = AccessTools.Method("AlienRace.HarmonyPatches:GetHumanlikeHairSetForPawnHelper");
                        if (helper != null)
                        {
                            if (codes[i].Calls(helper) && !found)
                            {
                                found = true;
                                codes.Insert(i, new CodeInstruction(OpCodes.Call, headScaleFactorVector));
                                codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                            }
                        }*/
                    }
                    else
                    {
                        if (codes[i].Calls(meshXY) && !foundXY)
                        {
                            foundXY = true;
                            codes[i].opcode = OpCodes.Nop;
                            codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                            codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, updatedMeshSetXY));
                        }
                    }

                }
                foreach (var code in codes)
                    yield return code;
            }
        }
    }
}
