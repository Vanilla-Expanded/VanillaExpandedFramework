using System.Collections.Generic;
using System.Linq;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using RimWorld;
using UnityEngine;

namespace VanillaGenesExpanded
{

    [HarmonyPatch]
    public static class DrawGeneEyes_Patch
    {
        public static MethodBase TargetMethod()
        {
            foreach (var type in typeof(PawnRenderer).GetNestedTypes(AccessTools.all))
            {
                foreach (var method in type.GetMethods(AccessTools.all))
                {
                    if (method.Name.Contains("DrawExtraEyeGraphic"))
                    {
                        return method;
                    }
                }
            }
            return null;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var nestedType = typeof(PawnRenderer).GetNestedTypes(AccessTools.all)
                    .First(c => c.Name.Contains("c__DisplayClass54_0"));
            var thisClass = AccessTools.Field(nestedType, "<>4__this");
            var pawnField = AccessTools.Field(thisClass.FieldType, "pawn");
            var drawScaleField = AccessTools.Field(typeof(GeneGraphicData), nameof(GeneGraphicData.drawScale));
            var offsetField = AccessTools.Field(typeof(BodyTypeDef.WoundAnchor), nameof(BodyTypeDef.WoundAnchor.offset));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, thisClass);
            yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DrawGeneEyes_Patch), nameof(SetHeadScale)));
            yield return new CodeInstruction(OpCodes.Starg_S, 2);
            int patched = 0;
            var codes = codeInstructions.ToList();
            for (var i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                yield return code;
                if (patched < 2 && code.LoadsField(offsetField) && (codes[i - 1].opcode == OpCodes.Ldloc_3 || codes[i - 1].opcode == OpCodes.Ldloc_S 
                    && codes[i - 1].operand is LocalBuilder lb && lb.LocalIndex == 4))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, thisClass);
                    yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DrawGeneEyes_Patch), nameof(ChangeOffset)));
                    patched++;
                }
                else if (patched >= 2)
                {
                    if (code.opcode == OpCodes.Stloc_S && code.operand is LocalBuilder lb2 && lb2.LocalIndex == 6)
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 6);
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, thisClass);
                        yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DrawGeneEyes_Patch), nameof(ChangeOffset)));
                        yield return new CodeInstruction(OpCodes.Stloc_S, 6);
                    }
                    else if (code.opcode == OpCodes.Stloc_S && code.operand is LocalBuilder lb3 && lb3.LocalIndex == 8)
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 8);
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, thisClass);
                        yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DrawGeneEyes_Patch), nameof(ChangeOffset)));
                        yield return new CodeInstruction(OpCodes.Stloc_S, 8);
                    }
                }
            }
        }

        public static float SetHeadScale(Pawn pawn, float scale)
        {
            foreach (var g in pawn.genes.GenesListForReading.Where(x => x.Active))
            {
                if (g.Active)
                {
                    var extension = g.def.GetModExtension<GeneExtension>();
                    if (extension != null)
                    {
                        scale *= extension.headScaleFactor.x;
                    }
                }
            }
            return scale;
        }

        public static Vector3 ChangeOffset(Vector3 offset, Pawn pawn)
        {
            foreach (var g in pawn.genes.GenesListForReading.Where(x => x.Active))
            {
                if (g.Active)
                {
                    var extension = g.def.GetModExtension<GeneExtension>();
                    if (extension != null)
                    {
                        offset.x *= extension.headScaleFactor.x;
                        offset.z /= extension.headScaleFactor.y > 1f ? extension.headScaleFactor.y : 1f; //Dont know exactly why but, z offset doesn't need to be adjusted at all for genes making someone smaller, only bigger
                    }
                }
            }
            return offset;
        }
    }
}
