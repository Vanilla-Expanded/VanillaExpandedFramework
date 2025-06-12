
using Verse;

namespace VEF.AnimalBehaviours
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

            StaticCollectionsClass.AddNotFleeingAnimalToList(this.parent);

        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            StaticCollectionsClass.RemoveNotFleeingAnimalFromList(this.parent);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            StaticCollectionsClass.RemoveNotFleeingAnimalFromList(this.parent);
        }


    }
}
