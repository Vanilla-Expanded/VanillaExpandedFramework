using RimWorld;
using Verse;

namespace AnimalBehaviours
{
    public class DeathActionProperties_ConfigurableExplosion : DeathActionProperties
    {
        public float babyExplosionRadius = 1.9f;
        public float juvenileExplosionRadius = 2.9f;
        public float adultExplosionRadius = 4.9f;

        public DamageDef damageDef;

        public int damAmount = -1;
        public int armorPenetration = 0;
        public SoundDef explosionSound = null;

        public DeathActionProperties_ConfigurableExplosion()
        {
            workerClass = typeof(DeathActionWorker_ConfigurableExplosion);
        }
    }
}