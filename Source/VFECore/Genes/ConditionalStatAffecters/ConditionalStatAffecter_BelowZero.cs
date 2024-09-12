
using RimWorld;
using Verse;

namespace VanillaGenesExpanded
{
	public class ConditionalStatAffecter_BelowZero : ConditionalStatAffecter
	{
		public override string Label => "VGE_StatsReport_Below0".Translate();

		public override bool Applies(StatRequest req)
		{
			if (!ModsConfig.BiotechActive)
			{
				return false;
			}
			Pawn pawn;
			if (req.HasThing && (pawn = (req.Thing as Pawn)) != null && pawn.Map != null && pawn.Map.regionAndRoomUpdater?.Enabled ==true && pawn.Position.GetTemperature(pawn.Map) <0)
			{
				return true;
			}
			return false;
		}
	}
}
