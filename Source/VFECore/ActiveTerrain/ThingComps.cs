using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace VFECore
{
    public class CompPowerTraderFloor : CompPowerTrader
    {
        public List<TerrainComp_PowerTrader> acceptedComps = new List<TerrainComp_PowerTrader>();
        public override void SetUpPowerVars()
        {
            base.SetUpPowerVars();
            UpdatePowerOutput();
        }
        public virtual void ReceiveTerrainComp(TerrainComp_PowerTrader comp)
        {
            acceptedComps.Add(comp);
            UpdatePowerOutput();
        }
        public virtual void Notify_TerrainCompRemoved(TerrainComp_PowerTrader comp)
        {
            acceptedComps.Remove(comp);
            UpdatePowerOutput();
        }
        public void UpdatePowerOutput()
        {
            float f = CurPowerDemand;
            var output = -Props.basePowerConsumption + f;
            PowerOutput = output;
        }

        float cachedCurPowerDemand;
        public float CurPowerDemand
        {
            get
            {
                float f = 0f;
                foreach (var comp in acceptedComps)
                {
                    f += comp.PowerOutput;
                }
                return cachedCurPowerDemand = f;
            }
        }

        public override string CompInspectStringExtra()
        {
            return base.CompInspectStringExtra() + "FloorWire_InspectStringPart".Translate(acceptedComps.Count, -cachedCurPowerDemand);
        }
    }
}
