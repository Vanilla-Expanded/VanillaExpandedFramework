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

            return new CodeMatcher(codeInstructions)
                .SearchForward(instr => instr.LoadsField(damageEffecterField))
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_2), // pawn
                    new CodeInstruction(OpCodes.Call, getEffecterDef)
                )
                .InstructionEnumeration();
        }

        public static EffecterDef GetEffecterDef(EffecterDef effecterDef, Pawn curPawn)
        {
            if (StaticCollectionsClass.bloodEffect_gene_pawns.ContainsKey(curPawn))
            {
                return StaticCollectionsClass.bloodEffect_gene_pawns[curPawn];
            }
            return effecterDef;
        }
    }
}
