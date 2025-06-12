using Verse;
using UnityEngine;

namespace AnimalBehaviours
{
    public class CompProperties_FactionAfterHealthLoss : CompProperties
    {

        //A comp class that changes the faction of the pawn after losing a precent of it health

        public int healthPercent = 50;
        public int tickInterval = 1000;

        public string factionToReturnTo = "";

        //If true  and factionToReturnTo not set, their faction will be set to a random non hostile faction
        //If false or factionToReturnTo not set, their faction will be set to a random enemy faction

        public bool nonHostileFaction = false;

        //If true they will attack the colony

        public bool attackColony = false;

        public CompProperties_FactionAfterHealthLoss()
        {
            this.compClass = typeof(Comp_FactionAfterHealthLoss);
        }
    }
}