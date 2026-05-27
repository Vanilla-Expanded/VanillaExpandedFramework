using System.Collections.Generic;
using Verse;

namespace VEF.AnimalBehaviours

{
    public class DeathActionProperties_SpawnItemsAndFilth : DeathActionProperties
    {
        public float dropChance = 1f;
        public bool isRandom = false;
        public List<ThingDefCountClass> items = new List<ThingDefCountClass>();
        public ThingDef filthCreated;
        public IntRange filthCountRange;
        public SoundDef sound;

        public DeathActionProperties_SpawnItemsAndFilth()
        {
            workerClass = typeof(DeathActionWorker_SpawnItemsAndFilth);
        }


    }
}