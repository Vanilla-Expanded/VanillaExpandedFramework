using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VFECore
{
    public class WeatherEffectsExtension : DefModExtension
    {
        public List<HediffAndStat> hediffsToApply;

        public IntRange ticksInterval;

        public bool causesRotting;

        public bool killsPlants;

        public float chanceToKillPlants;

        public float rotProgressPerDamage;

        public WeatherDef activeOnWeatherPerceived;

        public bool worksOnNonFleshPawns;

        public DamageDef damageToApply;

        public FloatRange damageRange;

        public bool worksIndoors;

        public float percentOfPawnsToDealDamage;
    }

    public class HediffAndStat
    {
        public string hediff;
        public StatDef effectMultiplyingStat;
        public float severityOffset;
    }
}