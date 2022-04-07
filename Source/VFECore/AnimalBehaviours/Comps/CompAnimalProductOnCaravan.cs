
using System.Collections.Generic;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace AnimalBehaviours
{

	public class CompAnimalProductOnCaravan : ThingComp
	{
		
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

		

		


		

		public override string CompInspectStringExtra()
		{
			
				return "VEF_WhileCaravaning".Translate(Props.resourceAmount,Props.resourceDef.LabelCap)+ Props.gatheringIntervalTicks.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor);
			
		}
	}
}