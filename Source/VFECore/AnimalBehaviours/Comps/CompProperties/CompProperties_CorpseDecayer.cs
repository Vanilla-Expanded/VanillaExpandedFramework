
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_CorpseDecayer : CompProperties
    {
        
        //A comp class to make a creature feed on nearby corpses, making it recover its hunger
        //and rotting the corpses

        public int radius = 5;
        public int tickInterval = 500;
        public int decayOnHitPoints = 1;
        public float nutritionGained = 0.2f;
        public string corpseSound = "";

        public CompProperties_CorpseDecayer()
        {
            this.compClass = typeof(CompCorpseDecayer);
        }


    }
}