using RimWorld;
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_Untameable : CompProperties
    {

        //A comp class to make animals not tameable. You can indicate what Faction to return them to. If null, they'll just go manhunter

        public CompProperties_Untameable()
        {
            this.compClass = typeof(CompUntameable);
        }

        public string factionToReturnTo = "";

    }
}