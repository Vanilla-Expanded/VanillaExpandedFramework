using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VanillaStorytellersExpanded
{
    public class QuestWorker
    {
        public QuestGiverDef def;
        public virtual List<QuestInfo> GenerateQuests(QuestGiverManager questGiverManager)
        {
            List<QuestInfo> generatedQuests = new List<QuestInfo>();
            var questCountToGenerate = def.maximumAvailableQuestCount != -1 ? (def.maximumAvailableQuestCount - 
                questGiverManager.AvailableQuests.Count) : 100;
            var points = StorytellerUtility.DefaultThreatPointsNow(Find.World);
            var questDefsToProcess = questGiverManager.def.onlySpecifiedQuests != null ? questGiverManager.def.onlySpecifiedQuests :
                DefDatabase<QuestScriptDef>.AllDefs.Where(x => !x.isRootSpecial && x.IsRootAny).ToList();
            while (generatedQuests.Count < questCountToGenerate && questDefsToProcess.Any())
            {
                if (!questDefsToProcess.Any())
                {
                    break;
                }
                var newQuestCandidate = questDefsToProcess.RandomElement();
                questDefsToProcess.Remove(newQuestCandidate);
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
                        if (def.currency is null)
                        {
                            var questInfo = new QuestInfo(quest, questGiverManager.FixedQuestGiverFaction, null, onlyOneChoice: questGiverManager.def.onlyOneReward ? true : false);
                            generatedQuests.Add(questInfo);
                        }
                        else if (def.currency.Allows(questGiverManager, quest, Patch_AddSlateQuestTags.slate, out QuestInfo questInfo))
                        {
                            generatedQuests.Add(questInfo);
                        }
                    }
                }
                catch (Exception)
                {

                }

            }
            return generatedQuests;
        }
    }
}
