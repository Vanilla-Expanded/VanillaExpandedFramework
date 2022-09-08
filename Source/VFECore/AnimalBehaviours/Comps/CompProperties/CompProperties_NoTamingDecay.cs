
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_NoTamingDecay : CompProperties
    {
        //Both taming and training of this animal won't ever decay, avoiding it losing
        //training abilities and returning to the wild
       
        public CompProperties_NoTamingDecay()
        {
            this.compClass = typeof(CompNoTamingDecay);
        }


    }
}