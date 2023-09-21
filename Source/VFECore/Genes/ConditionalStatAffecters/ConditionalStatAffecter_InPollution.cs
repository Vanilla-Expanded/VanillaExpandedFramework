
using RimWorld;
using Verse;
namespace VanillaGenesExpanded
{
    public class ConditionalStatAffecter_InPollution : ConditionalStatAffecter
    {
        public override string Label => "VGE_StatsReport_InPollution".Translate();

        public override bool Applies(StatRequest req)
        {
            if (!ModsConfig.BiotechActive)
            {
                return false;
            }
            if (req.HasThing && req.Thing.Spawned)
            {
                
                return req.Thing.Position.IsPolluted(req.Thing.Map);
            }
            return false;
        }
    }
}
