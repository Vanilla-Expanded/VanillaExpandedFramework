using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(DamageWorker_AddInjury), "ApplyToPawn")]
    public static class VanillaGenesExpanded_DamageWorker_AddInjury_ApplyToPawn_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var damageEffecterField = AccessTools.Field(typeof(FleshTypeDef), nameof(FleshTypeDef.damageEffecter));
            var getEffecterDef = AccessTools.Method(typeof(VanillaGenesExpanded_DamageWorker_AddInjury_ApplyToPawn_Patch), nameof(GetEffecterDef));

            foreach (var codeInstruction in codeInstructions)
            {
                yield return codeInstruction;
                if (codeInstruction.opcode == OpCodes.Stloc_S 
                    && codeInstruction.operand is LocalBuilder lb && lb.LocalIndex == 11)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 11);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Call, getEffecterDef);
                    yield return new CodeInstruction(OpCodes.Stloc_S, 11);
                }
            }
        }

        public static EffecterDef GetEffecterDef(EffecterDef effecterDef, Pawn curPawn)
        {
            if (curPawn!=null && StaticCollectionsClass.bloodEffect_gene_pawns.ContainsKey(curPawn))
            {
                return StaticCollectionsClass.bloodEffect_gene_pawns[curPawn];
            }
            return effecterDef;
        }
    }
}
