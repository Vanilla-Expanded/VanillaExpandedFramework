using Verse;
using System.Collections.Generic;


namespace AnimalBehaviours
{
    public class CompProperties_AnimalProductOnCaravan : CompProperties
    {

        //Makes the animal produce things while caravaning. These resources will be added
        //to caravan inventory, and CAN OVERLOAD IT, making it unable to move
      
        public int gatheringIntervalTicks = 30000;
        public int resourceAmount = 1;
        public ThingDef resourceDef = null;
        public bool femaleOnly = false;

       
        public CompProperties_AnimalProductOnCaravan()
        {
            this.compClass = typeof(CompAnimalProductOnCaravan);
        }


    }
}
