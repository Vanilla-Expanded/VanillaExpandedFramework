using Verse;

namespace VanillaStorytellersExpanded
{
	public class StorytellerWatcher : GameComponent
	{
        public int lastRaidExpansionTicks;
        public StorytellerWatcher()
        {
        }

        public StorytellerWatcher(Game game)
        {

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastRaidExpansionTicks, "lastRaidExpansionTicks", 0);
        }
    }
}
