
using Verse;

namespace VEF.AnimalBehaviours
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

            StaticCollectionsClass.AddNoDiseasesAnimalToList(this.parent);

        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            StaticCollectionsClass.RemoveNoDiseasesAnimalFromList(this.parent);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            StaticCollectionsClass.RemoveNoDiseasesAnimalFromList(this.parent);
        }


    }
}
