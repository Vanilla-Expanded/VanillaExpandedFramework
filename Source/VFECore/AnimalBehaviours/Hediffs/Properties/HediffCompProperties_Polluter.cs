

using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Text;

namespace AnimalBehaviours
{
    public class HediffCompProperties_Polluter : HediffCompProperties
    {


        public int amount;
        public int timer;


        public HediffCompProperties_Polluter()
        {
            this.compClass = typeof(HediffComp_Polluter);
        }
    }
}
