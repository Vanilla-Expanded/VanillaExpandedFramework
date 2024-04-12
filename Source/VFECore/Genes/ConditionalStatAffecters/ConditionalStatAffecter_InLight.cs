
using RimWorld;
using Verse;
namespace VanillaGenesExpanded
{
    public class ConditionalStatAffecter_InLight : ConditionalStatAffecter
    {
        public override string Label => "VGE_StatsReport_InLight".Translate();

        public override bool Applies(StatRequest req)
        {
            if (!ModsConfig.BiotechActive)
            {
                return false;
            }
            if (req.HasThing && req.Thing.Spawned)
            {
                return req.Thing.Map.glowGrid.GroundGlowAt(req.Thing.Position)>=0.5f;
            }
            return false;
        }
    }
}
