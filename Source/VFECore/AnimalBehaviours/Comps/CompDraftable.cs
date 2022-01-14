
using Verse;

namespace AnimalBehaviours
{
    class CompDraftable : ThingComp
    {
        

        public CompProperties_Draftable Props
        {
            get
            {
                return (CompProperties_Draftable)this.props;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {

            AnimalCollectionClass.AddDraftableAnimalToList(this.parent);
            if (Props.makeNonFleeingToo)
            {
                AnimalCollectionClass.AddNotFleeingAnimalToList(this.parent);
            }

        }

        public override void PostDeSpawn(Map map)
        {
            AnimalCollectionClass.RemoveDraftableAnimalFromList(this.parent);
            if (Props.makeNonFleeingToo)
            {
                AnimalCollectionClass.RemoveNotFleeingAnimalFromList(this.parent);
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            AnimalCollectionClass.RemoveDraftableAnimalFromList(this.parent);
            if (Props.makeNonFleeingToo)
            {
                AnimalCollectionClass.RemoveNotFleeingAnimalFromList(this.parent);
            }
        }

       
    }
}
