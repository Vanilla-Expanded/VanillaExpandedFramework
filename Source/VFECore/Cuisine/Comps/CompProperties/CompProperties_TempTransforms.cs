using RimWorld;
using Verse;

namespace VanillaCookingExpanded
{

    //A comp class to make an item transform into a different one if a certain temperature is reached

    //It is used for example in Vanilla Cooking Expanded's grills, that turn into "ruined" versions of
    //themselves if frozen, or in vanilla Brewing Expanded's Hot coffee, that turns into Iced coffee
    //when frozen

    public class CompProperties_TempTransforms : CompProperties
    {
        public CompProperties_TempTransforms()
        {
            this.compClass = typeof(CompTempTransforms);
        }

        public float minSafeTemperature;

        public float maxSafeTemperature = 100f;

        public float progressPerDegreePerTick = 1E-05f;

        public string thingToTransformInto = "";
    }
}
