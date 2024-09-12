
using RimWorld;
using Verse;

namespace VanillaGenesExpanded
{
	public class ConditionalStatAffecter_OverFortyDegrees : ConditionalStatAffecter
	{
		public override string Label => "VGE_StatsReport_Over40".Translate();

		public override bool Applies(StatRequest req)
		{
			if (!ModsConfig.BiotechActive)
			{
				return false;
			}
			Pawn pawn;
			if (req.HasThing && (pawn = (req.Thing as Pawn)) != null && pawn.Map != null && pawn.Map.regionAndRoomUpdater?.Enabled == true && pawn.Position.GetTemperature(pawn.Map) > 40)
			{
				return true;
			}
			return false;
		}
	}
}
