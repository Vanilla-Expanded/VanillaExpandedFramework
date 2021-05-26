
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_AutoNutrition : CompProperties
    {

        //A comp class that makes the animal stop when it's hungry and eat a source of food that the player doesn't see
        //Example: ants, or air

        public int tickInterval = 250;
        public string consumingFoodReportString = "Eating food";

        public CompProperties_AutoNutrition()
        {
            this.compClass = typeof(CompAutoNutrition);
        }


    }
}
