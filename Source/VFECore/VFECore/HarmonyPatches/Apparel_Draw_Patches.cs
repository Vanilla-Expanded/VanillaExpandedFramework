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
using Verse.AI;
using static RimWorld.FleshTypeDef;

namespace VFECore
{
    public class ApparelDrawPosExtension : DefModExtension
    {
        public DrawSettings headgearDrawSettings;
        public DrawSettings apparelDrawSettings;
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

        public Vector3 GetDrawPosOffset(Pawn pawn, Vector3 loc)
        {
            switch (pawn.Rotation.AsByte)
            {
                case 0: if (drawPosNorthOffset.HasValue) return loc + drawPosNorthOffset.Value; break;
                case 1: if (drawPosNorthOffset.HasValue) return loc + drawPosEastOffset.Value; break;
                case 2: if (drawPosNorthOffset.HasValue) return loc + drawPosSouthOffset.Value; break;
                case 3: if (drawPosNorthOffset.HasValue) return loc + drawPosWestOffset.Value; break;
            }
            if (drawPosOffset.HasValue)
                return loc + drawPosOffset.Value;
            return loc;
        }
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
            bool found = false;
            for (var i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (!found && i > 3 && codes[i - 3].opcode == OpCodes.Ldc_R4 && codes[i - 3].OperandIs(0.00289575267f) && codes[i - 2].opcode == OpCodes.Add && codes[i - 1].opcode == OpCodes.Stind_R4)
                {
                    found = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_DrawHeadHair_DrawApparel_Transpiler), nameof(ModifyHeadGearLoc)));
                    yield return new CodeInstruction(OpCodes.Stloc_3);
                }
            }
            if (!found)
            {
                Log.Error("[Vanilla Framework Expanded] Transpiler on PawnRenderer:DrawHeadHair+DrawApparel failed.");
            }
        }

        public static Vector3 ModifyHeadGearLoc(Pawn pawn, Vector3 loc, ApparelGraphicRecord apparelRecord)
        {
            var extension = apparelRecord.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>();
            if (extension != null && extension.headgearDrawSettings != null)
            {
                return extension.headgearDrawSettings.GetDrawPosOffset(pawn, loc);
            }
            return loc;
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
        }

        public static void MapValue(Material mat, ApparelGraphicRecord apparelGraphicRecord)
        {
            Patch_PawnRenderer_DrawPawnBody_Transpiler.mappedValues[mat] = apparelGraphicRecord;
        }
    }
    
    [HarmonyPatch(typeof(PawnRenderer), "DrawPawnBody")]
    public static class Patch_PawnRenderer_DrawPawnBody_Transpiler
    {
        public static Vector3 oldVector;
    
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
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 1);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_PawnRenderer_DrawPawnBody_Transpiler), nameof(ModifyApparelLoc)));
                }
                yield return codes[i];
                if (found1 && !found2 && codes[i].Calls(drawMeshNowOrLaterMethod))
                {
                    found2 = true;
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_PawnRenderer_DrawPawnBody_Transpiler), nameof(ResetVector)));
}
            }
            if (!found1 && !found2)
            {
                Log.Error("[Vanilla Framework Expanded] Transpiler on PawnRenderer:DrawPawnBody failed.");
            }
        }
        public static void ModifyApparelLoc(Pawn pawn, ref Vector3 vector, Material mat)
        {
            oldVector = vector;
            if (mappedValues.TryGetValue(mat, out var apparelRecord))
            {
                var extension = apparelRecord.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>();
                if (extension != null)
                {
                    vector = extension.apparelDrawSettings.GetDrawPosOffset(pawn, vector);
                }
            }
        }
    
        public static void ResetVector(ref Vector3 vector)
        {
            vector = oldVector;
        }
    }
}
