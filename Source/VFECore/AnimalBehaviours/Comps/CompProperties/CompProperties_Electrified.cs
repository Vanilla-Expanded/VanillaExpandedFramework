using Verse;
using System.Collections.Generic;


namespace AnimalBehaviours
{
    public class CompProperties_Electrified : CompProperties
    {

        //This comp class makes creatures recharge batteries nearby

        public int electroRate = 0;
        public int electroRadius = 0;
        public int electroChargeAmount = 1;
        public List<string> batteriesToAffect = null;

        public CompProperties_Electrified()
        {
            this.compClass = typeof(CompElectrified);
        }


    }
}