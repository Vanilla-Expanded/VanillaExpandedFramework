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
        static MethodInfo headScaleFactor = AccessTools.Method("HumanlikeMeshPoolUtility_Patches:headSizeFactor");
        static MethodInfo headScaleFactorVector = AccessTools.Method("HumanlikeMeshPoolUtility_Patches:headSizeFactorVector");
        static MethodInfo updatedMeshSet = AccessTools.Method("HumanlikeMeshPoolUtility_Patches:GetUpdatedMeshSet");
        static MethodInfo updatedMeshSetXY = AccessTools.Method("HumanlikeMeshPoolUtility_Patches:GetUpdatedMeshSetXY");


        public static float GeneScaleFactor(float width, Pawn pawn)
        {
            var genes = pawn.genes;
            if (ModLister.BiotechInstalled && genes != null)
            {
                foreach (var gene in genes.GenesListForReading)
                {
                    var ext = gene.def.GetModExtension<GeneExtension>();
                    if (ext != null && ext.bodyScaleFactor != 1f)
                    {
                        width *= ext.bodyScaleFactor;
                    }
                }
            }
            return width;
        }
        public static float headSizeFactor(float headSize, Pawn pawn)
        {
            var genes = pawn.genes;
            if (ModLister.BiotechInstalled && genes != null)
            {

                foreach (var gene in genes.GenesListForReading)
                {
                    var ext = gene.def.GetModExtension<GeneExtension>();
                    if (ext != null && ext.headScaleFactor != 1f)
                    {
                        headSize *= ext.headScaleFactor;
                    }
                }
            }
            return headSize;
        }
        public static Vector2 headSizeFactorVector(Vector2 headSize, Pawn pawn)
        {
            var genes = pawn.genes;
            if (ModLister.BiotechInstalled && genes != null)
            {

                foreach (var gene in genes.GenesListForReading)
                {
                    var ext = gene.def.GetModExtension<GeneExtension>();
                    if (ext != null && ext.headScaleFactor != 1f)
                    {
                        headSize *= ext.headScaleFactor;
                    }
                }
            }
            return headSize;
        }
        public static GraphicMeshSet GetUpdatedMeshSet(Pawn pawn)
        {
            var genes = pawn.genes;
            float width = 1.5f;
            if (ModLister.BiotechInstalled && genes != null)
            {
                foreach (var gene in genes.GenesListForReading)
                {
                    var ext = gene.def.GetModExtension<GeneExtension>();
                    if (ext != null && ext.bodyScaleFactor != 1f)
                    {
                        width *= ext.bodyScaleFactor;
                    }
                }
            }
            return MeshPool.GetMeshSetForWidth(width);
        }
        public static GraphicMeshSet GetUpdatedMeshSetXY(float x, float y, Pawn pawn)
        {
            var genes = pawn.genes;

            if (ModLister.BiotechInstalled && genes != null)
            {
                foreach (var gene in genes.GenesListForReading)
                {
                    var ext = gene.def.GetModExtension<GeneExtension>();
                    if (ext != null && ext.headScaleFactor != 1f)
                    {
                        x *= ext.headScaleFactor;
                        y *= ext.headScaleFactor;
                    }
                }
            }
            return MeshPool.GetMeshSetForWidth(x, y);
        }

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
                        if (codes[i].Calls(AccessTools.Method("AlienRace.HarmonyPatches:GetHumanlikeBodySetForPawnHelper")) && codes[i - 2].opcode == OpCodes.Call)
                        {
                            yield return new CodeInstruction(OpCodes.Call, bodyScaleFactor);
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                        }
                        if (codes[i].opcode == OpCodes.Ldc_R4)
                        {
                            yield return codes[i];
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, bodyScaleFactor);
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
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, bodyScaleFactor);
                        }
                        if (codes[i].LoadsField(meshPoolHumanLike))
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
                        if (codes[i].Calls(AccessTools.Method("AlienRace.HarmonyPatches:GetHumanlikeHeadSetForPawnHelper")) && codes[i - 2].opcode == OpCodes.Call)
                        {
                            yield return new CodeInstruction(OpCodes.Call, headScaleFactor);
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                        }
                        if (codes[i].opcode == OpCodes.Ldc_R4)
                        {
                            yield return codes[i];
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, headScaleFactor);
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
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, bodyScaleFactor);
                        }
                        if (codes[i].LoadsField(meshPoolHeadHumanLike))
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
                        //alright trying to figure out how to get the field info for a nullable was making me tired so let me introduce big ol hack
                        //update after adding HAR compat fucks sake I did it dumb way
                        if (codes[i].opcode == OpCodes.Ldflda && codes[i + 3].opcode == OpCodes.Stloc_0 && !found)
                        {
                            found = true;
                            codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
                            codes.Insert(i + 3, new CodeInstruction(OpCodes.Call, headScaleFactor));

                        }
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
                        if (codes[i].opcode == OpCodes.Ldflda && codes[i + 3].opcode == OpCodes.Stloc_0 && !found)
                        {
                            found = true;
                            codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
                            codes.Insert(i + 3, new CodeInstruction(OpCodes.Call, headScaleFactor));

                        }
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
