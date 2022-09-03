using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace VanillaStorytellersExpanded
{

    public class QuestInfo : IExposable
    {
        public Quest quest;
        public int quest_Part_choiceInd = -1;
        public QuestPart_Choice quest_Part_choice;
        public QuestPart_Choice.Choice choice;
        public Faction askerFaction;
        public int tickGenerated;
        public QuestCurrencyInfo currencyInfo;

        public QuestInfo()
        {

        }

        public QuestInfo(Quest quest, Faction askerFaction, QuestCurrencyInfo currencyInfo, bool onlyOneChoice = false)
        {
            this.quest = quest;
            this.askerFaction = askerFaction;
            this.currencyInfo = currencyInfo;
            this.tickGenerated = Find.TickManager.TicksAbs;
            if (onlyOneChoice)
            {
                var choices = this.quest.PartsListForReading.Where(x => x is QuestPart_Choice choice).Cast<QuestPart_Choice>().ToList();
                if (choices.Any())
                {
                    quest_Part_choice = choices.RandomElement();
                    quest_Part_choiceInd = choices.IndexOf(quest_Part_choice);
                    choice = quest_Part_choice.choices.RandomElement();
                }
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref tickGenerated, "tickGenerated");
            Scribe_References.Look(ref askerFaction, "askerFaction");
            Scribe_Deep.Look(ref quest, "quest");
            Scribe_Deep.Look(ref choice, "choice");
            Scribe_Deep.Look(ref currencyInfo, "currencyInfo");
            Scribe_Values.Look(ref quest_Part_choiceInd, "quest_Part_choiceInd");
            if (quest_Part_choiceInd != -1 && Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                quest_Part_choice = this.quest.PartsListForReading.Where(x => x is QuestPart_Choice choice).Cast<QuestPart_Choice>().ToList()[quest_Part_choiceInd];
            }
        }
    }
}
