using RimWorld;
using Verse;
using System.Collections.Generic;

namespace VEF.Genes

{
	public class ConditionalStatAffecter_InVacuum : ConditionalStatAffecter
	{
		public override string Label => "VGE_StatsReport_InVacuum".Translate();

		public override bool Applies(StatRequest req)
		{
			if (!ModsConfig.OdysseyActive)
			{
				return false;
			}
			Pawn pawn;
			
			if (req.HasThing && (pawn = req.Thing as Pawn) != null && pawn.Map != null)
			{

                if (pawn.Position.GetVacuum(pawn.Map)>0)
                {
					return true;
                }


			}
			return false;



		}
	}
}