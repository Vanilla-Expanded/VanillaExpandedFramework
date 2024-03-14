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
                foreach (var code in instructions)
                {
                    yield return code;
                    if (code.Calls(allDefsInfo))
                    {
                        yield return new CodeInstruction(OpCodes.Call, allowedFactionDefsInfo); 
                    }
                }
            }
        
            private static IEnumerable<FactionDef> AllowedFactionDefs(IEnumerable<FactionDef> original) =>
                original?.Where(CustomStorytellerUtility.FactionAllowed);
        }
    }
}