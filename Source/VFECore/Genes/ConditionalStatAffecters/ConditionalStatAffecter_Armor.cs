using RimWorld;
using Verse;
using System.Collections.Generic;

namespace VanillaGenesExpanded

{
	public class ConditionalStatAffecter_Armor : ConditionalStatAffecter
	{
		public override string Label => "VGE_StatsReport_Armoured".Translate();

		public override bool Applies(StatRequest req)
		{
			if (!ModsConfig.BiotechActive)
			{
				return false;
			}
			Pawn pawn;
			bool flag = false;
			if (req.HasThing && (pawn = req.Thing as Pawn) != null && pawn.apparel != null)
			{

				List<Apparel> wornApparel = pawn.apparel.WornApparel;
				for (int i = 0; i < wornApparel.Count; i++)
				{
					if (wornApparel[i].Stuff?.stuffProps?.categories?.Contains(StuffCategoryDefOf.Metallic) == true || wornApparel[i].def?.thingCategories?.Contains(ThingCategoryDefOf.ApparelArmor) == true)
					{
						flag = true;
					}

					if (wornApparel[i].def?.thingSetMakerTags?.Contains("Warcasket") ?? false)
					{
						flag = true;
					}

				}

				if (flag)
				{

					return true;
				}



			}
			return false;



		}
	}
}