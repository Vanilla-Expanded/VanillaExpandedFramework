using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace PipeSystem
{
    public class Building_ResourceHeater : Building_TempControl
    {
        public CompResourceTrader compResourceTrader;


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            compResourceTrader = GetComp<CompResourceTrader>();
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void TickRare()
        {
            if (compTempControl != null && compResourceTrader != null && compResourceTrader.PipeNet != null && compResourceTrader.ResourceOn)
            {
                float efficiency = Mathf.InverseLerp(120f, 20f, AmbientTemperature);
                float energyLimit = compTempControl.Props.energyPerSecond * efficiency * 4.16666651f;
                float tempChange = GenTemperature.ControlTemperatureTempChange(Position, Map, energyLimit, compTempControl.targetTemperature);

                if (!Mathf.Approximately(tempChange, 0f))
                {
                    this.GetRoom().Temperature += tempChange;
                    compResourceTrader.BaseConsumption = compResourceTrader.Props.consumptionPerTick;
                    compResourceTrader.PipeNet.receiversDirty = true;
                }
                else
                {
                    compResourceTrader.BaseConsumption = compResourceTrader.Props.consumptionPerTick * compTempControl.Props.lowPowerConsumptionFactor;
                    compResourceTrader.PipeNet.receiversDirty = true;
                }
            }
        }
    }
}