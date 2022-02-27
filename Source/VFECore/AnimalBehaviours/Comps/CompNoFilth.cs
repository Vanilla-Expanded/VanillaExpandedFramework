
using Verse;

namespace AnimalBehaviours
{
    class CompNoFilth : ThingComp
    {


        public CompProperties_NoFilth Props
        {
            get
            {
                return (CompProperties_NoFilth)this.props;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {

            AnimalCollectionClass.AddNoFilthAnimalToList(this.parent);

        }

        public override void PostDeSpawn(Map map)
        {
            AnimalCollectionClass.RemoveNoFilthAnimalFromList(this.parent);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            AnimalCollectionClass.RemoveNoFilthAnimalFromList(this.parent);
        }


    }
}
