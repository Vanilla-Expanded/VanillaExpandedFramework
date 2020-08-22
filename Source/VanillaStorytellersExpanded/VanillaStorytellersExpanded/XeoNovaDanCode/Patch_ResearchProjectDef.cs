using System;
using HarmonyLib;
using Verse;

namespace VanillaStorytellersExpanded
{
	// Token: 0x02000012 RID: 18
	public static class Patch_ResearchProjectDef
	{
		// Token: 0x02000013 RID: 19
		[HarmonyPatch(typeof(ResearchProjectDef), "CanStartNow", MethodType.Getter)]
		public static class get_CanStartNow
		{
			// Token: 0x06000028 RID: 40 RVA: 0x00002894 File Offset: 0x00000A94
			public static void Postfix(ResearchProjectDef __instance, ref bool __result)
			{
				bool flag = __result && !CustomStorytellerUtility.TechLevelAllowed(__instance.techLevel);
				if (flag)
				{
					__result = false;
				}
			}
		}
	}
}
