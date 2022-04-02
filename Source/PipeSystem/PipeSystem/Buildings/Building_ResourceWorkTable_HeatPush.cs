using Verse;

namespace PipeSystem
{
    public class Building_ResourceWorkTable_HeatPush : Building_ResourceWorkTable
    {
        private const int HeatPushInterval = 30;

        public override void UsedThisTick()
        {
            base.UsedThisTick();
            if (Find.TickManager.TicksGame % HeatPushInterval == 4)
            {
                GenTemperature.PushHeat(this, def.building.heatPerTickWhileWorking * HeatPushInterval);
            }
        }
    }
}