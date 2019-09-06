using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace VFECore
{

    public static class Patch_FactionGenerator
    {

        [HarmonyPatch(typeof(FactionGenerator), nameof(FactionGenerator.GenerateFactionsIntoWorld))]
        public static class GenerateFactionsIntoWorld
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                var allDefsInfo = AccessTools.Property(typeof(DefDatabase<FactionDef>), nameof(DefDatabase<FactionDef>.AllDefs)).GetGetMethod();

                var allowedFactionDefsInfo = AccessTools.Method(typeof(GenerateFactionsIntoWorld), nameof(AllowedFactionDefs));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    // Filter the calls to DefDatabase<FactionDef>.AllDefs to just include those allowed by the storyteller
                    if (instruction.opcode == OpCodes.Call && instruction.operand == allDefsInfo)
                    {
                        yield return instruction; // DefDatabase<FactionDef>.AllDefs
                        instruction = new CodeInstruction(OpCodes.Call, allowedFactionDefsInfo); // AllowedFactionDefs(DefDatabase<FactionDef>.AllDefs)
                    }

                    yield return instruction;
                }
            }

            private static IEnumerable<FactionDef> AllowedFactionDefs(IEnumerable<FactionDef> original)
            {
                return original.Where(f => CustomStorytellerUtility.FactionAllowed(f));
            }

        }

    }

}
