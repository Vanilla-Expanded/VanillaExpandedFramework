using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaStorytellersExpanded
{
	// Token: 0x02000023 RID: 35
	public static class CustomStorytellerUtility
	{
		// Token: 0x0600004E RID: 78 RVA: 0x000031BC File Offset: 0x000013BC
		public static bool FactionAllowed(FactionDef def)
		{
			return def.isPlayer || def.hidden || CustomStorytellerUtility.TechLevelAllowed(def.techLevel);
		}

		// Token: 0x0600004F RID: 79 RVA: 0x000031EC File Offset: 0x000013EC
		public static bool TechLevelAllowed(TechLevel level)
		{
			Storyteller storyteller = Find.Storyteller;
			bool flag = storyteller != null;
			return !flag || StorytellerDefExtension.Get(storyteller.def).allowedTechLevels.Includes(level);
		}

		// Token: 0x06000050 RID: 80 RVA: 0x00003228 File Offset: 0x00001428
		public static IEnumerable<ResearchProjectDef> AllowedResearchProjectDefs()
		{
			return from r in DefDatabase<ResearchProjectDef>.AllDefsListForReading
			where CustomStorytellerUtility.TechLevelAllowed(r.techLevel)
			select r;
		}

		// Token: 0x06000051 RID: 81 RVA: 0x00003264 File Offset: 0x00001464
		public static bool TryGetRandomUnfinishedResearchProject(out ResearchProjectDef research)
		{
			return (from r in DefDatabase<ResearchProjectDef>.AllDefsListForReading
			where !r.IsFinished
			select r).TryRandomElementByWeight((ResearchProjectDef r) => Mathf.Pow(1f / (float)(r.techLevel + 1), 2f), out research);
		}
	}
}
