
using Verse;
using RimWorld;


namespace VEF.AnimalBehaviours
{
    public class HediffCompProperties_Thorns : HediffCompProperties
    {
        public IntRange damageDone;

        public DamageDef damageType;

        public float armorPenetration = 0;

       

        public HediffCompProperties_Thorns()
        {
            this.compClass = typeof(HediffComp_Thorns);
        }
    }
}
