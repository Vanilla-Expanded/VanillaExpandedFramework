using HarmonyLib;
using System.Collections.Generic;
using Verse;
using System.Reflection.Emit;
using System.Reflection;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "DropBloodSmear")]
    public static class VanillaGenesExpanded_Pawn_HealthTracker_DropBloodSmear_Patch
    {
        public static MethodInfo TryChangeBloodSmearInfo = AccessTools.Method(typeof(VanillaGenesExpanded_Pawn_HealthTracker_DropBloodSmear_Patch),
            nameof(TryChangeBloodSmear));

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            foreach (var code in codeInstructions)
            {
                yield return code;
                if (code.opcode == OpCodes.Stloc_0)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_HealthTracker), "pawn"));
                    yield return new CodeInstruction(OpCodes.Call, TryChangeBloodSmearInfo);
                    yield return new CodeInstruction(OpCodes.Stloc_0);
                }
            }
        }

        public static ThingDef TryChangeBloodSmear(ThingDef thingDef, Pawn pawn)
        {
            if (StaticCollectionsClass.bloodsmear_gene_pawns.TryGetValue(pawn, out var customBlood))
            {
                return customBlood;
            }
            return thingDef;
        }
    }
}
