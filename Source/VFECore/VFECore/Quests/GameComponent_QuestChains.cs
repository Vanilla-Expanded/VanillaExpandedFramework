using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VFECore
{
    public class GameComponent_QuestChains : GameComponent
    {
        public List<QuestInfo> quests = new List<QuestInfo>();
        public List<FutureQuestInfo> futureQuests = new List<FutureQuestInfo>();
        public static GameComponent_QuestChains Instance;

        private List<QuestScriptDef> questsInChains;
        public List<QuestScriptDef> QuestsInChains => questsInChains ??= DefDatabase<QuestScriptDef>.AllDefsListForReading.Where(x => x.GetModExtension<QuestChainExtension>() != null).ToList();

        public GameComponent_QuestChains(Game game)
        {
            Instance = this;
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                for (int i = futureQuests.Count - 1; i >= 0; i--)
                {
                    FutureQuestInfo futureQuest = futureQuests[i];
                    if (futureQuest.TryFire())
                    {
                        futureQuests.RemoveAt(i);
                    }
                }
            }
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
            TryScheduleQuests();
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            TryScheduleQuests();
        }

        public void TryScheduleQuests()
        {
            foreach (var questDef in QuestsInChains)
            {
                TryScheduleQuest(questDef);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref quests, "finishedQuests", LookMode.Deep);
            Scribe_Collections.Look(ref futureQuests, "futureQuests", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                quests ??= new List<QuestInfo>();
                futureQuests ??= new List<FutureQuestInfo>();
            }
            Instance = this;
        }

        public void QuestCompleted(Quest quest, QuestEndOutcome outcome)
        {
            var entry = quests.LastOrDefault(x => x.quest == quest);
            if (entry != null)
            {
                entry.outcome = outcome;
                entry.tickCompleted = Find.TickManager.TicksGame;
            }

            if (outcome == QuestEndOutcome.Fail)
            {
                TryGrantAgainOnFailure(quest.root);
            }

            TryScheduleQuests();
        }

        public void QuestExpired(Quest quest)
        {
            var entry = quests.FirstOrDefault(x => x.quest == quest);
            if (entry != null)
            {
                entry.tickExpired = Find.TickManager.TicksGame;
            }

            TryGrantAgainOnExpiry(quest.root);
            TryScheduleQuests();
        }

        public bool QuestIsCompletedAndSucceeded(QuestScriptDef quest)
        {
            return quests.Any(x => x.questDef == quest && x.outcome == QuestEndOutcome.Success);
        }

        public bool QuestIsCompletedAndFailed(QuestScriptDef quest)
        {
            return quests.Any(x => x.questDef == quest && x.outcome == QuestEndOutcome.Fail);
        }

        public bool TryScheduleQuest(QuestScriptDef quest)
        {
            var ext = quest.GetModExtension<QuestChainExtension>();
            if (futureQuests.Any(x => x.questDef == quest))
            {
                return false;
            }

            if (!ext.isRepeatable && quests.Any(x => x.questDef == quest))
            {
                return false;
            }

            if (ext.conditionEither != null && quests.Any(x => x.questDef == ext.conditionEither && x.tickAccepted > 0))
            {
                return false;
            }

            if (ext.conditionSucceedQuests != null && ext.conditionSucceedQuests.NullOrEmpty() is false)
            {
                if (ext.conditionSucceedQuests.All(QuestIsCompletedAndSucceeded) is false)
                {
                    return false;
                }
                else
                {
                    ScheduleQuestInTicks(quest, ext.ticksSinceSucceed.RandomInRange);
                    return true;
                }
            }

            if (ext.conditionFailQuests != null && ext.conditionFailQuests.NullOrEmpty() is false)
            {
                if (ext.conditionFailQuests.All(QuestIsCompletedAndFailed) is false)
                {
                    return false;
                }
                else
                {
                    ScheduleQuestInTicks(quest, ext.ticksSinceFail.RandomInRange);
                    return true;
                }
            }

            if (ext.isRepeatable)
            {
                ScheduleQuestMTB(quest, ext.mtbDaysRepeat);
                return true;
            }
            else if (ext.conditionMinDaysSinceStart > 0)
            {
                var days = ext.conditionMinDaysSinceStart - GenDate.DaysPassed;
                var ticks = (int)(days <= 0 ? 0 : days * GenDate.TicksPerDay);
                ScheduleQuestInTicks(quest, ticks);
                return true;
            }
            return false;
        }

        public bool TryGrantAgainOnFailure(QuestScriptDef quest)
        {
            var extension = quest.GetModExtension<QuestChainExtension>();
            if (!extension.grantAgainOnFailure)
            {
                return false;
            }
            ScheduleQuestInTicks(quest, (int)(GenDate.TicksPerDay * extension.daysUntilGrantAgainOnFailure.RandomInRange));
            return true;
        }

        public bool TryGrantAgainOnExpiry(QuestScriptDef quest)
        {
            var extension = quest.GetModExtension<QuestChainExtension>();
            if (!extension.grantAgainOnExpiry)
            {
                return false;
            }
            ScheduleQuestMTB(quest, (int)(GenDate.TicksPerDay * extension.daysUntilGrantAgainOnExpiry.RandomInRange));
            return true;
        }

        public void ScheduleQuestInTicks(QuestScriptDef quest, int ticksInFuture)
        {
            futureQuests.Add(new FutureQuestInfo
            {
                questDef = quest,
                tickToFire = Find.TickManager.TicksGame + ticksInFuture
            });
        }

        public void ScheduleQuestMTB(QuestScriptDef quest, float mtbDays)
        {
            futureQuests.Add(new FutureQuestInfo
            {
                questDef = quest,
                mtbDays = mtbDays,
            });
        }
    }
}