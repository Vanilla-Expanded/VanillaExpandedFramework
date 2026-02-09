using RimWorld;
using Verse;

namespace PipeSystem
{
    public class CompHeatPusherPowered_Overclocked : CompHeatPusherPowered
    {
        public const float IdleFrac = 0.25f;
        public CompAdvancedResourceProcessor cachedAdvancedProcessor;

        public CompAdvancedResourceProcessor AdvancedProcessor
        {
            get
            {
                if (cachedAdvancedProcessor == null)
                {
                    cachedAdvancedProcessor = this.parent.GetComp<CompAdvancedResourceProcessor>();
                }
                return cachedAdvancedProcessor;
            }
        }

      

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (ShouldPushHeatNow)
            {
                float overclock = AdvancedProcessor.overclockMultiplier;
                 
                GenTemperature.PushHeat(parent.PositionHeld, parent.MapHeld, Props.heatPerSecond * 4.16666651f * (IdleFrac + (1 - IdleFrac) * overclock * overclock));
            }
        }

      

     
    }
}