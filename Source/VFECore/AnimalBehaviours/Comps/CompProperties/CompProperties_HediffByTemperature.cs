
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_HediffByTemperature : CompProperties
    {

        //A comp class that applies a hediff to the pawn depending on ambient temperature

        public bool doTemperatureBelow = false;
        public bool doTemperatureAbove = false;

        public float temperatureBelow;
        public float temperatureAbove;
        public int tickInterval = 1000;
        public HediffDef hediffBelow; 
        public HediffDef hediffAbove;

        public float severity = 1;
        public BodyPartDef bodyPart = null;


        public CompProperties_HediffByTemperature()
        {
            this.compClass = typeof(CompHediffByTemperature);
        }


    }
}