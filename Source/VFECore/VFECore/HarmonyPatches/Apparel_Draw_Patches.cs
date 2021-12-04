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

namespace VFECore
{
    public class ApparelDrawPosExtension : DefModExtension
    {
        public DrawSettings headgearDrawSettings;
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
}
