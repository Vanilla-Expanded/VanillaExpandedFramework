
using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Text;

namespace AnimalBehaviours
{
    class HediffCompProperties_ThoughtEffecter : HediffCompProperties
    {


        //A comp class that makes a hediff produce a certain Thought in nearby pawns

        public int radius = 1;
        public int tickInterval = 1000;
        public string thoughtDef = "AteWithoutTable";
        public bool showEffect = false;
        public bool needsToBeTamed = false;
        public bool conditionalOnWellBeing = false;
        public string thoughtDefWhenSuffering = "AteWithoutTable";

        public HediffCompProperties_ThoughtEffecter()
        {
            this.compClass = typeof(HediffComp_ThoughtEffecter);
        }
    }
}
