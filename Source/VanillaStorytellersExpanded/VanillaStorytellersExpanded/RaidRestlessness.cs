using RimWorld;
using System;
using Verse;

namespace VanillaStorytellersExpanded
{
	public class RaidRestlessness : IExposable
	{
        public int startAfterTicks;
        public ThoughtDef thoughtDef;
        public int deadlineQuadrum;
        public int GetThoughtState()
        {
            if (Find.TickManager.TicksGame < this.startAfterTicks)
            {
                return -1;
            }
            else
            {
                var result = this.startAfterTicks + Current.Game.GetComponent<StorytellerWatcher>().lastRaidExpansionTicks;
                //Log.Message("Find.TickManager.TicksGame: " + Find.TickManager.TicksGame, true);
                //Log.Message("result: " + result, true);
                //Log.Message("result2: " + (int)((Find.TickManager.TicksGame - result) / 900000f), true);
                //Log.Message("result3: " + (int)((int)((Find.TickManager.TicksGame - result) / 900000f) / 4f), true);
                var stageIndex = (int)((int)((Find.TickManager.TicksGame - result) / 900000f) / 4f);
                return stageIndex;
            }
        }
        public void ExposeData()
        {
            Scribe_Values.Look<int>(ref this.startAfterTicks, "startAfterTicks", 0, true);
            Scribe_Defs.Look<ThoughtDef>(ref this.thoughtDef, "thoughtDef");
            Scribe_Values.Look<int>(ref this.deadlineQuadrum, "deadlineQuadrum", 1, true);
        }
    }
}
