using Verse;

namespace VanillaGenesExpanded
{
    public class CompProperties_HumanHatcher : CompProperties
    {

       
        public float hatcherDaystoHatch = 1f;

        public CompProperties_HumanHatcher()
        {
            this.compClass = typeof(CompHumanHatcher);
        }
    }
}