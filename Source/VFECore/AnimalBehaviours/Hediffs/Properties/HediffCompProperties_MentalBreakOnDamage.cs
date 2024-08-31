
using Verse;
using RimWorld;


namespace AnimalBehaviours
{
    public class HediffCompProperties_MentalBreakOnDamage : HediffCompProperties
    {
       
        public DamageDef damageTypeReceived;
        public MentalBreakDef mentalBreak;
        public string reason;
     
      

        public HediffCompProperties_MentalBreakOnDamage()
        {
            this.compClass = typeof(HediffComp_MentalBreakOnDamage);
        }
    }
}
