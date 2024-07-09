using System.Collections.Generic;
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_MakeOtherPawnsFlee : CompProperties
    {


        public List<PawnKindDef> pawnkinddefsToAffect;
        public int checkingInterval = 200;

        public CompProperties_MakeOtherPawnsFlee()
        {
            this.compClass = typeof(CompMakeOtherPawnsFlee);
        }


    }
}
