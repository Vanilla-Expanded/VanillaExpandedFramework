using Verse;

namespace AnimalBehaviours
{
    public class HediffCompProperties_Draftable : HediffCompProperties
    {

        //This is equivalent to the CompDraftable class, but just adds things through a hediff so draftability can be added via implant

        public int checkingInterval = 500;

        public HediffCompProperties_Draftable()
        {
            this.compClass = typeof(HediffComp_Draftable);
        }
    }
}

