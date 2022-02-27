using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System;
using Verse.AI;
using RimWorld.Planet;
using System.Reflection.Emit;
using System.Linq;

namespace AnimalBehaviours
{
    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddDraftedOrders")]
    public static class FloatMenuMakerMap_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            var shouldSkip = AccessTools.Method(typeof(AnimalCollectionClass), nameof(AnimalCollectionClass.IsDraftableControllableAnimal));
            var codes = instructions.ToList();
            var pawnField = AccessTools.Field(typeof(FloatMenuMakerMap).GetNestedTypes(AccessTools.all).First(c => c.Name.Contains("c__DisplayClass8_0")), "pawn");
            var skillsField = AccessTools.Field(typeof(Pawn), "skills");
            var constructionDefField = AccessTools.Field(typeof(SkillDefOf), "Construction");
            bool patched = false;
            for (var i = 0; i < codes.Count; i++)
            {
                var instr = codes[i];
                if (!patched && codes[i].opcode == OpCodes.Ldloc_0 && codes[i + 1].LoadsField(pawnField) && codes[i + 2].LoadsField(skillsField)
                    && codes[i + 3].LoadsField(constructionDefField))
                {
                    patched = true;
                    yield return new CodeInstruction(OpCodes.Ldloc_0).MoveLabelsFrom(codes[i]);
                    yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
                    yield return new CodeInstruction(OpCodes.Call, shouldSkip);
                    yield return new CodeInstruction(OpCodes.Brtrue_S, codes[i + 6].operand);
                }
                yield return instr;
            }

            if (!patched)
            {
                Log.Error("FloatMenuMakerMap:AddDraftedOrders Transpiler failed");
            }
        }
    }
}
