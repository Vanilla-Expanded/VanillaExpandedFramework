using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;


namespace AnimalBehaviours
{
    public class CompDropOnDeath : ThingComp
    {

        System.Random rand = new System.Random();

        


        public CompProperties_DropOnDeath Props
        {
            get
            {
                return (CompProperties_DropOnDeath)this.props;
            }
        }

        

        public override void PostDestroy(DestroyMode mode, Map previousMap) {
            Pawn pawn = this.parent as Pawn;
            if (previousMap != null && pawn.health.Dead) {
                if (rand.NextDouble() <= Props.dropChance)
                {
                    ThingDef thingDef;
                    if (Props.isRandom)
                    {

                        thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(Props.randomItems.RandomElement());
                        if (thingDef != null)
                        {
                            Thing thing = ThingMaker.MakeThing(thingDef, null);
                            thing.stackCount = Props.resourceAmount;
                            GenPlace.TryPlaceThing(thing, this.parent.Position, this.parent.Map, ThingPlaceMode.Near, null, null, default(Rot4));
                        }

                    }
                    else
                    {
                        thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(Props.resourceDef);
                        if (thingDef != null)
                        {
                            Thing thing = ThingMaker.MakeThing(thingDef, null);
                            thing.stackCount = Props.resourceAmount;
                            GenPlace.TryPlaceThing(thing, this.parent.Position, this.parent.Map, ThingPlaceMode.Near, null, null, default(Rot4));
                        }
                    }

                }


            }
            

            
        
        
        }


    }
}
