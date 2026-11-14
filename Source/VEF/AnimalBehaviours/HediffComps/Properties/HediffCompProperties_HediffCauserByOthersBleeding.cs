
using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Text;

namespace VEF.AnimalBehaviours
{
    public class HediffCompProperties_HediffCauserByOthersBleeding : HediffCompProperties
    {


        //A comp class that makes a hediff cause another hediff if any nearby pawn is bleeding

        public int radius = 1;
        public int tickInterval = 1000;
        public HediffDef newHediff;
        public bool needsToBeTamed = false;

        public HediffCompProperties_HediffCauserByOthersBleeding()
        {
            this.compClass = typeof(HediffComp_HediffCauserByOthersBleeding);
        }
    }
}
