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
            Log.Message($"[QuestChains] Initialized QuestChains component.");
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
                        Log.Message($"[QuestChains] Firing scheduled quest: {futureQuest.questDef.defName}");
                        futureQuests.RemoveAt(i);
                    }
                }
            }
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
            Log.Message($"[QuestChains] Loaded game. Trying to schedule quests.");
            TryScheduleQuests();
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            Log.Message($"[QuestChains] Started new game. Trying to schedule quests.");
            TryScheduleQuests();
        }

        public void TryScheduleQuests()
        {
            Log.Message($"[QuestChains] Trying to schedule quests from chain definitions.");
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

            Log.Message($"[QuestChains] Quest completed: {quest.root.defName}, Outcome: {outcome}");

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

            Log.Message($"[QuestChains] Quest expired: {quest.root.defName}");

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
                Log.Message($"[QuestChains] Quest already scheduled: {quest.defName}");
                return false;
            }

            if (!ext.isRepeatable && quests.Any(x => x.questDef == quest))
            {
                Log.Message($"[QuestChains] Quest is not repeatable and has already been completed: {quest.defName}");
                return false;
            }

            if (ext.conditionEither != null && quests.Any(x => x.questDef == ext.conditionEither && x.tickAccepted > 0))
            {
                Log.Message($"[QuestChains] Quest condition 'conditionEither' not met for: {quest.defName}");
                return false;
            }

            if (ext.conditionSucceedQuests != null && ext.conditionSucceedQuests.NullOrEmpty() is false)
            {
                if (ext.conditionSucceedQuests.All(QuestIsCompletedAndSucceeded) is false)
                {
                    Log.Message($"[QuestChains] Quest condition 'conditionSucceedQuests' not met for: {quest.defName}");
                    return false;
                }
                else
                {
                    Log.Message($"[QuestChains] Scheduling quest: {quest.defName}, in {ext.ticksSinceSucceed.RandomInRange} ticks (conditionSucceedQuests)");
                    ScheduleQuestInTicks(quest, Find.TickManager.TicksGame + ext.ticksSinceSucceed.RandomInRange);
                    return true;
                }
            }

            if (ext.conditionFailQuests != null && ext.conditionFailQuests.NullOrEmpty() is false)
            {
                if (ext.conditionFailQuests.All(QuestIsCompletedAndFailed) is false)
                {
                    Log.Message($"[QuestChains] Quest condition 'conditionFailQuests' not met for: {quest.defName}");
                    return false;
                }
                else
                {
                    Log.Message($"[QuestChains] Scheduling quest: {quest.defName}, in {ext.ticksSinceFail.RandomInRange} ticks (conditionFailQuests)");
                    ScheduleQuestInTicks(quest, Find.TickManager.TicksGame + ext.ticksSinceFail.RandomInRange);
                    return true;
                }
            }

            if (ext.isRepeatable)
            {
                Log.Message($"[QuestChains] Scheduling quest: {quest.defName}, mtbDays: {ext.mtbDaysRepeat} (repeatable)");
                ScheduleQuestMTB(quest, ext.mtbDaysRepeat);
                return true;
            }
            else if (ext.conditionMinDaysSinceStart > 0)
            {
                var days = ext.conditionMinDaysSinceStart - GenDate.DaysPassed;
                var ticks = (int)(days <= 0 ? 0 : days * GenDate.TicksPerDay);
                Log.Message($"[QuestChains] Scheduling quest: {quest.defName}, in: {ticks} ticks");
                ScheduleQuestInTicks(quest, ticks);
                return true;
            }
            Log.Message($"[QuestChains] No matching conditions to schedule quest: {quest.defName}");
            return false;
        }

        public bool TryGrantAgainOnFailure(QuestScriptDef quest)
        {
            var extension = quest.GetModExtension<QuestChainExtension>();
            if (!extension.grantAgainOnFailure)
            {
                Log.Message($"[QuestChains] Quest does not allow retry on failure: {quest.defName}");
                return false;
            }
            Log.Message($"[QuestChains] Scheduling quest to be granted again on failure: {quest.defName}, in {extension.daysUntilGrantAgainOnFailure.RandomInRange} days");
            ScheduleQuestInTicks(quest, (int)(GenDate.TicksPerDay * extension.daysUntilGrantAgainOnFailure.RandomInRange));
            return true;
        }

        public bool TryGrantAgainOnExpiry(QuestScriptDef quest)
        {
            var extension = quest.GetModExtension<QuestChainExtension>();
            if (!extension.grantAgainOnExpiry)
            {
                Log.Message($"[QuestChains] Quest does not allow retry on expiry: {quest.defName}");
                return false;
            }
            Log.Message($"[QuestChains] Scheduling quest to be granted again on expiry: {quest.defName}, in {extension.daysUntilGrantAgainOnExpiry.RandomInRange} days");
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