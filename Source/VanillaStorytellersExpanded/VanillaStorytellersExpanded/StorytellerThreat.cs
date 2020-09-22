using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace VanillaStorytellersExpanded
{
	public class StorytellerThreat : IExposable
	{
        public IntRange naturallGoodwillForAllFactions;
        public int disableThreatsAtPopulationCount;
        public float allDamagesMultiplier;
        public List<string> goodIncidents = new List<string>();
        public void ExposeData()
        {
            Scribe_Values.Look<IntRange>(ref naturallGoodwillForAllFactions, "naturallGoodwillForAllFactions");
            Scribe_Values.Look<int>(ref disableThreatsAtPopulationCount, "disableThreatsAtPopulationCount");
            Scribe_Values.Look<float>(ref allDamagesMultiplier, "allDamagesMultiplier");
            Scribe_Collections.Look<string>(ref goodIncidents, "goodIncidents", LookMode.Value);
        }
    }
}
