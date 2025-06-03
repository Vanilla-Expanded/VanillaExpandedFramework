using RimWorld;
using Verse;

namespace VFECore
{
    public static class QuestUtils
    {
        public static void CreateQuest(this QuestScriptDef questDef)
        {
            var quest = QuestUtility.GenerateQuestAndMakeAvailable(questDef, StorytellerUtility.DefaultThreatPointsNow(Find.World));
            if (questDef.sendAvailableLetter)
            {
                QuestUtility.SendLetterQuestAvailable(quest);
            }
        }
    }
}