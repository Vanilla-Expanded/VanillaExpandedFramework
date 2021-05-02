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
	// Token: 0x0200000A RID: 10
	public static class Patch_FactionGenerator
	{
		// Token: 0x0200000B RID: 11
		[HarmonyPatch(typeof(FactionGenerator), "GenerateFactionsIntoWorld")]
		public static class GenerateFactionsIntoWorld
		{
			// Token: 0x0600000E RID: 14 RVA: 0x000020AA File Offset: 0x000002AA
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
				MethodInfo allDefsInfo = AccessTools.Property(typeof(DefDatabase<FactionDef>), "AllDefs").GetGetMethod();
				MethodInfo allowedFactionDefsInfo = AccessTools.Method(typeof(Patch_FactionGenerator.GenerateFactionsIntoWorld), "AllowedFactionDefs", null, null);
				int num;
				for (int i = 0; i < instructionList.Count; i = num + 1)
				{
					CodeInstruction instruction = instructionList[i];
					bool flag = instruction.opcode == OpCodes.Call && instruction.OperandIs(allDefsInfo);
					if (flag)
					{
						yield return instruction;
						instruction = new CodeInstruction(OpCodes.Call, allowedFactionDefsInfo);
					}
					yield return instruction;
					instruction = null;
					num = i;
				}
				yield break;
			}

			// Token: 0x0600000F RID: 15 RVA: 0x000024A8 File Offset: 0x000006A8
			private static IEnumerable<FactionDef> AllowedFactionDefs(IEnumerable<FactionDef> original)
			{
				return from f in original
				where CustomStorytellerUtility.FactionAllowed(f)
				select f;
			}
		}
	}
}
