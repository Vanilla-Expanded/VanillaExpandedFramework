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
        public static Pawn curPawn;
        public static void Prefix(DamageInfo dinfo, Pawn pawn)
        {
            curPawn = pawn;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            var field = AccessTools.Field(typeof(FleshTypeDef), "damageEffecter");
            foreach (var code in codes)
            {
                yield return code;
                if (code.LoadsField(field))
                {
           
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(VanillaGenesExpanded_DamageWorker_AddInjury_ApplyToPawn_Patch), nameof(GetEffecterDef)));
                }
            }
        }

        public static EffecterDef GetEffecterDef(EffecterDef effecterDef)
        {
            if (StaticCollectionsClass.bloodEffect_gene_pawns.ContainsKey(curPawn))
            {
                return StaticCollectionsClass.bloodEffect_gene_pawns[curPawn];
            }
            return effecterDef;
        }
        public static void Postfix()
        {
            curPawn = null;
        }
    }
}
