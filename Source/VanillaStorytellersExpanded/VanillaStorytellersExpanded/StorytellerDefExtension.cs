using RimWorld;
using System;
using Verse;

namespace VanillaStorytellersExpanded
{
	// Token: 0x02000003 RID: 3
	public class StorytellerDefExtension : DefModExtension
	{
		// Token: 0x06000002 RID: 2 RVA: 0x00002059 File Offset: 0x00000259
		public static StorytellerDefExtension Get(Def def)
		{
			return def.GetModExtension<StorytellerDefExtension>() ?? StorytellerDefExtension.DefaultValues;
		}

		// Token: 0x04000001 RID: 1
		private static readonly StorytellerDefExtension DefaultValues = new StorytellerDefExtension();

		// Token: 0x04000002 RID: 2
		public TechLevelRange allowedTechLevels = TechLevelRange.All;

		public RaidRestlessness raidRestlessness;

	}
}
