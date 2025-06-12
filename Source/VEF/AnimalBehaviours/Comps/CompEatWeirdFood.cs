using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace VEF.AnimalBehaviours
{
    public class CompEatWeirdFood : ThingComp
    {

        public int currentFeedings = 0;


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.currentFeedings, "currentFeedings", 0, false);
        }


        public CompProperties_EatWeirdFood Props
        {
            get
            {
                return (CompProperties_EatWeirdFood)this.props;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {

            StaticCollectionsClass.AddWeirdEaterAnimalToList(this.parent);

        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            StaticCollectionsClass.RemoveWeirdEaterAnimalFromList(this.parent);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            StaticCollectionsClass.RemoveWeirdEaterAnimalFromList(this.parent);
        }
    }
}