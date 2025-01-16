using RimWorld;
using Verse;

namespace VFECore
{
    public class QuestInfo : IExposable
    {
        public int tickCompleted;
        public int tickExpired;
        public int tickAccepted;
        public QuestEndOutcome outcome;
        public QuestScriptDef questDef;
        public Quest quest;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref questDef, "questDef");
            Scribe_References.Look(ref quest, "quest");
            Scribe_Values.Look(ref outcome, "outcome");
            Scribe_Values.Look(ref tickCompleted, "tickCompleted");
            Scribe_Values.Look(ref tickExpired, "tickExpired");
        }
    }
}