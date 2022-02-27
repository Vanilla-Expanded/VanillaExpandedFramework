using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VFECore
{
    public class ApparelDrawPosExtension : DefModExtension
    {
        public DrawSettings headgearDrawSettings;
        public DrawSettings apparelDrawSettings;
        public DrawSettings packPosDrawSettings;
        public DrawSettings shellPosDrawSettings;

        public List<ThingDef> secondaryApparelGraphics;

        public bool isUnifiedApparel;
        public bool hideHead;
        public bool showBodyInBedAlways;
    }
    public class DrawSettings
    {
        public Vector3? drawPosOffset;
        public Vector3? drawPosNorthOffset;
        public Vector3? drawPosSouthOffset;
        public Vector3? drawPosWestOffset;
        public Vector3? drawPosEastOffset;

        public Vector2? drawSize;
        public Vector2? drawNorthSize;
        public Vector2? drawSouthSize;
        public Vector2? drawWestSize;
        public Vector2? drawEastSize;
        public Vector3 GetDrawPosOffset(Rot4 rot, Vector3 loc)
        {
            switch (rot.AsByte)
            {
                case 0: if (drawPosNorthOffset.HasValue) return loc + drawPosNorthOffset.Value; break;
                case 1: if (drawPosEastOffset.HasValue) return loc + drawPosEastOffset.Value; break;
                case 2: if (drawPosSouthOffset.HasValue) return loc + drawPosSouthOffset.Value; break;
                case 3: if (drawPosNorthOffset.HasValue) return loc + drawPosWestOffset.Value; break;
            }
            if (drawPosOffset.HasValue)
                return loc + drawPosOffset.Value;
            return loc;
        }

        private Vector2 GetDrawSize(Rot4 rot)
        {
            switch (rot.AsByte)
            {
                case 0: if (drawNorthSize.HasValue) return drawNorthSize.Value; break;
                case 1: if (drawSouthSize.HasValue) return drawSouthSize.Value; break;
                case 2: if (drawEastSize.HasValue) return drawEastSize.Value; break;
                case 3: if (drawWestSize.HasValue) return drawWestSize.Value; break;
            }
            if (drawSize.HasValue) return drawSize.Value;

            return default;
        }
        public Mesh TryGetNewMesh(Mesh mesh, Pawn pawn)
        {
            var rot = pawn.Rotation;
            var size = GetDrawSize(rot);
            if (size == default)
                return mesh;

            size.x *= mesh.vertices[2].x * 2f;
            size.y *= mesh.vertices[2].z * 2f;

            if (!newPlanes.TryGetValue(size, out var value))
            {
                value = new GraphicMeshSet(size.x, size.y);
                newPlanes.Add(size, value);
            }

            return value.MeshAt(rot);
        }

        public static Dictionary<Vector2, GraphicMeshSet> newPlanes = new Dictionary<Vector2, GraphicMeshSet>();
    }

    [HarmonyPatch]
    public static class Patch_DrawHeadHair_DrawApparel_Transpiler
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            return typeof(PawnRenderer).GetMethods(AccessTools.all).FirstOrDefault(x => x.Name.Contains("<DrawHeadHair>") && x.Name.Contains("DrawApparel"));
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            bool foundFirstBlock = false;
            bool foundSecondBlock = false;
            bool foundThirdBlock = false;
            var drawMeshNowOrLaterMethod = AccessTools.Method(typeof(GenDraw), "DrawMeshNowOrLater", new Type[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(bool) });
            var displayType = typeof(PawnRenderer).GetNestedTypes(AccessTools.all).First();
    
            for (var i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
    
                if (!foundFirstBlock && codes[i].opcode == OpCodes.Stloc_0)
                {
                    foundFirstBlock = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_DrawHeadHair_DrawApparel_Transpiler), nameof(TryModifyMeshRef)));
                }
                if (foundFirstBlock && !foundSecondBlock && codes[i].opcode == OpCodes.Stloc_1 && codes[i + 1].opcode == OpCodes.Ldloc_0)
                {
                    foundSecondBlock = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(displayType, "headFacing"));
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(displayType, "onHeadLoc"));
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_DrawHeadHair_DrawApparel_Transpiler), nameof(TryModifyHeadGearLocRef)));
                }
    
                if (foundSecondBlock && !foundThirdBlock && i > 3 && codes[i - 3].opcode == OpCodes.Ldc_R4 && codes[i - 3].OperandIs(0.00289575267f) && codes[i - 2].opcode == OpCodes.Add && codes[i - 1].opcode == OpCodes.Stind_R4)
                {
                    foundThirdBlock = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(displayType, "headFacing"));
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_DrawHeadHair_DrawApparel_Transpiler), nameof(TryModifyHeadGearLoc)));
                    yield return new CodeInstruction(OpCodes.Stloc_3);
                }
            }
            if (!foundFirstBlock || !foundSecondBlock || !foundThirdBlock)
            {
                Log.Error("[Vanilla Framework Expanded] Transpiler on PawnRenderer:DrawHeadHair+DrawApparel failed.");
            }
        }
    
        public static Vector3 TryModifyHeadGearLoc(Rot4 rot, Vector3 loc, ApparelGraphicRecord apparelRecord)
        {
            var extension = apparelRecord.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>();
            if (extension?.headgearDrawSettings != null)
            {
                return extension.headgearDrawSettings.GetDrawPosOffset(rot, loc);
            }
            return loc;
        }
    
        public static void TryModifyHeadGearLocRef(Rot4 rot, ref Vector3 loc, ApparelGraphicRecord apparelRecord)
        {
            var extension = apparelRecord.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>();
            if (extension?.headgearDrawSettings != null)
            {
                loc = extension.headgearDrawSettings.GetDrawPosOffset(rot, loc);
            }
        }
    
        public static void TryModifyMeshRef(Pawn pawn, ref Mesh mesh, ApparelGraphicRecord apparelRecord)
        {
            var extension = apparelRecord.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>();
            if (extension?.headgearDrawSettings != null)
            {
                mesh = extension.headgearDrawSettings.TryGetNewMesh(mesh, pawn);
            }
        }
    }
    
    [HarmonyPatch(typeof(PawnGraphicSet), "MatsBodyBaseAt")]
    public static class Patch_PawnGraphicSet_MatsBodyBaseAt_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var apparelGraphicsField = AccessTools.Field(typeof(PawnGraphicSet), "apparelGraphics");
            var matsAtMethod = AccessTools.Method(typeof(Graphic), nameof(Graphic.MatAt));
            var codes = codeInstructions.ToList();
            bool foundApparelGraphicsField = false;
            for (var i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (codes[i].LoadsField(apparelGraphicsField))
                {
                    foundApparelGraphicsField = true;
                }
                if (foundApparelGraphicsField && codes[i].Calls(matsAtMethod))
                {
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, apparelGraphicsField);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(List<ApparelGraphicRecord>), "get_Item"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_PawnGraphicSet_MatsBodyBaseAt_Transpiler), nameof(MapValue)));
                }
            }
            if (!foundApparelGraphicsField)
            {
                Log.Error("[Vanilla Framework Expanded] Transpiler on PawnGraphicSet:MatsBodyBaseAt failed.");
            }
        }

        public static void MapValue(Material mat, ApparelGraphicRecord apparelGraphicRecord)
        {
            Patch_PawnRenderer_DrawPawnBody_Transpiler.mappedValues[mat] = apparelGraphicRecord;
        }
    }

    [HarmonyPatch(typeof(PawnGraphicSet), "ResolveApparelGraphics")]
    public static class Patch_PawnGraphicSet_ResolveApparelGraphics_Patch
    {
        public static void Postfix(PawnGraphicSet __instance)
        {
            foreach (Apparel item in __instance.pawn.apparel.WornApparel)
            {
                TryAddSecondaryGraphics(item, __instance);
            }
        }
        public static void TryAddSecondaryGraphics(Apparel apparel, PawnGraphicSet __instance)
        {
            var extension = apparel.def.GetModExtension<ApparelDrawPosExtension>();
            if (extension?.secondaryApparelGraphics != null)
            {
                foreach (var thingDef in extension.secondaryApparelGraphics)
                {
                    var item = ThingMaker.MakeThing(thingDef) as Apparel;
                    if (ApparelGraphicRecordGetter.TryGetGraphicApparel(item, __instance.pawn.story.bodyType, out var rec))
                    {
                        __instance.apparelGraphics.Add(rec);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "DrawPawnBody")]
    public static class Patch_PawnRenderer_DrawPawnBody_Transpiler
    {
        public static Vector3 oldVector;
    
        public static Mesh oldMesh;
    
        public static Dictionary<Material, ApparelGraphicRecord> mappedValues = new Dictionary<Material, ApparelGraphicRecord>();
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            bool found1 = false;
            bool found2 = false;
            var drawMeshNowOrLaterMethod = AccessTools.Method(typeof(GenDraw), nameof(GenDraw.DrawMeshNowOrLater), new Type[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(bool) });
            for (var i = 0; i < codes.Count; i++)
            {
                if (!found1 && i > 3 && codes[i - 1].opcode == OpCodes.Stloc_S
                    && codes[i - 1].operand is LocalBuilder lb && lb.LocalIndex == 5
                    && codes[i].opcode == OpCodes.Ldarg_S && codes[i].OperandIs(6))
                {
                    found1 = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 1);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_PawnRenderer_DrawPawnBody_Transpiler), nameof(ModifyApparelLoc)));
    
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 6);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_PawnRenderer_DrawPawnBody_Transpiler), nameof(ModifyMesh)));
                }
                yield return codes[i];
                if (found1 && !found2 && codes[i].Calls(drawMeshNowOrLaterMethod))
                {
                    found2 = true;
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_PawnRenderer_DrawPawnBody_Transpiler), nameof(ResetVector)));
                
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 6);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_PawnRenderer_DrawPawnBody_Transpiler), nameof(ResetMesh)));
                }
            }
            if (!found1 || !found2)
            {
                Log.Error("[Vanilla Framework Expanded] Transpiler on PawnRenderer:DrawPawnBody failed.");
            }
        }
        public static void ModifyApparelLoc(Rot4 rot, ref Vector3 vector, Material mat)
        {
            oldVector = vector;
            if (mappedValues.TryGetValue(mat, out var apparelRecord))
            {
                var extension = apparelRecord.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>();
                if (extension?.apparelDrawSettings != null)
                {
                    vector = extension.apparelDrawSettings.GetDrawPosOffset(rot, vector);
                }
            }
        }
    
        public static void ResetVector(ref Vector3 vector)
        {
            vector = oldVector;
        }
    
        public static void ModifyMesh(Pawn pawn, ref Mesh mesh, Material mat)
        {
            oldMesh = mesh;
            if (mappedValues.TryGetValue(mat, out var apparelRecord))
            {
                var extension = apparelRecord.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>();
                if (extension?.apparelDrawSettings != null)
                {
                    mesh = extension.apparelDrawSettings.TryGetNewMesh(mesh, pawn);
                }
            }
        }
    
        public static void ResetMesh(ref Mesh mesh)
        {
            mesh = oldMesh;
        }
    }
    
    [HarmonyPatch(typeof(PawnRenderer), "DrawBodyApparel")]
    public static class Harmony_PawnRenderer_DrawBodyApparel
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var drawMeshNowOrLaterMethodMatrix = AccessTools.Method(typeof(GenDraw), nameof(GenDraw.DrawMeshNowOrLater), new Type[] { typeof(Mesh), typeof(Matrix4x4), typeof(Material), typeof(bool) });
            var drawMeshNowOrLaterMethodVector3 = AccessTools.Method(typeof(GenDraw), nameof(GenDraw.DrawMeshNowOrLater), new Type[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(bool) });
        
            var translateMethod = AccessTools.Method(typeof(Matrix4x4), nameof(Matrix4x4.Translate));
            List<CodeInstruction> codes = instructions.ToList();
            bool foundFirstBlock = false;
            bool foundSecondBlock = false;
            bool foundThirdBlock = false;
    
            for (var i = 0; i < codes.Count; i++)
            {
                if (!foundFirstBlock && i > 3 && codes[i - 3].opcode == OpCodes.Ldc_R4 && codes[i - 3].OperandIs(0.00289575267f) && codes[i - 2].opcode == OpCodes.Add && codes[i - 1].opcode == OpCodes.Stind_R4)
                {
                    foundFirstBlock = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 5).MoveLabelsFrom(codes[i]);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 5);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_PawnRenderer_DrawBodyApparel), nameof(ModifyShellLoc)));
                
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldarga_S, 3);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_PawnRenderer_DrawBodyApparel), nameof(ModifyShellMesh)));
                }
                yield return codes[i];
                if (foundFirstBlock && !foundSecondBlock && codes[i].Calls(drawMeshNowOrLaterMethodVector3))
                {
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 5);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_PawnRenderer_DrawBodyApparel), nameof(ResetLoc)));
                
                    yield return new CodeInstruction(OpCodes.Ldarga_S, 3);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_PawnRenderer_DrawBodyApparel), nameof(ResetMesh)));
                }
                if (!foundSecondBlock && codes[i + 2].Calls(translateMethod) && codes[i + 3].opcode == OpCodes.Ldloc_1)
                {
                    foundSecondBlock = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 5);
                    yield return new CodeInstruction(OpCodes.Ldarga_S, 2);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_PawnRenderer_DrawBodyApparel), nameof(ModifyPackLoc)));
                }
                if (foundSecondBlock && !foundThirdBlock && codes[i].Calls(drawMeshNowOrLaterMethodMatrix))
                {
                    yield return new CodeInstruction(OpCodes.Ldarga_S, 2);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_PawnRenderer_DrawBodyApparel), nameof(ResetLoc)));
                }
                if (!foundThirdBlock && i > 3 && codes[i - 2].Calls(drawMeshNowOrLaterMethodMatrix) && codes[i - 1].opcode == OpCodes.Br_S)
                {
                    foundThirdBlock = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 5);
                    yield return new CodeInstruction(OpCodes.Ldarga_S, 1);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_PawnRenderer_DrawBodyApparel), nameof(ModifyPackLoc)));
                }
                if (foundThirdBlock && codes[i].Calls(drawMeshNowOrLaterMethodVector3))
                {
                    yield return new CodeInstruction(OpCodes.Ldarga_S, 1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_PawnRenderer_DrawBodyApparel), nameof(ResetLoc)));
                }
            }
            if (!foundFirstBlock || !foundSecondBlock || !foundThirdBlock)
            {
                Log.Error("[Vanilla Framework Expanded] Transpiler on PawnRenderer:DrawBodyApparel failed.");
            }
        }
    
        public static Vector3 oldVector;
        public static Mesh oldMesh;
        public static void ModifyPackLoc(Rot4 rot, ref Vector3 loc, ApparelGraphicRecord apparelRecord)
        {
            oldVector = loc;
            var extension = apparelRecord.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>();
            if (extension?.packPosDrawSettings != null)
            {
                loc = extension.packPosDrawSettings.GetDrawPosOffset(rot, loc);
            }
        }
    
        public static void ModifyShellLoc(Rot4 rot, ref Vector3 loc, ApparelGraphicRecord apparelRecord)
        {
            oldVector = loc;
            var extension = apparelRecord.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>();
            if (extension?.shellPosDrawSettings != null)
            {
                loc = extension.shellPosDrawSettings.GetDrawPosOffset(rot, loc);
            }
        }
        public static void ResetLoc(ref Vector3 loc)
        {
            loc = oldVector;
        }
    
        public static void ModifyShellMesh(Pawn pawn, ref Mesh mesh, ApparelGraphicRecord apparelRecord)
        {
            oldMesh = mesh;
            var extension = apparelRecord.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>();
            if (extension?.shellPosDrawSettings != null)
            {
                mesh = extension.shellPosDrawSettings.TryGetNewMesh(mesh, pawn);
            }
        }
    
        public static void ModifyPackMesh(Pawn pawn, ref Mesh mesh, ApparelGraphicRecord apparelRecord)
        {
            oldMesh = mesh;
            var extension = apparelRecord.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>();
            if (extension?.packPosDrawSettings != null)
            {
                mesh = extension.packPosDrawSettings.TryGetNewMesh(mesh, pawn);
            }
        }
    
        public static void ResetMesh(ref Mesh mesh)
        {
            mesh = oldMesh;
        }
    
    }

    [HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
    public static class Patch_ApparelGraphicRecordGetter_TryGetGraphicApparel_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var renderAsPackMethod = AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.RenderAsPack));
            var codes = codeInstructions.ToList();
            bool found = false;
            for (var i = 0; i < codes.Count; i++)
            {
                yield return codes[i];

                if (codes[i].opcode == OpCodes.Brtrue_S && codes[i - 1].Calls(renderAsPackMethod))
                {
                    found = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_ApparelGraphicRecordGetter_TryGetGraphicApparel_Transpiler), nameof(IsUnifiedApparel)));
                    yield return new CodeInstruction(OpCodes.Brtrue_S, codes[i].operand);
                }
            }
            if (!found)
            {
                Log.Error("[Vanilla Framework Expanded] Transpiler on ApparelGraphicRecordGetter:TryGetGraphicApparel failed.");
            }
        }

        public static bool IsUnifiedApparel(Apparel apparel)
        {
            var extension = apparel.def.GetModExtension<ApparelDrawPosExtension>();
            return extension != null && extension.isUnifiedApparel;
        }
    }

    [HarmonyPatch]
    public static class FemaleBB_BodyType_Support_Patch
    {
        private static MethodBase target;
        private static bool Prepare()
        {
            target = AccessTools.Method(AccessTools.TypeByName("BBBodySupport.BBBodyTypeSupportHarmony+BBBodyGraphicApparelPatch"), "BBBody_ApparelPatch");
            return target != null;
        }

        [HarmonyTargetMethod]
        public static MethodBase GetMethod()
        {
            return target;
        }
        public static bool Prefix(ref Apparel apparel, ref BodyTypeDef bodyType, ref ApparelGraphicRecord rec, ref bool __3, ref bool __result)
        {
            if (Patch_ApparelGraphicRecordGetter_TryGetGraphicApparel_Transpiler.IsUnifiedApparel(apparel))
            {
                __3 = true;
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PawnGraphicSet), "HairMatAt")]
    public static class PawnGraphicSet_HairMatAt_Patch
    {
        public static bool Prefix(PawnGraphicSet __instance, ref Material __result, Rot4 facing, bool portrait = false, bool cached = false)
        {
            return ShouldShowHead(__instance.pawn, portrait);
        }

        public static bool ShouldShowHead(Pawn pawn, bool portrait)
        {
            if (Prefs.HatsOnlyOnMap && portrait)
            {
                return true;
            }
            if (pawn.apparel.AnyApparel)
            {
                if (pawn.apparel.WornApparel.Any(x => x.def.GetModExtension<ApparelDrawPosExtension>()?.hideHead ?? false))
                {
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PawnGraphicSet), "BeardMatAt")]
    public static class PawnGraphicSet_BeardMatAt_Patch
    {
        public static bool Prefix(PawnGraphicSet __instance, ref Material __result, Rot4 facing, bool portrait = false, bool cached = false)
        {
            return PawnGraphicSet_HairMatAt_Patch.ShouldShowHead(__instance.pawn, portrait);
        }
    }

    [HarmonyPatch(typeof(PawnGraphicSet), "HeadMatAt")]
    public static class PawnGraphicSet_HeadMatAt_Patch
    {
        public static bool Prefix(PawnGraphicSet __instance, ref Material __result, Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh, bool stump = false, bool portrait = false, bool allowOverride = true)
        {
            return PawnGraphicSet_HairMatAt_Patch.ShouldShowHead(__instance.pawn, portrait);
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "GetBodyPos")]
    public static class PawnRenderer_GetBodyPos_Patch
    {
        public static void Postfix(Pawn ___pawn, Vector3 drawLoc, ref bool showBody)
        {
            if (!showBody)
            {
                Pawn pawn = ___pawn;
                if (pawn.apparel.AnyApparel && ___pawn.CurrentBed() != null)
                {
                    if (pawn.apparel.WornApparel.Any(x => x.def.GetModExtension<ApparelDrawPosExtension>()?.showBodyInBedAlways ?? false))
                    {
                        showBody = true;
                    }
                }
            }
        }
    }
}
