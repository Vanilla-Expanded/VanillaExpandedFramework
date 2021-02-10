using Verse;
using System.Collections.Generic;


namespace AnimalBehaviours
{
    public class CompProperties_Refueling : CompProperties
    {

        //Similar to CompProperties_Electrified, this comp makes the user
        //refuel nearby specified buildings

        public int fuelingRate = 0;
        public int fuelingRadius = 0;
        public List<string> buildingsToAffect = null;
        public bool mustBeTamed = false;

        public CompProperties_Refueling()
        {
            this.compClass = typeof(CompRefueling);
        }


    }
}
