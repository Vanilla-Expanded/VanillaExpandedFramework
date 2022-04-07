
using System.Collections.Generic;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace AnimalBehaviours
{

	public class CompAnimalProductOnCaravan : ThingComp
	{
		private int ticksUntilSpawn;

		public CompProperties_AnimalProductOnCaravan Props => (CompProperties_AnimalProductOnCaravan)props;

		

		

		public override void CompTick()
		{
            if (parent.IsHashIntervalTick(Props.gatheringIntervalTicks))
            {
				Pawn pawn = parent as Pawn;
				if (CaravanUtility.IsCaravanMember(pawn))
                {
					
					Caravan caravan = CaravanUtility.GetCaravan(pawn);
					float mass = Props.resourceDef.BaseMass * Props.resourceAmount;
					

					if (caravan.MassUsage + mass < caravan.MassCapacity)
                    {
						Thing thing = ThingMaker.MakeThing(Props.resourceDef);
						thing.stackCount= Props.resourceAmount;
						CaravanInventoryUtility.GiveThing(caravan, thing);

					}

                }
            }
		}

		

		

		public override void PostExposeData()
		{
		
			Scribe_Values.Look(ref ticksUntilSpawn, "ticksUntilSpawn", 0);
		}

		

		public override string CompInspectStringExtra()
		{
			
				return "GR_WhileCaravaning".Translate(GenLabel.ThingLabel(Props.resourceDef, null, Props.resourceAmount)).Resolve()+ ticksUntilSpawn.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor);
			
		}
	}
}