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
			if (ModCompatibilityCheck.ResearchTree)
			{
				Type typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly("FluffyResearchTree.ResearchNode", "FluffyResearchTree");
				if (typeInAnyAssembly != null)
				{
					Patch_FluffyResearchTree_ResearchNode.instanceType = typeInAnyAssembly;
					VanillaStorytellersExpanded.harmonyInstance.Patch(AccessTools.Property(typeInAnyAssembly, "Available").GetGetMethod(), null, new HarmonyMethod(typeof(Patch_FluffyResearchTree_ResearchNode.manual_get_Available), "Postfix", null), null, null);
					VanillaStorytellersExpanded.harmonyInstance.Patch(AccessTools.Method(typeInAnyAssembly, "Draw", null, null), null, null, new HarmonyMethod(typeof(Patch_FluffyResearchTree_ResearchNode.manual_Draw), "Transpiler", null), null);
				}
				else
				{
					Log.Error("Could not find type FluffyResearchTree.ResearchNode in Research Tree");
				}
			}
			if (ModCompatibilityCheck.ResearchPal)
			{
				Type typeInAnyAssembly2 = GenTypes.GetTypeInAnyAssembly("ResearchPal.ResearchNode", "ResearchPal");
				if (typeInAnyAssembly2 != null)
				{
					Patch_FluffyResearchTree_ResearchNode.instanceType = typeInAnyAssembly2;
					VanillaStorytellersExpanded.harmonyInstance.Patch(AccessTools.Property(typeInAnyAssembly2, "Available").GetGetMethod(), null, new HarmonyMethod(typeof(Patch_FluffyResearchTree_ResearchNode.manual_get_Available), "Postfix", null), null, null);
					VanillaStorytellersExpanded.harmonyInstance.Patch(AccessTools.Method(typeInAnyAssembly2, "Draw", null, null), null, null, new HarmonyMethod(typeof(Patch_FluffyResearchTree_ResearchNode.manual_Draw), "Transpiler", null), null);
				}
				else
				{
					Log.Error("Could not find type ResearchPal.ResearchNode in ResearchPal");
				}
			}
			if (ModCompatibilityCheck.RimCities)
			{
				Type typeInAnyAssembly3 = GenTypes.GetTypeInAnyAssembly("Cities.WorldGenStep_Cities", "Cities");
				if (typeInAnyAssembly3 != null)
				{
					VanillaStorytellersExpanded.harmonyInstance.Patch(AccessTools.Method(typeInAnyAssembly3, "GenerateFresh", null, null), new HarmonyMethod(typeof(Patch_Cities_WorldGenStep_Cities.manual_GenerateFresh), "Prefix", null), null, null, null);
				}
			}
		}
	}
}
