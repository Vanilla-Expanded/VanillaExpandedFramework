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
    /// just modifying the lifestage factor on the stack then let HAR method do the actual vector manipulation
    /// Only has single factor so can't make wide without tall etc.
    /// vector2 could be added easily for non HAR compat, HAR compat would need to be revisited to make that work
    /// </summary>

    public static class HumanlikeMeshPoolUtility_Patches
    {
        static MethodInfo meshX = AccessTools.Method(typeof(MeshPool), "GetMeshSetForWidth", new Type[] { typeof(float) });
        static MethodInfo meshXY = AccessTools.Method(typeof(MeshPool), "GetMeshSetForWidth", new Type[] { typeof(float), typeof(float) });
        static FieldInfo meshPoolHumanLike = AccessTools.Field("MeshPool:humanlikeBodySet");
        static FieldInfo meshPoolHeadHumanLike = AccessTools.Field("MeshPool:humanlikeHeadSet");
        static MethodInfo bodyScaleFactor = AccessTools.Method("HumanlikeMeshPoolUtility_Patches:GeneScaleFactor");
        static MethodInfo bodyScaleFactorVect2 = AccessTools.Method("HumanlikeMeshPoolUtility_Patches:GeneScaleFactorVect2"); 
        static MethodInfo headScaleFactorVector = AccessTools.Method("HumanlikeMeshPoolUtility_Patches:headSizeFactorVector");
        static MethodInfo headScaleVectorFromFactor = AccessTools.Method("HumanlikeMeshPoolUtility_Patches:headSizeVectorFromFactor");
        static MethodInfo updatedMeshSet = AccessTools.Method("HumanlikeMeshPoolUtility_Patches:GetUpdatedMeshSet");
        static MethodInfo updatedHeadMeshSet = AccessTools.Method("HumanlikeMeshPoolUtility_Patches:GetUpdatedHeadMeshSet");
        static MethodInfo updatedMeshSetXY = AccessTools.Method("HumanlikeMeshPoolUtility_Patches:GetUpdatedMeshSetXY");


        public static float GeneScaleFactor(float width, Pawn pawn)
        {
            var genes = pawn.genes;
            if (ModLister.BiotechInstalled && genes != null)
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
            if (ModLister.BiotechInstalled && genes != null)
            {
                foreach (var gene in genes.GenesListForReading)
                {
                    var ext = gene.def.GetModExtension<GeneExtension>();
                    if (ext != null)
                    {
                        scaling = new Vector2(scaling.x * ext.bodyScaleFactor.x, scaling.y * ext.bodyScaleFactor.y);
                    }
                }
            }
            return scaling;
        }

        //Used when HAR is loaded
        public static Vector2 headSizeFactorVector(Vector2 headSize, Pawn pawn)
        {
            var genes = pawn.genes;
            if (ModLister.BiotechInstalled && genes != null)
            {

                foreach (var gene in genes.GenesListForReading)
                {
                    var ext = gene.def.GetModExtension<GeneExtension>();
                    if (ext != null)
                    {
                        headSize = new Vector2(headSize.x * ext.headScaleFactor.x, headSize.y * ext.headScaleFactor.y);
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
            if (ModLister.BiotechInstalled && genes != null)
            {

                foreach (var gene in genes.GenesListForReading)
                {
                    var ext = gene.def.GetModExtension<GeneExtension>();
                    if (ext != null)
                    {
                        scaling = new Vector2(scaling.x * ext.headScaleFactor.x, scaling.y * ext.headScaleFactor.y);
                    }
                }
            }
            return scaling;
        }
        //Used when HAR is not loaded
        public static GraphicMeshSet GetUpdatedMeshSet(Pawn pawn)
        {
            var genes = pawn.genes;
            Vector2 width = new Vector2(1.5f, 1.5f);
            if (ModLister.BiotechInstalled && genes != null)
            {
                var bodyWidth = pawn.ageTracker.CurLifeStage.bodyWidth;//Because I dont want to make 2 methods per head/body to accomodate this
                if (bodyWidth != null)
                {
                    width = new Vector2(bodyWidth.Value, bodyWidth.Value);
                }
                foreach (var gene in genes.GenesListForReading)
                {
                    var ext = gene.def.GetModExtension<GeneExtension>();                    
                    if (ext != null)
                    {
                        width = new Vector2(width.x * ext.bodyScaleFactor.x, width.y * ext.bodyScaleFactor.y);
                    }
                }
            }
            return MeshPool.GetMeshSetForWidth(width.x,width.y);
        }
        //Used when HAR is not loaded
        public static GraphicMeshSet GetUpdatedHeadMeshSet(Pawn pawn)
        {
            var genes = pawn.genes;            
            Vector2 width = new Vector2(1.5f, 1.5f);
            if (ModLister.BiotechInstalled && genes != null)
            {
                var bodyWidth = pawn.ageTracker.CurLifeStage.bodyWidth;//Because I dont want to make 2 methods per head/body to accomodate this
                if (bodyWidth != null)
                {
                    width = new Vector2(bodyWidth.Value, bodyWidth.Value);
                }
                foreach (var gene in genes.GenesListForReading)
                {
                    var ext = gene.def.GetModExtension<GeneExtension>();
                    if (ext != null)
                    {
                        width = new Vector2(width.x * ext.headScaleFactor.x, width.y * ext.headScaleFactor.y);
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

            if (ModLister.BiotechInstalled && genes != null)
            {
                foreach (var gene in genes.GenesListForReading)
                {
                    var ext = gene.def.GetModExtension<GeneExtension>();
                    if (ext != null)
                    {
                        x *= ext.headScaleFactor.x;
                        y *= ext.headScaleFactor.y;
                    }
                }
            }
            return MeshPool.GetMeshSetForWidth(x, y);
        }
        //This is still only the X factor due to the return of method being float. This method isn't actually used by ludeon at all so I can probably just remove it outright
        //But will leave just in case some mod is using it
        [HarmonyPatch(typeof(HumanlikeMeshPoolUtility), "HumanlikeBodyWidthForPawn")]
        public static class HumanlikeBodyWidthForPawn_Patch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].Calls(meshX))
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, bodyScaleFactor);
                    }
                    yield return codes[i];
                    if (codes[i].opcode == OpCodes.Ldc_R4)
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, bodyScaleFactor);
                    }
                }
            }
        }
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
                        if (codes[i].opcode == OpCodes.Box)//Removing HAR Boxes as we will box the vect 2 instead
                        {
                            continue;
                        }
                        if (codes[i].Calls(AccessTools.Method("AlienRace.HarmonyPatches:GetHumanlikeBodySetForPawnHelper")) && codes[i - 3].opcode == OpCodes.Call)
                        {
                            yield return new CodeInstruction(OpCodes.Call, bodyScaleFactorVect2);
                            yield return new CodeInstruction(OpCodes.Box,typeof(Vector2));
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
                        }
                    }
                    else
                    {
                        if (codes[i].Calls(meshX))
                        {
                            yield return new CodeInstruction(OpCodes.Pop);
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, updatedMeshSet); //changed to not call original meshsetforwidth
                        }
                        else if (codes[i].LoadsField(meshPoolHumanLike))
                        {
                            codes[i].opcode = OpCodes.Nop;
                            yield return codes[i];
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
                        if (codes[i].opcode == OpCodes.Box) //Removing HAR Boxes as we will box the vect 2 instead
                        {
                            continue;
                        }
                        if (codes[i].Calls(AccessTools.Method("AlienRace.HarmonyPatches:GetHumanlikeHeadSetForPawnHelper")) && codes[i - 2].opcode == OpCodes.Call)
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
                        }
                    }
                    else
                    {
                        if (codes[i].Calls(meshX))
                        {
                            yield return new CodeInstruction(OpCodes.Pop);
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, updatedHeadMeshSet);
                        }
                        else if (codes[i].LoadsField(meshPoolHeadHumanLike))
                        {
                            codes[i].opcode = OpCodes.Nop;
                            yield return codes[i];
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
                        var helper = AccessTools.Method("AlienRace.HarmonyPatches:GetHumanlikeHairSetForPawnHelper");
                        if (helper != null)
                        {
                            if (codes[i].Calls(helper) && !found)
                            {
                                found = true;
                                codes.Insert(i, new CodeInstruction(OpCodes.Call, headScaleFactorVector));
                                codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                            }
                        }
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
                        var helper = AccessTools.Method("AlienRace.HarmonyPatches:GetHumanlikeHairSetForPawnHelper");
                        if (helper != null)
                        {
                            if (codes[i].Calls(helper) && !found)
                            {
                                found = true;
                                codes.Insert(i, new CodeInstruction(OpCodes.Call, headScaleFactorVector));
                                codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                            }
                        }
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
