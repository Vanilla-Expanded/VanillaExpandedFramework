
using Verse;

namespace AnimalBehaviours
{
    class CompDiseaseEventImmunity : ThingComp
    {


        public CompProperties_DiseaseEventImmunity Props
        {
            get
            {
                return (CompProperties_DiseaseEventImmunity)this.props;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {

            AnimalCollectionClass.AddNoDiseasesAnimalToList(this.parent);

        }

        public override void PostDeSpawn(Map map)
        {
            AnimalCollectionClass.RemoveNoDiseasesAnimalFromList(this.parent);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            AnimalCollectionClass.RemoveNoDiseasesAnimalFromList(this.parent);
        }


    }
}
