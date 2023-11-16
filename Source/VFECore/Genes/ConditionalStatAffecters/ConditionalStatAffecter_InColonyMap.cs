
using RimWorld;
using Verse;
namespace VanillaGenesExpanded
{
    public class ConditionalStatAffecter_InColonyMap : ConditionalStatAffecter
    {
        public override string Label => "VGE_StatsReport_ColonyMap".Translate();

        public override bool Applies(StatRequest req)
        {
            if (!ModsConfig.BiotechActive)
            {
                return false;
            }
            if (req.HasThing && req.Thing.Spawned)
            {

                return req.Thing.Map?.IsPlayerHome==true;
            }
            return false;
        }
    }
}
