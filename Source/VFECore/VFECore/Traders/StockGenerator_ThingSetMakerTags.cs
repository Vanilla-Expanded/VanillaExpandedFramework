using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VFECore
{
	public class StockGenerator_ThingSetMakerTags : StockGenerator
	{
		[NoTranslate]
		public List<string> thingSetMakerTags;
		private IntRange thingDefCountRange = IntRange.one;

		private List<ThingDef> excludedThingDefs;

		public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
		{
			List<ThingDef> generatedDefs = new List<ThingDef>();
			int numThingDefsToUse = thingDefCountRange.RandomInRange;
			var list = DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => HandlesThingDef(d) && d.tradeability.TraderCanSell());
			for (int i = 0; i < numThingDefsToUse; i++)
			{
				if (!list.Where(d => (excludedThingDefs == null || !excludedThingDefs.Contains(d)) && !generatedDefs.Contains(d)).TryRandomElement(out ThingDef chosenThingDef))
				{
					break;
				}
				foreach (Thing item in StockGeneratorUtility.TryMakeForStock(chosenThingDef, RandomCountOf(chosenThingDef),null))
				{
					yield return item;
				}
				generatedDefs.Add(chosenThingDef);
				chosenThingDef = null;
			}
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			if (thingDef.tradeability != 0 && (int)thingDef.techLevel <= (int)maxTechLevelBuy && thingDef.thingSetMakerTags != null)
			{
				foreach (var tag in thingSetMakerTags)
                {
					if (thingDef.thingSetMakerTags.Contains(tag))
					{
						return true;
                    }
                }
			}
			return false;
		}
	}
}
