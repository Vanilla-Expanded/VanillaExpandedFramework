
using RimWorld;
using Verse;
namespace VanillaGenesExpanded
{
    public class ConditionalStatAffecter_Outdoors : ConditionalStatAffecter
    {
        public override string Label => "VGE_StatsReport_Outside".Translate();

        public override bool Applies(StatRequest req)
        {
            if (!ModsConfig.BiotechActive)
            {
                return false;
            }
            if (req.HasThing && req.Thing.Spawned)
            {
                return !req.Thing.Map.roofGrid.Roofed(req.Thing.Position);
            }
            return false;
        }
    }
}
