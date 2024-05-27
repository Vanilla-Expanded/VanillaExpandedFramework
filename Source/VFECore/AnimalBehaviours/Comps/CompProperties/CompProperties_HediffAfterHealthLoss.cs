
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_HediffAfterHealthLoss : CompProperties
    {

        //A comp class that applies a hediff to the pawn after losing a precent of it health

        public int healthPercent = 50;
        public int tickInterval = 1000;
        public HediffDef hediff;
        public float severity = 1;
        public BodyPartDef bodyPart = null;


        public CompProperties_HediffAfterHealthLoss()
        {
            this.compClass = typeof(CompHediffAfterHealthLoss);
        }


    }
}