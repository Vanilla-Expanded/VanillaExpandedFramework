using Verse;

namespace AnimalBehaviours
{
    public class HediffCompProperties_Waterstriding : HediffCompProperties
    {


        public int checkingInterval = 500;
       
        public HediffCompProperties_Waterstriding()
        {
            this.compClass = typeof(HediffComp_Waterstriding);
        }
    }
}

