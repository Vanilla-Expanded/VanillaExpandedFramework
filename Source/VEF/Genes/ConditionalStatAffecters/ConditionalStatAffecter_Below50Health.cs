using RimWorld;
using Verse;

namespace VEF.Genes

{
    public class ConditionalStatAffecter_Below50Health : ConditionalStatAffecter
    {
        public override string Label => "VGE_StatsReport_Below50Health".Translate();

        public override bool Applies(StatRequest req)
        {

            Pawn pawn;
            bool flag = false;
            if (req.HasThing && (pawn = req.Thing as Pawn) != null)
            {
                if (pawn.health.summaryHealth.SummaryHealthPercent < 0.5f)
                {
                    return true;
                }
            }
            return false;
        }
    }
}