
using RimWorld;
using Verse;
namespace VEF.Genes
{
    public class ConditionalStatAffecter_Day : ConditionalStatAffecter
    {
        public override string Label => "VGE_StatsReport_Day".Translate();

        public override bool Applies(StatRequest req)
        {

            if (req.HasThing && req.Thing.Spawned)
            {
                return GenLocalDate.HourInteger(req.Thing) >= 11 && GenLocalDate.HourInteger(req.Thing) <= 17;
            }
            return false;
        }
    }
}
