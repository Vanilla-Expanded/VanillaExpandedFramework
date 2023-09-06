using HarmonyLib;
using Mono.Cecil.Cil;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(SkillRecord), "Interval")]
    public static class VanillaGenesExpanded_SkillRecord_Interval_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn ___pawn, SkillRecord __instance)
        {
            if (StaticCollectionsClass.noSkillLoss_gene_pawns.ContainsKey(___pawn) && StaticCollectionsClass.noSkillLoss_gene_pawns[___pawn] == __instance.def)
            {
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix(Pawn ___pawn, SkillRecord __instance)
        {
            if (StaticCollectionsClass.skillDegradation_gene_pawns.Contains(___pawn))
            {
                if (__instance.levelInt < 10)
                {
                    __instance.Learn(-0.1f);
                }
            }
            
        }
    }

    

    [HarmonyPatch]
    public static class VanillaGenesExpanded_SkillRecord_Interval_Transpiler_Patch
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(SkillRecord), nameof(SkillRecord.Interval));
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var pawn = AccessTools.Field(typeof(SkillRecord), "pawn");

            foreach (var instruction in instructions)
            {
                yield return instruction;
                if (instruction.opcode == OpCodes.Stloc_0)
                {

                
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, pawn);
                    yield return CodeInstruction.Call(typeof(VanillaGenesExpanded_SkillRecord_Interval_Transpiler_Patch), nameof(VanillaGenesExpanded_SkillRecord_Interval_Transpiler_Patch.GetMultiplier));
                    yield return new CodeInstruction(OpCodes.Mul);
                    yield return new CodeInstruction(OpCodes.Stloc_0);
                }
            }
        }

        public static float GetMultiplier(Pawn pawn)
        {
            if (StaticCollectionsClass.skillLossMultiplier_gene_pawns.ContainsKey(pawn))
            {
                return StaticCollectionsClass.skillLossMultiplier_gene_pawns[pawn];
            }
            else return 1f;
        }


    }


}
