
using Verse;

namespace VEF.AnimalBehaviours
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

            StaticCollectionsClass.AddNoFilthAnimalToList(this.parent);

        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            StaticCollectionsClass.RemoveNoFilthAnimalFromList(this.parent);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            StaticCollectionsClass.RemoveNoFilthAnimalFromList(this.parent);
        }


    }
}
