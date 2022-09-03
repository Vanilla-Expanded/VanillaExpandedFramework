using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace VanillaStorytellersExpanded
{
    public class QuestGiverManager : IExposable
    {
        public QuestGiverDef def;
        private List<QuestInfo> availableQuests = new List<QuestInfo>();
        public Faction FixedQuestGiverFaction => def.fixedQuestGiverFaction != null
            ? Find.FactionManager.FirstFactionOfDef(def.fixedQuestGiverFaction) : Find.FactionManager.RandomAlliedFaction();
        public List<QuestInfo> AvailableQuests
        {
            get
            {
                if (availableQuests is null)
                {
                    availableQuests = new List<QuestInfo>();
                }
                availableQuests.RemoveAll(x => x is null || x.askerFaction is null || x.quest_Part_choice is null || x.choice is null);
                return availableQuests;
            }
        }
            
        private int lastResetTick;
        public QuestGiverManager()
        {

        }

        public QuestGiverManager(QuestGiverDef def)
        {
            this.def = def;
            this.availableQuests = new List<QuestInfo>();
        }
        public void Tick()
        {
            if (def.resetEveryTick != -1 && Find.TickManager.TicksAbs > lastResetTick + def.resetEveryTick)
            {
                Reset();
            }
        }

        public void Init()
        {
            GenerateQuests();
        }
        public void Reset()
        {
            availableQuests.Clear();
            GenerateQuests();
            lastResetTick = Find.TickManager.TicksAbs;
        }

        public void GenerateQuests()
        {
            availableQuests.AddRange(def.Worker.GenerateQuests(this));
        }

        public void ActivateQuest(Pawn accepter, QuestInfo questInfo)
        {
            Find.QuestManager.Add(questInfo.quest);
            questInfo.quest.Accept(accepter);
            QuestUtility.SendLetterQuestAvailable(questInfo.quest);
            questInfo.currencyInfo?.Buy(questInfo);
            availableQuests.Remove(questInfo);
        }

        public void CallWindow()
        {
            var window = (Window)Activator.CreateInstance(this.def.windowClass, this);
            Find.WindowStack.Add(window);
        }
        public void ExposeData()
        {
            Scribe_Collections.Look(ref availableQuests, "availableQuests", LookMode.Deep);
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref lastResetTick, "lastResetTick");
        }
    }
}
