using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VFECore
{
    public class QuestChainDef : Def
    {
        public string iconPath;
        public Texture2D icon;
        public string questChainName;
        public Type workerClass;
        private QuestChainWorker cachedWorker;

        public override void PostLoad()
        {
            base.PostLoad();
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                if (!iconPath.NullOrEmpty())
                {
                    icon = ContentFinder<Texture2D>.Get(iconPath);
                }
            });

            if (workerClass == null)
            {
                workerClass = typeof(QuestChainWorker);
            }

            cachedWorker = (QuestChainWorker)Activator.CreateInstance(workerClass);
        }

        public QuestChainWorker Worker => cachedWorker;
    }

    public class QuestChainWorker
    {
        public virtual string GetDescription(QuestChainDef def)
        {
            return def.description;
        }
    }

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
    [HarmonyPatch(typeof(Quest), "CleanupQuestParts")]
    public static class Quest_CleanupQuestParts_Patch
    {
        public static void Prefix(Quest __instance, QuestEndOutcome ___endOutcome)
        {
            if (___endOutcome == QuestEndOutcome.Success || ___endOutcome == QuestEndOutcome.Fail)
            {
                QuestChains.Instance.QuestCompleted(__instance, ___endOutcome);
            }
            else if (__instance.State == QuestState.EndedOfferExpired)
            {
                QuestChains.Instance.QuestExpired(__instance);
            }
        }
    }

    public class QuestChains : GameComponent
    {
        public List<QuestInfo> quests = new List<QuestInfo>();
        public List<FutureQuestInfo> futureQuests = new List<FutureQuestInfo>();
        public static QuestChains Instance;

        private List<QuestScriptDef> questsInChains;
        public List<QuestScriptDef> QuestsInChains => questsInChains ??= DefDatabase<QuestScriptDef>.AllDefsListForReading.Where(x => x.GetModExtension<QuestChainExtension>() != null).ToList();
        
        public QuestChains(Game game)
        {
            Instance = this;
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                foreach (var quest in QuestsInChains)
                {
                    if (CanGrantQuest(quest))
                    {

                    }
                }
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
            else if (CanGrantQuest(quest.root))
            {

            }
        }

        public void QuestExpired(Quest quest)
        {
            var entry = quests.FirstOrDefault(x => x.quest == quest);
            if (entry != null)
            {
                entry.tickExpired = Find.TickManager.TicksGame;
            }
            TryGrantAgainOnExpiry(quest.root);
        }

        public bool QuestIsCompletedAndSucceeded(QuestScriptDef quest)
        {
            return quests.Any(x => x.questDef == quest && x.outcome == QuestEndOutcome.Success);
        }

        public bool QuestIsCompletedAndFailed(QuestScriptDef quest)
        {
            return quests.Any(x => x.questDef == quest && x.outcome == QuestEndOutcome.Fail);
        }

        public bool CanGrantQuest(QuestScriptDef quest)
        {
            var extension = quest.GetModExtension<QuestChainExtension>();
            if (extension == null)
            {
                return false;
            }

            if (extension.conditionMinDaysSinceStart > 0 && Find.TickManager.TicksGame
                < (extension.conditionMinDaysSinceStart * 60000))
            {
                return false;
            }

            if (extension.conditionEither != null && quests.Any(x => x.questDef == extension.conditionEither && x.tickAccepted > 0))
            {
                return false;
            }

            if (extension.conditionSucceedQuests != null && extension.conditionSucceedQuests.NullOrEmpty() is false
                && !extension.conditionSucceedQuests.All(QuestIsCompletedAndSucceeded))
            {
                return false;
            }

            if (extension.conditionFailQuests != null && extension.conditionFailQuests.NullOrEmpty() is false 
                && !extension.conditionFailQuests.All(QuestIsCompletedAndFailed))
            {
                return false;
            }

            if (extension.conditionSucceedQuests != null && extension.conditionSucceedQuests.NullOrEmpty() is false 
                && extension.ticksSinceSucceed != null)
            {
                foreach (QuestScriptDef successQuest in extension.conditionSucceedQuests)
                {
                    var completedQuestInfo = quests.LastOrDefault(x => x.questDef == successQuest
                    && x.outcome == QuestEndOutcome.Success);
                    if (completedQuestInfo != null && (Find.TickManager.TicksGame - completedQuestInfo.tickCompleted) < extension.ticksSinceSucceed.min || (Find.TickManager.TicksGame - completedQuestInfo.tickCompleted) > extension.ticksSinceSucceed.max)
                    {
                        return false;
                    }
                }
            }

            if (extension.conditionFailQuests != null && extension.conditionFailQuests.NullOrEmpty() is false && extension.ticksSinceFail != null)
            {
                foreach (QuestScriptDef failQuest in extension.conditionFailQuests)
                {
                    var completedQuestInfo = quests.LastOrDefault(x => x.questDef == failQuest && x.outcome == QuestEndOutcome.Fail);
                    if (completedQuestInfo != null && (Find.TickManager.TicksGame - completedQuestInfo.tickCompleted) < extension.ticksSinceFail.min || (Find.TickManager.TicksGame - completedQuestInfo.tickCompleted) > extension.ticksSinceFail.max)
                    {
                        return false;
                    }
                }
            }
            if (!extension.isRepeatable && quests.Any(x => x.questDef == quest) && futureQuests.Any(x => x.questDef == quest))
            {
                return false;
            }
            return true;
        }

        public bool TryGrantAgainOnFailure(QuestScriptDef quest)
        {
            if (CanGrantQuest(quest) is false)
            {
                return false;
            }
            var extension = quest.GetModExtension<QuestChainExtension>();
            if (extension == null)
            {
                return false; //If no extension, never grant again on failure
            }
            if (!extension.grantAgainOnFailure)
            {
                return false;
            }
            ScheduleQuest(quest, extension.daysUntilGrantAgainOnFailure.RandomInRange);
            return true;
        }

        public bool TryGrantAgainOnExpiry(QuestScriptDef quest)
        {
            if (CanGrantQuest(quest) is false)
            {
                return false;
            }
            var extension = quest.GetModExtension<QuestChainExtension>();
            if (extension == null)
            {
                return false; //If no extension, never grant again on expiry
            }
            if (!extension.grantAgainOnExpiry)
            {
                return false;
            }
            ScheduleQuest(quest, extension.daysUntilGrantAgainOnExpiry.RandomInRange);
            return true;
        }

        public void ScheduleQuest(QuestScriptDef quest, int ticksInFuture)
        {
            futureQuests.Add(new FutureQuestInfo
            {
                questDef = quest,
                tickToFire = Find.TickManager.TicksGame + ticksInFuture
            });
        }

        public void ScheduleQuest(QuestScriptDef quest, float mtbDays)
        {
            futureQuests.Add(new FutureQuestInfo
            {
                questDef = quest,
                mtbDays = mtbDays
            });
        }
    }

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