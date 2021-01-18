using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace VanillaStorytellersExpanded
{
    public enum Currency
    {
        None,
        Goodwill
    }
    public class QuestCurrency
    {
        public float costToAcceptQuest;
        public virtual Currency Currency => Currency.None;
        public virtual bool Allows(QuestGiverManager questGiverManager, Quest toCheck, Slate slate, out QuestInfo questInfo)
        {
            questInfo = null;
            return true;
        }
        public override string ToString()
        {
            return Currency + " - " + costToAcceptQuest;
        }
    }
    public class GoodwillCurrency : QuestCurrency
    {
        public int minimunGoodwillRequirement = -100;
        public override Currency Currency => Currency.Goodwill;
        public override bool Allows(QuestGiverManager questGiverManager, Quest quest, Slate slate, out QuestInfo questInfo)
        {
            var asker = slate.Get<Pawn>("asker");
            Log.Message($"Asker: {asker} - faction: {asker?.Faction} - slate: {slate}");
            if (asker?.Faction != null && asker.Faction.GoodwillWith(Faction.OfPlayer) >= minimunGoodwillRequirement)
            {
                var currencyInfo = new GoodwillCurrencyInfo();
                currencyInfo.currency = Currency.Goodwill;
                currencyInfo.amount = questGiverManager.def.currency.costToAcceptQuest;
                questInfo = new QuestInfo(quest, asker.Faction, currencyInfo, onlyOneChoice: questGiverManager.def.onlyOneReward ? true : false);
                return true;
            }
            questInfo = null;
            return false;
        }
    }

    public class CurrencyInfo : IExposable
    {
        public Currency currency;
        public float amount;
        public virtual void Buy(QuestInfo questInfo)
        {

        }
        public virtual string GetCurrencyInfo()
        {
            return "VSE.CostGoodwill".Translate(this.amount);
        } 

        public void ExposeData()
        {
            Scribe_Values.Look(ref currency, "currency");
            Scribe_Values.Look(ref amount, "amount");
        }
    }

    public class GoodwillCurrencyInfo : CurrencyInfo
    {
        public override void Buy(QuestInfo questInfo)
        {
            base.Buy(questInfo);
            questInfo.askerFaction.TryAffectGoodwillWith(Faction.OfPlayer, (int)amount);
        }
    }
}
