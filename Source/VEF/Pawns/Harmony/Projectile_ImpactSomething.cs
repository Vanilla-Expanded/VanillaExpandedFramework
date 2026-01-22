using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using Verse.Noise;
using static HarmonyLib.Code;

namespace VEF.Pawns
{

    [HarmonyPatch(typeof(Projectile), "ImpactSomething")]
    public static class VanillaExpandedFramework_Projectile_ImpactSomething_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            var codes = codeInstructions.ToList();

            var addRangedDodge = AccessTools.Method(typeof(VanillaExpandedFramework_Projectile_ImpactSomething_Patch), "AddRangedDodge");
            var impact = AccessTools.Method(typeof(Projectile), "Impact");


            var label = ilg.DefineLabel();
            var foundStloc = false;

            for (var i = 0; i < codes.Count; i++)
            {

                if (!foundStloc && codes[i].opcode == OpCodes.Stloc_S && codes[i].operand is LocalBuilder { LocalIndex: 5 })
                {
                    // Patch only once
                    foundStloc = true;

                    // Load the "pawn" local
                    yield return CodeInstruction.LoadLocal(6);
                    // Call our method
                    yield return CodeInstruction.Call(() => GetHitChanceFactor);
                    // Multiply the current value
                    yield return new CodeInstruction(OpCodes.Mul);

                    // Return the original opcode
                    yield return codes[i];
                }
                else if (i > 1 && codes[i].opcode == OpCodes.Stloc_2 && codes[i -1].opcode == OpCodes.Isinst && codes[i - 1].OperandIs(typeof(Pawn))  )
                {
                    codes[i + 1].labels.Add(label);
                    yield return new CodeInstruction(OpCodes.Stloc_2);

                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Call, addRangedDodge);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, label);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldnull);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new CodeInstruction(OpCodes.Callvirt,impact);



                    yield return new CodeInstruction(OpCodes.Ret);
                }

                else yield return codes[i];
            }
        }


        public static bool AddRangedDodge(Pawn pawn)
        {
            if (pawn != null) {
                float rangedDodgeChance = pawn.GetStatValue(InternalDefOf.VEF_RangedDodgeChance);
                if (Rand.Chance(rangedDodgeChance))
                {
                    MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "TextMote_Dodge".Translate(), 1.9f);

                    return true;
                }
            }

            return false;
        }

        public static float GetHitChanceFactor(Pawn pawn)
        {
            // Get the ranged dodge chance as a factor chance to get hit
            return 1f - pawn.GetStatValue(InternalDefOf.VEF_RangedDodgeChance);
        }
    }
}