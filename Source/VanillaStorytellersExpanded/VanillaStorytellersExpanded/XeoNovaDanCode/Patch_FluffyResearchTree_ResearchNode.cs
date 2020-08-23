using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace VanillaStorytellersExpanded
{
	// Token: 0x02000014 RID: 20
	public static class Patch_FluffyResearchTree_ResearchNode
	{
		// Token: 0x0400001F RID: 31
		public static Type instanceType;

		// Token: 0x02000015 RID: 21
		public static class manual_get_Available
		{
			// Token: 0x06000029 RID: 41 RVA: 0x00002894 File Offset: 0x00000A94
			public static void Postfix(ResearchProjectDef ___Research, ref bool __result)
			{
				bool flag = __result && !CustomStorytellerUtility.TechLevelAllowed(___Research.techLevel);
				if (flag)
				{
					__result = false;
				}
			}
		}

		// Token: 0x02000016 RID: 22
		public static class manual_Draw
		{
			// Token: 0x0600002A RID: 42 RVA: 0x00002156 File Offset: 0x00000356
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				//Log.Message("FluffyResearchTree.ResearchNode.manual_Draw transpiler start (1 match todo)", false);
				List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
				MethodInfo buildingPresentInfo = AccessTools.Method(Patch_FluffyResearchTree_ResearchNode.instanceType, "BuildingPresent", null, null);
				FieldInfo ResearchInfo = AccessTools.Field(Patch_FluffyResearchTree_ResearchNode.instanceType, "Research");
				MethodInfo canQueueResearchesInfo = AccessTools.Method(typeof(Patch_FluffyResearchTree_ResearchNode.manual_Draw), "CanQueueResearches", null, null);
				int buildingPresentCallCount = instructionList.Count((CodeInstruction i) => i.opcode == OpCodes.Call && i.OperandIs(buildingPresentInfo));
				int buildingPresentCalls = 0;
				int num;
				for (int j = 0; j < instructionList.Count; j = num + 1)
				{
					CodeInstruction instruction = instructionList[j];
					bool flag = instruction.opcode == OpCodes.Call && instruction.OperandIs(buildingPresentInfo);
					if (flag)
					{
						//Log.Message("FluffyResearchTree.ResearchNode.manual_Draw match 1 of 1", false);
						num = buildingPresentCalls;
						buildingPresentCalls = num + 1;
						bool flag2 = buildingPresentCalls == buildingPresentCallCount;
						if (flag2)
						{
							//Log.Message("FluffyResearchTree.ResearchNode.manual_Draw finalise", false);
							yield return instruction;
							yield return new CodeInstruction(OpCodes.Ldarg_0, null);
							yield return new CodeInstruction(OpCodes.Ldfld, ResearchInfo);
							instruction = new CodeInstruction(OpCodes.Call, canQueueResearchesInfo);
						}
					}
					yield return instruction;
					instruction = null;
					num = j;
				}
				yield break;
			}

			// Token: 0x0600002B RID: 43 RVA: 0x000028C0 File Offset: 0x00000AC0
			private static bool CanQueueResearches(bool original, ResearchProjectDef research)
			{
				return original && CustomStorytellerUtility.TechLevelAllowed(research.techLevel);
			}
		}
	}
}
