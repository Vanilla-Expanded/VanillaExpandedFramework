using RimWorld;
using Verse;
using System.Collections.Generic;

namespace VEF.Genes

{
	public class ConditionalStatAffecter_NotInSpace : ConditionalStatAffecter
	{
		public override string Label => "VGE_StatsReport_InPlanet".Translate();

		public override bool Applies(StatRequest req)
		{
			if (!ModsConfig.OdysseyActive)
			{
				return false;
			}
			Pawn pawn;

            if (req.HasThing && (pawn = req.Thing as Pawn) != null)
            {

                if (pawn.Position!= IntVec3.Invalid&& pawn.Map?.BiomeAt(pawn.Position)?.inVacuum == false)
                {
                    return true;
                }


            }
            return false;



		}
	}
}