using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFECore
{

    public static class Patch_MainTabWindow_Research
    {

        [HarmonyPatch(typeof(MainTabWindow_Research), "DrawRightRect")]
        public static class DrawRightRect
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                #if DEBUG
                    Log.Message("MainTabWindow_Research.DrawRightRect transpiler start (1 match todo)");
                #endif

                var instructionList = instructions.ToList();

                var getAllDefsListForReadingInfo = AccessTools.Property(typeof(DefDatabase<ResearchProjectDef>), nameof(DefDatabase<ResearchProjectDef>.AllDefsListForReading)).GetGetMethod();

                var allowedResearchProjectsInfo = AccessTools.Method(typeof(DrawRightRect), nameof(AllowedResearchProjects));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    if (instruction.opcode == OpCodes.Call && instruction.OperandIs(getAllDefsListForReadingInfo))
                    {
                        #if DEBUG
                            Log.Message("MainTabWindow_Research.DrawRightRect match 1 of 1");
                        #endif
                        yield return instruction; // DefDatabase<ResearchProjectDef>.AllDefsListForReading
                        instruction = new CodeInstruction(OpCodes.Call, allowedResearchProjectsInfo); // AllowedResearchProjects(DefDatabase<ResearchProjectDef>.AllDefsListForReading)
                    }

                    yield return instruction;
                }
            }

            private static List<ResearchProjectDef> AllowedResearchProjects(List<ResearchProjectDef> originalList)
            {
                return originalList.Where(r => CustomStorytellerUtility.TechLevelAllowed(r.techLevel)).ToList();
            }

        }

    }

}
