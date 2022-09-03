using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace VanillaStorytellersExpanded
{
    public class QuestGiverDef : Def
    {
        public QuestCurrency currency;
        public FactionDef fixedQuestGiverFaction;
        public List<QuestScriptDef> onlySpecifiedQuests;
        public int resetEveryTick = -1;
        public bool hideGeneratedQuestsInVanilla;
        public bool generateOnce;
        public bool onlyOneReward;
        public int maximumAvailableQuestCount = -1;
            
        [Unsaved(false)]
        private QuestWorker workerInt;

        private Type workerClass;
        public QuestWorker Worker
        {
            get
            {
                if (workerInt == null)
                {
                    workerInt = (QuestWorker)Activator.CreateInstance(workerClass);
                    workerInt.def = this;
                }
                return workerInt;
            }
        }

        public Type windowClass = typeof(Window_Contracts);

        public string windowTitleKey;
    }
}
