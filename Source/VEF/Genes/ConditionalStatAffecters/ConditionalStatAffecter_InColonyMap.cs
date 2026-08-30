
using RimWorld;
using Verse;
namespace VEF.Genes
{
    public class ConditionalStatAffecter_InColonyMap : ConditionalStatAffecter
    {
        public override string Label => "VGE_StatsReport_ColonyMap".Translate();

        public override bool Applies(StatRequest req)
        {
            
            if (req.HasThing && req.Thing.Spawned)
            {

                return req.Thing.Map?.IsPlayerHome==true;
            }
            return false;
        }
    }
}
