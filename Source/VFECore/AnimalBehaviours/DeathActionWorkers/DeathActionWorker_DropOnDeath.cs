using System;
using Verse;

namespace AnimalBehaviours
{
	public class DeathActionWorker_DropOnDeath : DeathActionWorker
	{

		System.Random rand = new System.Random();

		public override void PawnDied(Corpse corpse)
		{
			CompDropOnDeath comp = corpse.InnerPawn.TryGetComp<CompDropOnDeath>();
			if (comp != null)
            {



                if (corpse.Map != null)
                {
                    if (rand.NextDouble() <= comp.Props.dropChance)
                    {
                        ThingDef thingDef;
                        if (comp.Props.isRandom)
                        {

                            thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(comp.Props.randomItems.RandomElement());
                            if (thingDef != null)
                            {
                                Thing thing = ThingMaker.MakeThing(thingDef, null);
                                thing.stackCount = comp.Props.resourceAmount;
                                GenPlace.TryPlaceThing(thing, corpse.Position, corpse.Map, ThingPlaceMode.Near, null, null, default(Rot4));
                            }

                        }
                        else
                        {
                            thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(comp.Props.resourceDef);
                            if (thingDef != null)
                            {
                                Thing thing = ThingMaker.MakeThing(thingDef, null);
                                thing.stackCount = comp.Props.resourceAmount;
                                GenPlace.TryPlaceThing(thing, corpse.Position, corpse.Map, ThingPlaceMode.Near, null, null, default(Rot4));
                            }
                        }

                    }


                }




            }

		}
	}
}
