using System.Collections.Generic;
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_EnrageOtherPawns : CompProperties
    {

        //A comp class that will make other animals on the map go manhunter if it goes manhunter

        public List<PawnKindDef> pawnkinddefsToAffect;
        public int checkingInterval = 200;

        public CompProperties_EnrageOtherPawns()
        {
            this.compClass = typeof(CompEnrageOtherPawns);
        }


    }
}
