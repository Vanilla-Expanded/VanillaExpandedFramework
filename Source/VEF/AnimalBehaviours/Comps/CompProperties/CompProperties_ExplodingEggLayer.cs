using System;
using Verse;
using RimWorld;

namespace VEF.AnimalBehaviours
{
	public class CompProperties_ExplodingEggLayer : CompProperties
	{
		public CompProperties_ExplodingEggLayer()
		{
			this.compClass = typeof(CompExplodingEggLayer);
		}

		public float eggLayIntervalDays = 1f;

		public IntRange eggCountRange = IntRange.One;

		public ThingDef eggUnfertilizedDef;

		public ThingDef eggFertilizedDef;

		public int eggFertilizationCountMax = 1;

		public bool eggLayFemaleOnly = true;

		public float eggProgressUnfertilizedMax = 1f;
	}
}
