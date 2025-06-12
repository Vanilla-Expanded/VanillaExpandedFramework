using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VEF.AnimalBehaviours
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

            StaticCollectionsClass.AddFloatingAnimalToList(this.parent);

        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            StaticCollectionsClass.RemoveFloatingAnimalFromList(this.parent);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            StaticCollectionsClass.RemoveFloatingAnimalFromList(this.parent);
        }




    }
}

