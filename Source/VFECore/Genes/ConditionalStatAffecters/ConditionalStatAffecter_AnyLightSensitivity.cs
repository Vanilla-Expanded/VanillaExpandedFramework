
using RimWorld;
using Verse;
namespace VanillaGenesExpanded
{
    public class ConditionalStatAffecter_AnyLightSensitivity : ConditionalStatAffecter
    {
        public override string Label => "VGE_StatsReport_AnyLightSensitivity".Translate();

        public override bool Applies(StatRequest req)
        {
            if (!ModsConfig.BiotechActive)
            {
                return false;
            }
            if (req.HasThing && req.Thing.Spawned)
            {
                return req.Thing.Map.glowGrid.GroundGlowAt(req.Thing.Position)>=0.11f;
            }
            return false;
        }
    }
}
