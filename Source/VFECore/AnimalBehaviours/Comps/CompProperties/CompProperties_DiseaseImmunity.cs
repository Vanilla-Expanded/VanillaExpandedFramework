
using Verse;
using System.Collections.Generic;

namespace AnimalBehaviours

{
    public class CompProperties_DiseaseImmunity : CompProperties
    {

        //Makes the animal automatically heal a disease when he is afflicted by it
   
        public List<string> hediffsToRemove = null;
   
        public int tickInterval = 250;

       


        public CompProperties_DiseaseImmunity()
        {
            this.compClass = typeof(CompDiseaseImmunity);
        }
    }
}
