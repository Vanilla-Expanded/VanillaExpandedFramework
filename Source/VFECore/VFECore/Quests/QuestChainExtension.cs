using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VFECore
{
    public class QuestChainExtension : DefModExtension
    {
        public QuestChainDef questChainDef;
        public List<QuestScriptDef> conditionSucceedQuests;
        public List<QuestScriptDef> conditionFailQuests;
        public IntRange ticksSinceSucceed;
        public IntRange ticksSinceFail;

        public QuestScriptDef conditionEither;
        public float conditionMinDaysSinceStart;
        public bool isRepeatable;
        public float mtbDaysRepeat;

        public bool grantAgainOnFailure;
        public FloatRange daysUntilGrantAgainOnFailure;
        public bool grantAgainOnExpiry;
        public FloatRange daysUntilGrantAgainOnExpiry;
    }
}