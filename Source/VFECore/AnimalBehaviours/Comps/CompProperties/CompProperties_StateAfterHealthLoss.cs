
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_StateAfterHealthLoss : CompProperties
    {

        //A comp class that applies a mental state to the pawn after losing a precent of it health

        public int healthPercent = 50;
        public int tickInterval = 1000;
        public string mentalState = "PanicFlee";


        public CompProperties_StateAfterHealthLoss()
        {
            this.compClass = typeof(CompStateAfterHealthLoss);
        }


    }
}