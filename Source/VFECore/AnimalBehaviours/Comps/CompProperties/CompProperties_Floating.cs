using Verse;


namespace AnimalBehaviours
{

    public class CompProperties_Floating : CompProperties
    {
        //These two values are unused, keeping them just for back compat

        public bool isFloater = false;
        public bool canCrossWater = false;

        public CompProperties_Floating()
        {
            this.compClass = typeof(CompFloating);
        }

    }
}