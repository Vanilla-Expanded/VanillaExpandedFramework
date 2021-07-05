using RimWorld;
using Verse;

namespace VanillaCookingExpanded
{
    //A comp class to make items only stack if their ingredients lists are the same

    //Used for example in Vanilla Cooking Expanded gourmet meals to avoid condiment abuse

    public class CompProperties_StackByCondiments : CompProperties
    {

        public string condimentTagToCheck = "Condiments";

        public CompProperties_StackByCondiments()
        {
            this.compClass = typeof(CompStackByCondiments);
        }
    }
}