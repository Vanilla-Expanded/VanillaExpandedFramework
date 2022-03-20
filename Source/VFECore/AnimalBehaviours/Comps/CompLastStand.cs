using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.AI;

namespace AnimalBehaviours
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

            AnimalCollectionClass.AddLastStandAnimalToList(this.parent,Props.finalCoolDownMultiplier);
           

        }

        public override void PostDeSpawn(Map map)
        {
            AnimalCollectionClass.RemoveLastStandAnimalFromList(this.parent);
           
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            AnimalCollectionClass.RemoveLastStandAnimalFromList(this.parent);
           
        }



    }
}

