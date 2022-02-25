using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VFECore
{
    public class WeatherEffectsExtension : DefModExtension
    {
        public List<HediffAndStat> hediffs;

        public int ticksInterval;

        public float severity;

        public bool causesRotting;

        public bool killingPlants;
    }

    public class HediffAndStat
    {
        public string hediffDefName;
        public StatDef affectingStat;
    }
}