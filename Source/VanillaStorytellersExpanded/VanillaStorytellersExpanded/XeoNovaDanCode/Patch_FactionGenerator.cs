using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaStorytellersExpanded
{
    public static class Patch_FactionGenerator
    {
        [HarmonyPatch(typeof(FactionGenerator), "GenerateFactionsIntoWorld")]
        public static class GenerateFactionsIntoWorld
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var allDefsInfo = AccessTools.PropertyGetter(typeof(DefDatabase<FactionDef>), "AllDefs");
                var allowedFactionDefsInfo = AccessTools.Method(typeof(GenerateFactionsIntoWorld), nameof(AllowedFactionDefs));
                var factionAllowed = AccessTools.Method(typeof(CustomStorytellerUtility), nameof(CustomStorytellerUtility.FactionAllowed));
                var codes = instructions.ToList();
                var idx1 = codes.FindIndex(ins => ins.Calls(allDefsInfo));
                codes.Insert(idx1 + 1, new CodeInstruction(OpCodes.Call, allowedFactionDefsInfo));
                var label = (Label)codes.Find(ins => ins.opcode == OpCodes.Br_S).operand;
                var idx2 = codes.FindIndex(ins => ins.opcode == OpCodes.Stloc_2);
                codes.InsertRange(idx2 + 1, new[]
                {
                    new CodeInstruction(OpCodes.Ldloc_2),
                    new CodeInstruction(OpCodes.Call, factionAllowed),
                    new CodeInstruction(OpCodes.Brfalse, label)
                });
                return codes;
            }

            private static IEnumerable<FactionDef> AllowedFactionDefs(IEnumerable<FactionDef> original) =>
                original?.Where(CustomStorytellerUtility.FactionAllowed);
        }
    }
}