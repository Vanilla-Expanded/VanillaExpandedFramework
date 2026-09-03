
using RimWorld;
using Verse;
namespace VEF.Genes
{
    public class ConditionalStatAffecter_InGreatPain : ConditionalStatAffecter
    {
        public override string Label => "VGE_StatsReport_InGreatPain".Translate();

        public override bool Applies(StatRequest req)
        {
            
            if (req.HasThing && req.Thing.Spawned)
            {
                Pawn pawn = req.Thing as Pawn;
               
                return pawn.health.hediffSet.PainTotal > 0.2f;
            }
            return false;
        }
    }
}
