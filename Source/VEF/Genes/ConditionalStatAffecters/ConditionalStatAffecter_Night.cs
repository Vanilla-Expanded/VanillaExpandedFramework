
using RimWorld;
using Verse;
namespace VEF.Genes
{
    public class ConditionalStatAffecter_Night : ConditionalStatAffecter
    {
        public override string Label => "VGE_StatsReport_Night".Translate();

        public override bool Applies(StatRequest req)
        {

            if (req.HasThing && req.Thing.Spawned)
            {
                return (GenLocalDate.HourInteger(req.Thing) >= 23 || GenLocalDate.HourInteger(req.Thing) <= 5);
            }
            return false;
        }
    }
}
