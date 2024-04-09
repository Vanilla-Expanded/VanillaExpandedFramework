using System;
using HarmonyLib;
using Verse;

namespace VanillaStorytellersExpanded
{
	// Token: 0x02000004 RID: 4
	[StaticConstructorOnStartup]
	public static class HarmonyPatches
	{
		// Token: 0x06000005 RID: 5 RVA: 0x0000224C File Offset: 0x0000044C
		static HarmonyPatches()
		{
			VanillaStorytellersExpanded.harmonyInstance.PatchAll();
		}
	}
}
