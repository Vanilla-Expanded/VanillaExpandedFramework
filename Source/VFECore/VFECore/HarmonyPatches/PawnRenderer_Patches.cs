using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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

        public float? yLayerNorthOverride;
        public float? yLayerSouthOverride;
        public float? yLayerWestOverride;
        public float? yLayerEastOverride;

        public Vector2? drawSize;
        public Vector2? drawNorthSize;
        public Vector2? drawSouthSize;
        public Vector2? drawWestSize;
        public Vector2? drawEastSize;
        public Vector3 GetDrawPosOffset(Rot4 rot, Vector3 loc)
        {
            loc = GetDrawPosOffsetInt(rot, loc);
            switch (rot.AsByte)
            {
                case 0: if (yLayerNorthOverride.HasValue) { loc.y = yLayerNorthOverride.Value; } break;
                case 1: if (yLayerEastOverride.HasValue) { loc.y = yLayerEastOverride.Value; } break;
                case 2: if (yLayerSouthOverride.HasValue) { loc.y = yLayerSouthOverride.Value; } break;
                case 3: if (yLayerWestOverride.HasValue) { loc.y = yLayerWestOverride.Value; } break;
            }
            return loc;
        }

        private Vector3 GetDrawPosOffsetInt(Rot4 rot, Vector3 loc)
        {
            switch (rot.AsByte)
            {
                case 0: if (drawPosNorthOffset.HasValue) { return loc + drawPosNorthOffset.Value; } break;
                case 1: if (drawPosEastOffset.HasValue) { return loc + drawPosEastOffset.Value; } break;
                case 2: if (drawPosSouthOffset.HasValue) { return loc + drawPosSouthOffset.Value; } break;
                case 3: if (drawPosWestOffset.HasValue) { return loc + drawPosWestOffset.Value; } break;
            }
            if (drawPosOffset.HasValue)
            {
                return loc + drawPosOffset.Value;
            }
            return loc;
        }

        private Vector2 GetDrawSize(Rot4 rot)
        {
            switch (rot.AsByte)
            {
                case 0: if (drawNorthSize.HasValue) { return drawNorthSize.Value; } break;
                case 1: if (drawSouthSize.HasValue) { return drawSouthSize.Value; } break;
                case 2: if (drawEastSize.HasValue) { return drawEastSize.Value; } break;
                case 3: if (drawWestSize.HasValue) { return drawWestSize.Value; } break;
            }
            if (drawSize.HasValue)
            {
                return drawSize.Value;
            }

            return default;
        }
        public Mesh TryGetNewMesh(Mesh mesh, Pawn pawn)
        {
            var rot = pawn.Rotation;
            var size = GetDrawSize(rot);
            if (size == default)
            {
                return mesh;
            }

            size.x *= mesh.vertices[2].x * 2f;
            size.y *= mesh.vertices[2].z * 2f;

            if (!newPlanes.TryGetValue(size, out var value))
            {
                value = new GraphicMeshSet(size.x, size.y);
                newPlanes.Add(size, value);
            }

            return value.MeshAt(rot);
        }

        public static Dictionary<Vector2, GraphicMeshSet> newPlanes = new();
    }

    /* TODO: Revisit later
    [HarmonyPatch]
    public static class Patch_DrawHeadHair_DrawApparel_Transpiler
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            var method = typeof(PawnRenderer).GetNestedTypes(AccessTools.all)
                .SelectMany(type => type.GetMethods(AccessTools.all))
                .FirstOrDefault(x => x.Name.Contains("<DrawHeadHair>") && x.Name.Contains("DrawApparel"));
            displayType = method.DeclaringType;
            return method;
        }
        private static Type displayType;
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            bool foundFirstBlock = false;
            bool foundSecondBlock = false;
            bool foundThirdBlock = false;
            var drawMeshNowOrLaterMethod = AccessTools.Method(typeof(GenDraw), "DrawMeshNowOrLater", new Type[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(bool) });

            var getThis = codes.First(ins => ins.opcode == OpCodes.Ldfld);

            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];

                if (!foundFirstBlock && codes[i].opcode == OpCodes.Stloc_0)
                {
                    foundFirstBlock = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return getThis.Clone();
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldloca, 0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_DrawHeadHair_DrawApparel_Transpiler), nameof(TryModifyMeshRef)));
                }
                if (foundFirstBlock && !foundSecondBlock && codes[i].opcode == OpCodes.Stloc_1 && codes[i + 1].opcode == OpCodes.Ldloc_0)
                {
                    foundSecondBlock = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(displayType, "headFacing"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(displayType, "onHeadLoc"));
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_DrawHeadHair_DrawApparel_Transpiler), nameof(TryModifyHeadGearLocRef)));
                }

                if (foundSecondBlock && !foundThirdBlock && i > 3 && codes[i - 3].opcode == OpCodes.Ldc_R4 && codes[i - 3].OperandIs(0.00289575267f) && codes[i - 2].opcode == OpCodes.Add && codes[i - 1].opcode == OpCodes.Stind_R4)
                {
                    foundThirdBlock = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(displayType, "headFacing"));
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_DrawHeadHair_DrawApparel_Transpiler), nameof(TryModifyHeadGearLoc)));
                    yield return new CodeInstruction(OpCodes.Stloc_3);
                }
            }
            if (!foundFirstBlock || !foundSecondBlock || !foundThirdBlock)
            {
                Log.Error($"[Vanilla Framework Expanded] Transpiler on PawnRenderer:DrawHeadHair+DrawApparel failed.");
            }
        }

        public static Vector3 TryModifyHeadGearLoc(Rot4 rot, Vector3 loc, ApparelGraphicRecord apparelRecord)
        {
            var extension = apparelRecord.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>();
            if (extension != null)
            {
                if (extension.apparelDrawSettings != null)
                {
                    return extension.apparelDrawSettings.GetDrawPosOffset(rot, loc);
                }
                if (extension.headgearDrawSettings != null)
                {
                    return extension.headgearDrawSettings.GetDrawPosOffset(rot, loc);
                }
            }
            return loc;
        }

        public static void TryModifyHeadGearLocRef(Rot4 rot, ref Vector3 loc, ApparelGraphicRecord apparelRecord)
        {
            var extension = apparelRecord.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>();
            if (extension != null)
            {
                if (extension.apparelDrawSettings != null)
                {
                    loc = extension.apparelDrawSettings.GetDrawPosOffset(rot, loc);
                }
                else if (extension.headgearDrawSettings != null)
                {
                    loc = extension.headgearDrawSettings.GetDrawPosOffset(rot, loc);
                }
            }
        }

        public static void TryModifyMeshRef(Pawn pawn, ref Mesh mesh, ApparelGraphicRecord apparelRecord)
        {
            var extension = apparelRecord.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>();
            if (extension != null)
            {
                if (extension.apparelDrawSettings != null)
                {
                    mesh = extension.apparelDrawSettings.TryGetNewMesh(mesh, pawn);
                }
                else if (extension.headgearDrawSettings != null)
                {
                    mesh = extension.headgearDrawSettings.TryGetNewMesh(mesh, pawn);
                }
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
            for (int i = 0; i < codes.Count; i++)
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
            if (mat != null)
            {
                Patch_PawnRenderer_DrawPawnBody_Transpiler.mappedValues[mat] = apparelGraphicRecord;
            }
        }
    }

    [HarmonyPatch(typeof(PawnGraphicSet), "ResolveApparelGraphics")]
    public static class Patch_PawnGraphicSet_ResolveApparelGraphics_Patch
    {
        public static void Postfix(PawnGraphicSet __instance)
        {
            foreach (var item in __instance.pawn.apparel.WornApparel)
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

        public static Dictionary<Material, ApparelGraphicRecord> mappedValues = new();
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            bool found1 = false;
            bool found2 = false;
            var drawMeshNowOrLaterMethod = AccessTools.Method(typeof(GenDraw), nameof(GenDraw.DrawMeshNowOrLater), new Type[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(bool) });
            for (int i = 0; i < codes.Count; i++)
            {
                if (!found1 && i > 3 && codes[i - 1].opcode == OpCodes.Stloc_S && codes[i - 1].operand is LocalBuilder { LocalIndex: 5 })
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
            if (mat != null && mappedValues.TryGetValue(mat, out var apparelRecord))
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
            if (mat != null && mappedValues.TryGetValue(mat, out var apparelRecord))
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
            var codes = instructions.ToList();
            bool foundFirstBlock = false;
            bool foundSecondBlock = false;
            bool foundThirdBlock = false;

            for (int i = 0; i < codes.Count; i++)
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
            if (extension != null)
            {
                if (extension.apparelDrawSettings != null)
                {
                    loc = extension.apparelDrawSettings.GetDrawPosOffset(rot, loc);
                }
                else if (extension.packPosDrawSettings != null)
                {
                    loc = extension.packPosDrawSettings.GetDrawPosOffset(rot, loc);
                }
            }

        }

        public static void ModifyShellLoc(Rot4 rot, ref Vector3 loc, ApparelGraphicRecord apparelRecord)
        {
            oldVector = loc;
            var extension = apparelRecord.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>();
            if (extension != null)
            {
                if (extension.apparelDrawSettings != null)
                {
                    loc = extension.apparelDrawSettings.GetDrawPosOffset(rot, loc);
                }
                else if (extension?.shellPosDrawSettings != null)
                {
                    loc = extension.shellPosDrawSettings.GetDrawPosOffset(rot, loc);
                }
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
            if (extension != null)
            {
                if (extension.apparelDrawSettings != null)
                {
                    mesh = extension.apparelDrawSettings.TryGetNewMesh(mesh, pawn);
                }
                else if (extension.shellPosDrawSettings != null)
                {
                    mesh = extension.shellPosDrawSettings.TryGetNewMesh(mesh, pawn);
                }
            }
        }

        public static void ModifyPackMesh(Pawn pawn, ref Mesh mesh, ApparelGraphicRecord apparelRecord)
        {
            oldMesh = mesh;
            var extension = apparelRecord.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>();
            if (extension != null)
            {
                if (extension.apparelDrawSettings != null)
                {
                    mesh = extension.apparelDrawSettings.TryGetNewMesh(mesh, pawn);
                }
                else if (extension.packPosDrawSettings != null)
                {
                    mesh = extension.packPosDrawSettings.TryGetNewMesh(mesh, pawn);
                }
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
            for (int i = 0; i < codes.Count; i++)
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

    [HarmonyPatch(typeof(PawnRenderer), "DrawHeadHair")]
    public static class PawnRenderer_DrawHeadHair_Patch
    {
        public static Dictionary<Pawn, bool> hatsWithHideHeadsDrawn = new Dictionary<Pawn, bool>();

        [HarmonyPriority(int.MinValue)]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            bool found = false;
            MethodInfo get_IdeologyActive = AccessTools.Property(typeof(ModsConfig), "IdeologyActive").GetGetMethod();
            var pawnField = AccessTools.Field(typeof(PawnRenderer), "pawn");
            foreach (var code in codes)
            {
                if (!found)
                {
                    if (code.Calls(get_IdeologyActive))
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
                        yield return new CodeInstruction(OpCodes.Ldloc_1);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnRenderer_DrawHeadHair_Patch), "RegisterDrawHat"));
                        found = true;
                    }
                }
                yield return code;
            }
        }
    
        public static void RegisterDrawHat(Pawn pawn, List<ApparelGraphicRecord> list)
        {
            if (list.Any(x => x.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>()?.hideHead ?? false))
            {

                hatsWithHideHeadsDrawn[pawn] = true;
            }
            else
            {
                hatsWithHideHeadsDrawn[pawn] = false;
            }
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
                var headgear = pawn.apparel.WornApparel
                    .FirstOrDefault(x => x.def.GetModExtension<ApparelDrawPosExtension>()?.hideHead ?? false);
                if (headgear != null)
                {
                    if (PawnRenderer_DrawHeadHair_Patch.hatsWithHideHeadsDrawn.TryGetValue(pawn, out var value) && !value)
                    {
                        return true;
                    }
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
                var pawn = ___pawn;
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

    [HarmonyPatch(typeof(PawnGraphicSet), nameof(PawnGraphicSet.ResolveAllGraphics))]
    public static class PawnGraphicSet_ResolveAllGraphics_Patch
    {
        public static void Postfix(PawnGraphicSet __instance)
        {
            var faction = __instance.pawn.Faction;
            // If the pawn's a pack animal and is part of a medieval faction, use medieval pack texture if applicable
            if (faction != null && __instance.pawn.RaceProps.packAnimal)
            {
                var factionDefExtension = FactionDefExtension.Get(faction.def);
                if (!factionDefExtension.packAnimalTexNameSuffix.NullOrEmpty())
                {
                    var medievalPackTexture = ContentFinder<Texture2D>.Get(__instance.nakedGraphic.path + $"{factionDefExtension.packAnimalTexNameSuffix}_south", false);
                    if (medievalPackTexture != null)
                    {
                        __instance.packGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.nakedGraphic.path + factionDefExtension.packAnimalTexNameSuffix, ShaderDatabase.CutoutComplex, __instance.nakedGraphic.drawSize, __instance.pawn.Faction.Color);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.DrawEquipmentAiming))]
    public static class PawnRenderer_DrawEquipmentAiming_Patch
    {
        [HarmonyDelegate(typeof(PawnRenderer), "CarryWeaponOpenly")]
        public delegate bool CarryWeaponOpenly();
        [HarmonyPriority(Priority.First)]
        public static void Prefix(PawnRenderer __instance, Pawn ___pawn, Thing eq, ref Vector3 drawLoc, ref float aimAngle, CarryWeaponOpenly carryWeaponOpenly)
        {
            var thingDefExtension = eq.def.GetModExtension<ThingDefExtension>();

            if (thingDefExtension != null && carryWeaponOpenly())
            {
                var pawn = ___pawn;
                var pawnRot = pawn.Rotation;

                // Weapon draw offsets that apply at all times (i.e. carrying weapons while working, drafted, attacking)
                // Replaces the now-defunct CompOversizedWeapon
                if (thingDefExtension.weaponCarryDrawOffsets != null)
                {
                    if (pawnRot == Rot4.South)
                    {
                        drawLoc += thingDefExtension.weaponCarryDrawOffsets.south.drawOffset;
                        aimAngle += thingDefExtension.weaponCarryDrawOffsets.south.angleOffset;
                    }
                    else if (pawnRot == Rot4.North)
                    {
                        drawLoc += thingDefExtension.weaponCarryDrawOffsets.north.drawOffset;
                        aimAngle += thingDefExtension.weaponCarryDrawOffsets.north.angleOffset;
                    }
                    else if (pawnRot == Rot4.East)
                    {
                        drawLoc += thingDefExtension.weaponCarryDrawOffsets.east.drawOffset;
                        aimAngle += thingDefExtension.weaponCarryDrawOffsets.east.angleOffset;
                    }
                    else if (pawnRot == Rot4.West)
                    {
                        drawLoc += thingDefExtension.weaponCarryDrawOffsets.west.drawOffset;
                        aimAngle += thingDefExtension.weaponCarryDrawOffsets.west.angleOffset;
                    }
                }

                // Weapon draw offsets that only apply when the pawn is drafted but *not* actively attacking
                // Useful for things like holding a pike/halberd while standing at attention
                //
                // Note: These offsets add on to anything in weaponCarryDrawOffsets
                if (thingDefExtension.weaponDraftedDrawOffsets != null && !___pawn.stances.curStance.StanceBusy)
                {
                    if (pawnRot == Rot4.South)
                    {
                        drawLoc += thingDefExtension.weaponDraftedDrawOffsets.south.drawOffset;
                        aimAngle += thingDefExtension.weaponDraftedDrawOffsets.south.angleOffset;
                    }
                    else if (pawnRot == Rot4.North)
                    {
                        drawLoc += thingDefExtension.weaponDraftedDrawOffsets.north.drawOffset;
                        aimAngle += thingDefExtension.weaponDraftedDrawOffsets.north.angleOffset;
                    }
                    else if (pawnRot == Rot4.East)
                    {
                        drawLoc += thingDefExtension.weaponDraftedDrawOffsets.east.drawOffset;
                        aimAngle += thingDefExtension.weaponDraftedDrawOffsets.east.angleOffset;
                    }
                    else if (pawnRot == Rot4.West)
                    {
                        drawLoc += thingDefExtension.weaponDraftedDrawOffsets.west.drawOffset;
                        aimAngle += thingDefExtension.weaponDraftedDrawOffsets.west.angleOffset;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnGraphicSet), "CalculateHairMats")]
    public static class PawnGraphicSet_CalculateHairMats_Patch
    {
        public static void Postfix(PawnGraphicSet __instance)
        {
            var hair = __instance.pawn?.story?.hairDef;
            if (hair != null && ContentFinder<Texture2D>.Get(hair.texPath + "_northm", reportFailure: false) != null)
            {
                __instance.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(hair.texPath, ShaderDatabase.CutoutComplex, Vector2.one, __instance.pawn.story.HairColor);
            }
        }
    }
    */
}
