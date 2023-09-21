
using RimWorld;
using Verse;
namespace VanillaGenesExpanded
{
    public class ConditionalStatAffecter_InPain : ConditionalStatAffecter
    {
        public override string Label => "VGE_StatsReport_InPain".Translate();

        public override bool Applies(StatRequest req)
        {
            if (!ModsConfig.BiotechActive)
            {
                return false;
            }
            if (req.HasThing && req.Thing.Spawned)
            {
                Pawn pawn = req.Thing as Pawn;
               
                return pawn.health.hediffSet.PainTotal > 0;
            }
            return false;
        }
    }
}
