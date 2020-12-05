using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_Metamorphosis : CompProperties
    {

        //A comp class that makes an animal change into another animal after a given time

        public float timeInYears;
        public string pawnToTurnInto;
        public string reportString = "VEF_TimeToMetamorphosis";

        public CompProperties_Metamorphosis()
        {
            this.compClass = typeof(CompMetamorphosis);
        }

    }
}
