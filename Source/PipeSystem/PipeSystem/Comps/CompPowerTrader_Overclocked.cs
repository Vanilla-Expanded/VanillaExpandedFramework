using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;

namespace PipeSystem
{
    public class CompPowerTrader_Overclocked : ThingComp
    {

        public const float IdleFrac = 0.25f;
        public CompAdvancedResourceProcessor cachedAdvancedProcessor;
        public CompPowerTrader cachedPowerTrader;


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

        public CompPowerTrader PowerTrader
        {
            get
            {
                if (cachedPowerTrader == null)
                {
                    cachedPowerTrader = this.parent.GetComp<CompPowerTrader>();
                }
                return cachedPowerTrader;
            }
        }

        public override void CompTickRare()
        {
            base.CompTickRare();

            float overclock = AdvancedProcessor.overclockMultiplier;
            PowerTrader.PowerOutput = PowerTrader.Props.PowerConsumption * (IdleFrac + (1 - IdleFrac) * overclock * overclock);
        }




    }
}
