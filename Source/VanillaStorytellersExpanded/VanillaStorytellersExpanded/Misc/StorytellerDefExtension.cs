using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace VanillaStorytellersExpanded
{
	public class StorytellerDefExtension : DefModExtension
	{
		public static StorytellerDefExtension Get(Def def)
		{
			return def.GetModExtension<StorytellerDefExtension>() ?? StorytellerDefExtension.DefaultValues;
		}

		private static readonly StorytellerDefExtension DefaultValues = new StorytellerDefExtension();

		public RaidRestlessness raidRestlessness;
		public StorytellerThreat storytellerThreat;
		public IncidentSpawnOptions incidentSpawnOptions;
	}

    public class RaidRestlessness : IExposable
    {
        public int startAfterTicks;
        public ThoughtDef thoughtDef;
        public int GetThoughtState()
        {
            if (Find.TickManager.TicksGame < this.startAfterTicks)
            {
                return -1;
            }
            else
            {
                var result = this.startAfterTicks + Current.Game.GetComponent<StorytellerWatcher>().lastRaidExpansionTicks;
                var stageIndex = (int)((int)((Find.TickManager.TicksGame - result) / 900000f) / 4f);
                return stageIndex;
            }
        }
        public void ExposeData()
        {
            Scribe_Values.Look<int>(ref this.startAfterTicks, "startAfterTicks", 0, true);
            Scribe_Defs.Look<ThoughtDef>(ref this.thoughtDef, "thoughtDef");
        }
    }

    public class StorytellerThreat : IExposable
    {
        public IntRange naturallGoodwillForAllFactions;
        public int disableThreatsAtPopulationCount;
        public float allDamagesMultiplier;
        public List<string> goodIncidents = new List<string>();
        public IntRange? raidWarningRange;
        public void ExposeData()
        {
            Scribe_Values.Look<IntRange>(ref naturallGoodwillForAllFactions, "naturallGoodwillForAllFactions");
            Scribe_Values.Look<IntRange?>(ref raidWarningRange, "raidWarningRange");
            Scribe_Values.Look<int>(ref disableThreatsAtPopulationCount, "disableThreatsAtPopulationCount");
            Scribe_Values.Look<float>(ref allDamagesMultiplier, "allDamagesMultiplier");
            Scribe_Collections.Look<string>(ref goodIncidents, "goodIncidents", LookMode.Value);
        }
    }

    public class IncidentSpawnOptions
    {
        public bool alliesReduceThreats;
        public bool alliesIncreaseGoodIncidents;

        public bool enemiesReduceThreats;
        public bool enemiesIncreaseGoodIncidents;

        public List<string> goodIncidents = new List<string>();
        public List<string> negativeIncidents = new List<string>();
        public List<string> neutralIncidents = new List<string>();
    }
}
