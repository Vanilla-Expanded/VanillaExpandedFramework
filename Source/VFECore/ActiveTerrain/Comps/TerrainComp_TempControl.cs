using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace VFECore
{
    /// <summary>
    /// Terrain comp that controls and maintains temperature.
    /// </summary>
    public class TerrainComp_TempControl : TerrainComp_HeatPush
    {
        public bool operatingAtHighPower;
        public new TerrainCompProperties_TempControl Props { get { return (TerrainCompProperties_TempControl)props; } }
        public float AmbientTemperature { get { return GenTemperature.GetTemperatureForCell(parent.Position, parent.Map); } }
        public float PowerConsumptionNow
        {
            get
            {
                float basePowerConsumption = parent.def.GetCompProperties<TerrainCompProperties_PowerTrader>().basePowerConsumption;
                return operatingAtHighPower ? basePowerConsumption : basePowerConsumption * Props.lowPowerConsumptionFactor;
            }
        }

        [Unsaved] CompTempControl parentTempControl;
        public virtual CompTempControl HeaterToConformTo { get
            {
                if (parentTempControl != null && parentTempControl.parent.Spawned)
                {
                    parentTempControl = null;
                    return parentTempControl;
                }
                var room = parent.Position.GetRoom(parent.Map);
                if (room == null) return null;

                return parentTempControl = room.GetTempControl(this.AnalyzeType());
            } }
        public float TargetTemperature
        {
            get
            {
                return HeaterToConformTo?.TargetTemperature ?? 21;
            }
        }
        protected override float PushAmount
        {
            get
            {
                //Code mimicked from Building_Heater
                if (!Props.reliesOnPower || (parent.GetComp<TerrainComp_PowerTrader>()?.PowerOn ?? true))
                {
                    float ambientTemperature = AmbientTemperature;
                    //Ternary expression... Mathf.InverseLerp is already clamped though so Mathf.InverseLerp(120f, 20f, ambientTemperature) itself should yield same results
                    float heatPushEfficiency = (ambientTemperature < 20f) ? 1f : (ambientTemperature > 120f) ? 0f : Mathf.InverseLerp(120f, 20f, ambientTemperature);
                    float energyLimit = Props.energyPerSecond * heatPushEfficiency * 4.16666651f;
                    float num2 = GenTemperature.ControlTemperatureTempChange(parent.Position, parent.Map, energyLimit, TargetTemperature);
                    bool flag = !Mathf.Approximately(num2, 0f) && parent.Position.GetRoom(parent.Map) != null;//Added room check
                    var powerTraderComp = parent.GetComp<TerrainComp_PowerTrader>();
                    if (flag)
                    {
                        GenTemperature.PushHeat(parent.Position, parent.Map, num2);
                    }
                    if (powerTraderComp != null)
                    {
                        powerTraderComp.PowerOutput = flag ? -powerTraderComp.Props.basePowerConsumption : -powerTraderComp.Props.basePowerConsumption * Props.lowPowerConsumptionFactor;
                    }
                    operatingAtHighPower = flag;
                    return (flag) ? num2 : 0f;
                }
                operatingAtHighPower = false;
                return 0f;
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            if (Props.cleansSnow && Find.TickManager.TicksGame % 60 == this.HashCodeToMod(60))
            {
                CleanSnow();
                UpdatePowerConsumption();
            }
        }
        public virtual void CleanSnow()
        { 
            var snowDepth = parent.Map.snowGrid.GetDepth(parent.Position);
            if (!Mathf.Approximately(0f, snowDepth))
            {
                operatingAtHighPower = true;
                float newDepth = Mathf.Max(snowDepth - Props.snowMeltAmountPerSecond, 0f);
                parent.Map.snowGrid.SetDepth(parent.Position, newDepth);
            }
        }

        public void UpdatePowerConsumption()
        {
            TerrainComp_PowerTrader powerComp = parent.GetComp<TerrainComp_PowerTrader>();
            if (powerComp != null)
            {
                powerComp.PowerOutput = -PowerConsumptionNow;
            }
        }

        public override string TransformLabel(string label) { return base.TransformLabel(label) + " " + (operatingAtHighPower ? "HeatedFloor_HighPower".Translate() : "HeatedFloor_LowPower".Translate()); }
    }
}
