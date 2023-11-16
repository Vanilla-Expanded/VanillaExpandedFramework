
using RimWorld;
using Verse;
namespace VanillaGenesExpanded
{
    public class ConditionalStatAffecter_OutsideColonyMap : ConditionalStatAffecter
    {
        public override string Label => "VGE_StatsReport_OutsideColonyMap".Translate();

        public override bool Applies(StatRequest req)
        {
            if (!ModsConfig.BiotechActive)
            {
                return false;
            }
            if (req.HasThing && req.Thing.Spawned)
            {

                return req.Thing.Map?.IsPlayerHome!=true;
            }
            return false;
        }
    }
}
