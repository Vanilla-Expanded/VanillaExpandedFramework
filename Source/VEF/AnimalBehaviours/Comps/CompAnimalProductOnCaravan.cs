
using System.Collections.Generic;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace VEF.AnimalBehaviours
{

	public class CompAnimalProductOnCaravan : ThingComp
	{
		
		public CompProperties_AnimalProductOnCaravan Props => (CompProperties_AnimalProductOnCaravan)props;


		public override void CompTickInterval(int delta)
		{
            if (parent.IsHashIntervalTick(Props.gatheringIntervalTicks, delta))
            {

				

				Pawn pawn = parent as Pawn;
				if (!Props.femaleOnly || pawn.gender == Gender.Female) {

					Caravan caravan = pawn.GetCaravan();

					if (caravan != null)
					{
						float mass = Props.resourceDef.BaseMass * Props.resourceAmount;


						if (caravan.MassUsage + mass < caravan.MassCapacity)
						{
							Thing thing = ThingMaker.MakeThing(Props.resourceDef);
							thing.stackCount = Props.resourceAmount;
							CaravanInventoryUtility.GiveThing(caravan, thing);

						}

						

					}
				}
				
            }
		}

		

		


		

		public override string CompInspectStringExtra()
		{
			if (!Props.femaleOnly || ((Pawn)parent).gender == Gender.Female)
			{
				return "VEF_WhileCaravaning".Translate(Props.resourceAmount, Props.resourceDef.LabelCap) + Props.gatheringIntervalTicks.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor);
			}
			else return null;
		}
	}
}