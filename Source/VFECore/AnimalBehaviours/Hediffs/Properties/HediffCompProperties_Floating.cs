using Verse;

namespace AnimalBehaviours
{
    public class HediffCompProperties_Floating : HediffCompProperties
    {

        //This is equivalent to the CompFloating class, but just adds things through a hediff

        public int checkingInterval = 500;
       
        public HediffCompProperties_Floating()
        {
            this.compClass = typeof(HediffComp_Floating);
        }
    }
}

