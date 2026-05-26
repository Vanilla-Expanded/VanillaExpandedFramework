using System.Collections.Generic;
using Verse;

namespace VEF.AnimalBehaviours

{
    public class DeathActionProperties_DivideAndCreateFilth : DeathActionProperties
    {
        public List<PawnKindDef> dividePawnKindOptions = new List<PawnKindDef>();
        public ThingDef filthCreated;
        public IntRange filthCountRange;

        public DeathActionProperties_DivideAndCreateFilth()
        {
            workerClass = typeof(DeathActionWorker_DivideAndCreateFilth);
        }


    }
}