using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;

namespace VEF.Genes
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "DropBloodFilth")]
    public static class VanillaExpandedFramework_Pawn_HealthTracker_DropBloodFilth_Patch
    {
        public static MethodInfo TryChangeBloodFilthInfo = AccessTools.Method(typeof(VanillaExpandedFramework_Pawn_HealthTracker_DropBloodFilth_Patch),
            nameof(TryChangeBloodFilth));

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            foreach (var code in codeInstructions)
            {
                yield return code;
                if (code.opcode == OpCodes.Stloc_1)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_HealthTracker), "pawn"));
                    yield return new CodeInstruction(OpCodes.Call, TryChangeBloodFilthInfo);
                    yield return new CodeInstruction(OpCodes.Stloc_1);
                }
            }
        }

        public static ThingDef TryChangeBloodFilth(ThingDef thingDef, Pawn pawn) 
        {
            if (StaticCollectionsClass.bloodtype_gene_pawns.TryGetValue(pawn, out var customBlood))
            {
                return customBlood;
            }
            return thingDef;
        }
    }
}
