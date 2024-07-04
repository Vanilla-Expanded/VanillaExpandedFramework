
using Verse;
using RimWorld;


namespace AnimalBehaviours
{
    public class HediffCompProperties_ExplodeOnDamage : HediffCompProperties
    {
        public int minDamageToExplode;

        public DamageDef damageType;
        public int damageAmount = -1;
        public float radius;
        public SoundDef sound = null;
        public ThingDef spawnThingDef = null;
        public float spawnThingChance = 0f;
      

        public HediffCompProperties_ExplodeOnDamage()
        {
            this.compClass = typeof(HediffComp_ExplodeOnDamage);
        }
    }
}
