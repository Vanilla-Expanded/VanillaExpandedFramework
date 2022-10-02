
using Verse;
using RimWorld;

namespace AnimalBehaviours
{
    class HediffComp_HeatPusher : HediffComp
    {

       

        public HediffCompProperties_HeatPusher Props
        {
            get
            {
                return (HediffCompProperties_HeatPusher)this.props;
            }
        }


        protected virtual bool ShouldPushHeatNow
        {
            get
            {
                if (!parent.pawn.SpawnedOrAnyParentSpawned)
                {
                    return false;
                }
                
                float ambientTemperature = parent.pawn.AmbientTemperature;
                if (ambientTemperature < Props.heatPushMaxTemperature)
                {
                    return ambientTemperature > Props.heatPushMinTemperature;
                }
                return false;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (parent.pawn.IsHashIntervalTick(Props.tickCounter) && ShouldPushHeatNow)
            {
                GenTemperature.PushHeat(parent.pawn.PositionHeld, parent.pawn.MapHeld, Props.heatPerSecond);
            }
        }


    }
}
