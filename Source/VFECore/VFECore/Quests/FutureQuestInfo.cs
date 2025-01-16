using RimWorld;
using Verse;

namespace VFECore
{
    public class FutureQuestInfo : IExposable
    {
        public int tickToFire;
        public float mtbDays;
        public QuestScriptDef questDef;

        public bool TryFire()
        {
            if (tickToFire > 0 && Find.TickManager.TicksGame >= tickToFire 
                || mtbDays > 0 && Rand.MTBEventOccurs(mtbDays, 60000f, 60f))
            {
                var quest = QuestUtility.GenerateQuestAndMakeAvailable(questDef, StorytellerUtility.DefaultThreatPointsNow(Find.World));
                if (questDef.sendAvailableLetter)
                {
                    QuestUtility.SendLetterQuestAvailable(quest);
                }
                return true;
            }
            return false;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref tickToFire, "tickToFire");
            Scribe_Values.Look(ref mtbDays, "mtbDays");
            Scribe_Defs.Look(ref questDef, "questDef");
        }
    }
}