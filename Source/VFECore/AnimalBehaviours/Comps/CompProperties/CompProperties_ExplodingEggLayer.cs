using System;
using Verse;
using RimWorld;

namespace AnimalBehaviours
{
	public class CompProperties_ExplodingEggLayer : CompProperties
	{
		public CompProperties_ExplodingEggLayer()
		{
			this.compClass = typeof(CompExplodingEggLayer);
		}

		public float eggLayIntervalDays = 1f;

		public IntRange eggCountRange = IntRange.one;

		public ThingDef eggUnfertilizedDef;

		public ThingDef eggFertilizedDef;

		public int eggFertilizationCountMax = 1;

		public bool eggLayFemaleOnly = true;

		public float eggProgressUnfertilizedMax = 1f;
	}
}
