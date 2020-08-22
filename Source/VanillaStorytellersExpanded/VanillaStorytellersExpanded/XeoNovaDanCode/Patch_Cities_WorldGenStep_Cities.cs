using System;

namespace VanillaStorytellersExpanded
{
	// Token: 0x02000019 RID: 25
	public static class Patch_Cities_WorldGenStep_Cities
	{
		// Token: 0x0200001A RID: 26
		public static class manual_GenerateFresh
		{
			// Token: 0x06000036 RID: 54 RVA: 0x00002B7C File Offset: 0x00000D7C
			public static bool Prefix()
			{
				return NonPublicMethods.RimCities.GenCity_RandomCityFaction(null) != null;
			}
		}
	}
}
