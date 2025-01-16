using LudeonTK;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace VFECore
{
    [HotSwappable]
    public class QuestChainsDevWindow : Window
    {
        private Vector2 scrollPosition = Vector2.zero;
        private float lastHeight;

        public override Vector2 InitialSize => new Vector2(800f, 600f);

        [DebugAction("General", null, false, false, false, false, 0, false, allowedGameStates
            = AllowedGameStates.PlayingOnMap, requiresIdeology = true, displayPriority = 1000)]
        public static void ViewQuestChains()
        {
            Find.WindowStack.Add(new QuestChainsDevWindow());
        }

        public QuestChainsDevWindow()
        {
            this.doCloseX = true;
            this.draggable = true;
            this.resizeable = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            GameComponent_QuestChains questChains = Current.Game.GetComponent<GameComponent_QuestChains>();
            if (questChains == null)
            {
                Widgets.Label(inRect, "Quest Chains component not found.");
                return;
            }

            Rect viewRect = new Rect(inRect.x, inRect.y, inRect.width - 20f, lastHeight);
            Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);

            float curY = 0f;

            // Active Quests
            Widgets.Label(new Rect(0, curY, viewRect.width, 30f), "Quests (" + questChains.quests.Count + ")");
            curY += 30f;
            foreach (QuestInfo questInfo in questChains.quests.ToList())
            {
                curY += DrawQuestInfo(new Rect(0, curY, viewRect.width, 0), questInfo);
            }

            // Future Quests
            Widgets.Label(new Rect(0, curY, viewRect.width, 30f), "Future Quests (" + questChains.futureQuests.Count + ")");
            curY += 30f;
            foreach (FutureQuestInfo futureQuestInfo in questChains.futureQuests.ToList())
            {
                curY += DrawFutureQuestInfo(new Rect(0, curY, viewRect.width, 0), futureQuestInfo);
            }

            lastHeight = curY;
            Widgets.EndScrollView();
        }

        private float DrawQuestInfo(Rect rect, QuestInfo questInfo)
        {
            QuestScriptDef questDef = questInfo.questDef;
            QuestChainExtension ext = questDef.GetModExtension<QuestChainExtension>();

            float curY = 0f;

            // Calculate button width and position
            float buttonWidth = 150f;
            float buttonX = rect.xMax - buttonWidth;

            // Draw quest name label
            Widgets.Label(new Rect(rect.x, rect.y + curY, rect.width - buttonWidth - 10f, 25f), "- " + questDef.defName + " (Chain: " + (ext?.questChainDef.label ?? "None") + ")");

            // Draw buttons if quest is ongoing
            if (questInfo.quest?.State == QuestState.Ongoing)
            {
                if (Widgets.ButtonText(new Rect(buttonX - buttonWidth - 10, rect.y + curY, buttonWidth, 25f), "Force Success"))
                {
                    questInfo.quest.End(QuestEndOutcome.Success, false);
                }

                buttonX = rect.xMax - buttonWidth; // Reset buttonX for the second button
                if (Widgets.ButtonText(new Rect(buttonX, rect.y, buttonWidth, 25f), "Force Fail"))
                {
                    questInfo.quest.End(QuestEndOutcome.Fail, false);
                }
            }
            curY += 25f; // Move curY down after quest name and buttons (if any)


            // Draw remaining quest information
            Widgets.Label(new Rect(rect.x + 20f, rect.y + curY, rect.width - 20f, 25f), "  - State: " + questInfo.quest.State);
            curY += 25f;
            Widgets.Label(new Rect(rect.x + 20f, rect.y + curY, rect.width - 20f, 25f), "  - Outcome: " + questInfo.outcome);
            curY += 25f;

            if (questInfo.tickAccepted > 0)
            {
                Widgets.Label(new Rect(rect.x + 20f, rect.y + curY, rect.width - 20f, 25f), "  - Accepted: " + GenDate.DateFullStringAt(GenDate.TickGameToAbs(questInfo.tickAccepted), default));
                curY += 25f;
            }

            if (questInfo.tickCompleted > 0)
            {
                Widgets.Label(new Rect(rect.x + 20f, rect.y + curY, rect.width - 20f, 25f), "  - Completed: " + GenDate.DateFullStringAt(GenDate.TickGameToAbs(questInfo.tickCompleted), default));
                curY += 25f;
            }

            if (questInfo.tickExpired > 0)
            {
                Widgets.Label(new Rect(rect.x + 20f, rect.y + curY, rect.width - 20f, 25f), "  - Expired: " + GenDate.DateFullStringAt(GenDate.TickGameToAbs(questInfo.tickExpired), default));
                curY += 25f;
            }

            return curY;
        }

        private float DrawFutureQuestInfo(Rect rect, FutureQuestInfo futureQuestInfo)
        {
            QuestScriptDef questDef = futureQuestInfo.questDef;
            QuestChainExtension ext = questDef.GetModExtension<QuestChainExtension>();

            float curY = 0f;

            // Calculate button width and position
            float buttonWidth = 150f;
            float buttonX = rect.xMax - buttonWidth;

            // Draw quest name label
            Widgets.Label(new Rect(rect.x, rect.y + curY, rect.width - buttonWidth - 10f, 25f), "- " + questDef.defName + " (Chain: " + (ext?.questChainDef.label ?? "None") + ")");

            // Draw "Fire Now" button
            if (Widgets.ButtonText(new Rect(buttonX, rect.y + curY, buttonWidth, 25f), "Fire Now"))
            {
                Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(questDef, StorytellerUtility.DefaultThreatPointsNow(Find.World));
                if (questDef.sendAvailableLetter)
                {
                    QuestUtility.SendLetterQuestAvailable(quest);
                }
                GameComponent_QuestChains.Instance.futureQuests.Remove(futureQuestInfo);
            }
            curY += 25f; // Move curY down after quest name and button

            // Draw remaining future quest information
            if (futureQuestInfo.tickToFire > 0)
            {
                int ticksUntilFire = futureQuestInfo.tickToFire - Find.TickManager.TicksGame;
                Widgets.Label(new Rect(rect.x + 20f, rect.y + curY, rect.width - 20f, 50f), "  - Fires in: " + GenDate.ToStringTicksToPeriod(ticksUntilFire) + " (at " + GenDate.DateFullStringAt(GenDate.TickGameToAbs(futureQuestInfo.tickToFire), default) + ")");
                curY += 50f;
            }
            else if (futureQuestInfo.mtbDays > 0)
            {
                Widgets.Label(new Rect(rect.x + 20f, rect.y + curY, rect.width - 20f, 25f), "  - MTB: " + futureQuestInfo.mtbDays.ToString() + " days");
                curY += 25f;
            }

            return curY;
        }
    }
}