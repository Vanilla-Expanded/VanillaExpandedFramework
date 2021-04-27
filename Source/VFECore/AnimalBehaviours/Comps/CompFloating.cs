using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace AnimalBehaviours
{
    public class CompFloating : ThingComp
    {




        public CompProperties_Floating Props
        {
            get
            {
                return (CompProperties_Floating)this.props;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {

            AnimalCollectionClass.AddFloatingAnimalToList(this.parent);

        }

        public override void PostDeSpawn(Map map)
        {
            AnimalCollectionClass.RemoveFloatingAnimalFromList(this.parent);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            AnimalCollectionClass.RemoveFloatingAnimalFromList(this.parent);
        }




    }
}

