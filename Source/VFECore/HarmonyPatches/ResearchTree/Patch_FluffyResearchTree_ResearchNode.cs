using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace VFECore
{

    public static class Patch_FluffyResearchTree_ResearchNode
    {

        public static Type instanceType;

        public static class manual_get_Available
        {

            public static void Postfix(ResearchProjectDef ___Research, ref bool __result)
            {
                if (__result && !CustomStorytellerUtility.TechLevelAllowed(___Research.techLevel))
                {
                    __result = false;
                }
            }

        }

        public static class manual_Draw
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                var buildingPresentInfo = AccessTools.Method(instanceType, "BuildingPresent");
                var ResearchInfo = AccessTools.Field(instanceType, "Research");

                var canQueueResearchesInfo = AccessTools.Method(typeof(manual_Draw), nameof(CanQueueResearches));

                var buildingPresentCallCount = instructionList.Count(i => i.opcode == OpCodes.Call && i.operand == buildingPresentInfo);
                int buildingPresentCalls = 0;

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    // Also make sure that the storyteller's tech level range allows for the research project to be carried out - this is done after the last call for BuildingPresent
                    if (instruction.opcode == OpCodes.Call && instruction.operand == buildingPresentInfo)
                    {
                        buildingPresentCalls++;
                        if (buildingPresentCalls == buildingPresentCallCount)
                        {
                            yield return instruction; // this.BuildingPresent()
                            yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                            yield return new CodeInstruction(OpCodes.Ldfld, ResearchInfo); // this.Research
                            instruction = new CodeInstruction(OpCodes.Call, canQueueResearchesInfo); // CanQueueResearches(this.BuildingPresent(), this.Research)
                        }
                    }

                    yield return instruction;
                }
            }

            private static bool CanQueueResearches(bool original, ResearchProjectDef research)
            {
                return original && CustomStorytellerUtility.TechLevelAllowed(research.techLevel);
            }

        }

    }

}
