using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VanillaStorytellersExpanded
{
    public class QuestGiverDef : Def
    {
        public QuestCurrency currency;
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

        public Type windowClass;


    }

    public class QuestWorker
    {
        public QuestGiverDef def;
        public virtual List<QuestInfo> GenerateQuests(QuestGiverManager questGiverManager)
        {
            List<QuestInfo> generatedQuests = new List<QuestInfo>();
            var questCountToGenerate = def.maximumAvailableQuestCount != -1 ? (def.maximumAvailableQuestCount - questGiverManager.AvailableQuests.Count) : 100;
            var points = StorytellerUtility.DefaultThreatPointsNow(Find.World);
            var questDefsToProcess = DefDatabase<QuestScriptDef>.AllDefs.Where(x => !x.isRootSpecial && x.IsRootAny).ToList();
            Log.Message($"quest count to generate: {questCountToGenerate} - points: {points}");

            while (generatedQuests.Count < questCountToGenerate)
            {
                if (!questDefsToProcess.Any())
                {
                    break;
                }
                var newQuestCandidate = questDefsToProcess.RandomElement();
                questDefsToProcess.Remove(newQuestCandidate);
                Log.Message($"newQuestCandidate is choosen: {newQuestCandidate}");
                try
                {
                    Slate slate = new Slate();
                    slate.Set("points", points);
                    if (newQuestCandidate == QuestScriptDefOf.LongRangeMineralScannerLump)
                    {
                        slate.Set("targetMineable", ThingDefOf.MineableGold);
                        slate.Set("worker", PawnsFinder.AllMaps_FreeColonists.FirstOrDefault());
                    }
                    if (newQuestCandidate.CanRun(slate))
                    {
                        Quest quest = QuestGen.Generate(newQuestCandidate, slate);
                        if (def.currency.Allows(questGiverManager, quest, Patch_AddSlateQuestTags.slate, out QuestInfo questInfo))
                        {
                            generatedQuests.Add(questInfo);
                            Log.Message($"Added new quest {quest.name}");
                        }
                    }
                }
                catch { }

            }
            return generatedQuests;
        }
    }
}
