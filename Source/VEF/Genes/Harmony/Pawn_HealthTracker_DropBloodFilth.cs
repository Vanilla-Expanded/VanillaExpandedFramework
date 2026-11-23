using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace VEF.Genes
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "DropBloodFilth")]
    public static class VanillaExpandedFramework_Pawn_HealthTracker_DropBloodFilth_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = new List<CodeInstruction>(codeInstructions);

            var tryChangeBloodFilthInfo = AccessTools.Method(typeof(VanillaExpandedFramework_Pawn_HealthTracker_DropBloodFilth_Patch),nameof(TryChangeBloodFilth));

            var pawnField = AccessTools.Field(typeof(Pawn_HealthTracker), "pawn");

            bool found = false;

            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];

                if (!found && codes[i].opcode == OpCodes.Stloc_0)
                {
                    found = true;

                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
                    yield return new CodeInstruction(OpCodes.Call, tryChangeBloodFilthInfo);
                    yield return new CodeInstruction(OpCodes.Stloc_0);
                }
            }

            if (!found)
            {
                Log.Error("[VEF] Failed to transpile Pawn_HealthTracker.DropBloodFilth: could not find stloc.0.");
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