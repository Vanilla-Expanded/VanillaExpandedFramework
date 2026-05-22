using Verse;
namespace VEF.AnimalBehaviours
{
    public class DeathActionProperties_VanishInsect : DeathActionProperties
    {
        public FleckDef fleck;

        public ThingDef filth;

        public IntRange filthCountRange = IntRange.One;

        public DeathActionProperties_VanishInsect()
        {
            workerClass = typeof(DeathActionWorker_VanishInsect);
        }
    }
}
