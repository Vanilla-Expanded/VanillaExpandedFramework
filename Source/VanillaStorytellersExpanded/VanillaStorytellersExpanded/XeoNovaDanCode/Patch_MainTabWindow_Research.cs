using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaStorytellersExpanded
{
	// Token: 0x0200000E RID: 14
	public static class Patch_MainTabWindow_Research
	{
		// Token: 0x0200000F RID: 15
		[HarmonyPatch(typeof(MainTabWindow_Research), "DrawRightRect")]
		public static class DrawRightRect
		{
			// Token: 0x0600001B RID: 27 RVA: 0x00002102 File Offset: 0x00000302
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				//Log.Message("MainTabWindow_Research.DrawRightRect transpiler start (1 match todo)", false);
				List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
				MethodInfo getAllDefsListForReadingInfo = AccessTools.Property(typeof(DefDatabase<ResearchProjectDef>), "AllDefsListForReading").GetGetMethod();
				MethodInfo allowedResearchProjectsInfo = AccessTools.Method(typeof(Patch_MainTabWindow_Research.DrawRightRect), "AllowedResearchProjects", null, null);
				int num;
				for (int i = 0; i < instructionList.Count; i = num + 1)
				{
					CodeInstruction instruction = instructionList[i];
					bool flag = instruction.opcode == OpCodes.Call && instruction.OperandIs(getAllDefsListForReadingInfo);
					if (flag)
					{
						//Log.Message("MainTabWindow_Research.DrawRightRect match 1 of 1", false);
						yield return instruction;
						instruction = new CodeInstruction(OpCodes.Call, allowedResearchProjectsInfo);
					}
					yield return instruction;
					instruction = null;
					num = i;
				}
				yield break;
			}

			// Token: 0x0600001C RID: 28 RVA: 0x0000269C File Offset: 0x0000089C
			private static List<ResearchProjectDef> AllowedResearchProjects(List<ResearchProjectDef> originalList)
			{
				return (from r in originalList
				where CustomStorytellerUtility.TechLevelAllowed(r.techLevel)
				select r).ToList<ResearchProjectDef>();
			}
		}
	}
}
