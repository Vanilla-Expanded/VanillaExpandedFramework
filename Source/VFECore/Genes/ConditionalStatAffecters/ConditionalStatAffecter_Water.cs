using RimWorld;
using Verse;
using System.Collections.Generic;

namespace VanillaGenesExpanded

{
	public class ConditionalStatAffecter_Water : ConditionalStatAffecter
	{
		public override string Label => "VGE_StatsReport_Water".Translate();

		public override bool Applies(StatRequest req)
		{
			if (!ModsConfig.BiotechActive)
			{
				return false;
			}
			Pawn pawn;
			
			if (req.HasThing && (pawn = req.Thing as Pawn) != null && pawn.Map != null)
			{

                if (pawn.Position.GetTerrain(pawn.Map).IsWater)
                {
					return true;
                }


			}
			return false;



		}
	}
}