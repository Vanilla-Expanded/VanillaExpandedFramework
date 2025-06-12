using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.AI;

namespace VEF.AnimalBehaviours
{
    public class CompLastStand : ThingComp
    {

        

        public CompProperties_LastStand Props
        {
            get
            {
                return (CompProperties_LastStand)this.props;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {

            StaticCollectionsClass.AddLastStandAnimalToList(this.parent,Props.finalCoolDownMultiplier);
           

        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            StaticCollectionsClass.RemoveLastStandAnimalFromList(this.parent);
           
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            StaticCollectionsClass.RemoveLastStandAnimalFromList(this.parent);
           
        }



    }
}

