
using Verse;

namespace AnimalBehaviours
{
    class CompDoesntFlee : ThingComp
    {
     

        public CompProperties_DoesntFlee Props
        {
            get
            {
                return (CompProperties_DoesntFlee)this.props;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {

            AnimalCollectionClass.AddNotFleeingAnimalToList(this.parent);

        }

        public override void PostDeSpawn(Map map)
        {
            AnimalCollectionClass.RemoveNotFleeingAnimalFromList(this.parent);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            AnimalCollectionClass.RemoveNotFleeingAnimalFromList(this.parent);
        }


    }
}
