using RimWorld;
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_Untameable : CompProperties
    {

        //A comp class to make animals not tameable. You can indicate what Faction to return them to.

        public CompProperties_Untameable()
        {
            this.compClass = typeof(CompUntameable);
        }

        public string factionToReturnTo = "";

        //If true their faction will be reset

        public bool goWild = false;

        //If true and factionToReturnTo not set, the creature will go manhunter if tamed

        public bool goesManhunter = true;

        //Optional message to send

        public bool sendMessage = false;
        public string message = "VEF_NotTameable";

    }
}