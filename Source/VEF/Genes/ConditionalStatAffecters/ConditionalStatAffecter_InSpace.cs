using RimWorld;
using Verse;
using System.Collections.Generic;

namespace VEF.Genes

{
	public class ConditionalStatAffecter_InSpace : ConditionalStatAffecter
	{
		public override string Label => "VGE_StatsReport_InSpace".Translate();

		public override bool Applies(StatRequest req)
		{
			if (!ModsConfig.OdysseyActive)
			{
				return false;
			}
			Pawn pawn;
			
			if (req.HasThing && (pawn = req.Thing as Pawn) != null)
			{

                if (pawn.Map?.BiomeAt(pawn.Position)?.inVacuum==true)
                {
					return true;
                }


			}
			return false;



		}
	}
}