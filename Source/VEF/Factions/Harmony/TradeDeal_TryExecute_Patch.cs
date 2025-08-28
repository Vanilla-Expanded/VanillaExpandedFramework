using System.Text;
using VEF.Factions.GameComponents;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Factions;

[HarmonyPatch(typeof(TradeDeal), "TryExecute", [typeof(bool)], [ArgumentType.Out])]
public static class TradeDeal_TryExecute_Patch
{
    public class FoundContraband
    {
        public int count = 0;
        public ContrabandDef contrabandDef;
        public ChemicalDef chemical;
        public ThingDef thing; 
        public override string ToString()
        {
            return $"Found contraband {count} of {contrabandDef} : thing = {thing} | chemical = {chemical}";
        }
    }

    public static void Prefix(List<Tradeable> ___tradeables, out List<FoundContraband> __state)
    {
        __state = [];
        foreach (Tradeable tradeable in ___tradeables)
        {
            tradeable.ThingDef.GetContrabandMaterialCount(ref __state, tradeable.CountToTransferToDestination);
        }
    }

    /// <summary>
    /// Calculates the count of contraband materials for a given ThingDef and updates the contraband list.
    /// </summary>
    /// <param name="def">The ThingDef to check for contraband materials</param>
    /// <param name="contrabandList">List of contraband to update with material counts</param>
    /// <param name="countToTransfer">Number of items being transferred</param>
    public static void GetContrabandMaterialCount(this ThingDef def, ref List<FoundContraband> contrabandList, int countToTransfer)
    {
        if (def == null) return;

        foreach (ContrabandDef contrabandDef in DefDatabase<ContrabandDef>.AllDefs)
        {
            if (!contrabandDef.IsThingDefContraband(def, out int count, out ThingDef thingDef, out ChemicalDef chemicalDef))
                continue;

            var foundContraband = contrabandList.FirstOrFallback(
                c => c.contrabandDef == contrabandDef && c.thing == thingDef && c.chemical == chemicalDef,
                null);

            if (foundContraband == null)
            {
                foundContraband = new FoundContraband
                {
                    contrabandDef = contrabandDef,
                    thing = thingDef,
                    chemical = chemicalDef
                };
                contrabandList.Add(foundContraband);
            }

            foundContraband.count += count * countToTransfer;
        }
    }

    public static List<TaggedString> GetContrabandWarningMessages(this ThingDef def, bool isGifting)
    {
        List<TaggedString> messages = new List<TaggedString>();
        if (def == null) return messages;

        foreach (ContrabandDef contrabandDef in DefDatabase<ContrabandDef>.AllDefs)
        {
            if (!contrabandDef.IsThingDefContraband(def, out int count, out ThingDef thingDef, out ChemicalDef chemicalDef))
                continue;

            Def contrabandThingDef = thingDef != null ? thingDef : chemicalDef;
            bool isTradingWithIllegalFaction = !contrabandDef.illegalFactions.NullOrEmpty() && 
                                               contrabandDef.illegalFactions.Contains(TradeSession.trader.Faction.def);

            string warningKey = "";

            if (contrabandDef.illegalFactions.NullOrEmpty())
            {
                warningKey = isGifting ? contrabandDef.giftWarningKey : contrabandDef.sellWarningKey;
            }
            else if (isTradingWithIllegalFaction)
            {
                warningKey = isGifting ? contrabandDef.giftIllegalFactionWarningKey : contrabandDef.sellIllegalWarningKey;
            }
            
            foreach (FactionDef faction in contrabandDef.factions)
            {
                messages.Add(warningKey.Translate(
                    contrabandThingDef.Named("ILLEGALTHING"),
                    faction.Named("FACTION"),
                    TradeSession.trader.Faction.Named("ILLEGALFACTION")));
            }
        }

        return messages;
    }

    public static void Postfix(List<FoundContraband> __state, bool __result)
    {
        if (__state.NullOrEmpty() || !__result) return;

        foreach (FoundContraband contraband in __state)
        {
            bool isContrabandForTraderFaction = contraband.contrabandDef.factions.Contains(TradeSession.trader.Faction.def);
            if (!isContrabandForTraderFaction && Rand.Chance(contraband.contrabandDef.chanceToGetCaught))
            {
                ProcessCaughtContraband(contraband);
            }
        }
    }

    private static void ProcessCaughtContraband(FoundContraband contraband)
    {
        // get the faction that will discover the contraband exchange
        FactionDef randomFactionDef = contraband.contrabandDef.factions.RandomElement();
        Faction affectedFaction = Find.FactionManager.FirstFactionOfDef(randomFactionDef);
        
        Def contrabandThingDef = contraband.thing != null ? contraband.thing : contraband.chemical;
        
        bool tradedToIllegalFaction = contraband.contrabandDef.illegalFactions.Contains(TradeSession.trader.Faction.def);
        HistoryEventDef historyEvent = TradeSession.giftMode
            ? contraband.contrabandDef.giftedHistoryEvent
            : contraband.contrabandDef.soldHistoryEvent;
            
        string letterLabel = contraband.contrabandDef.letterLabelKey.Translate(affectedFaction.Named("FACTION"));
        string letterDesc = GenerateLetterDescription(contraband, affectedFaction, contrabandThingDef, tradedToIllegalFaction);
        
        int goodwillImpact = Mathf.RoundToInt(contraband.contrabandDef.impactMultiplier * contraband.count);
        
        Find.World.GetComponent<WorldComponent_FactionGoodwillImpactManager>().ImpactFactionGoodwill(
            new GoodwillImpactDelayed
            {
                factionToImpact = affectedFaction,
                goodwillImpact = goodwillImpact,
                historyEvent = historyEvent,
                impactInTicks = contraband.contrabandDef.ImpactInTicks,
                letterLabel = letterLabel,
                letterDesc = letterDesc,
                relationInfoKey = contraband.contrabandDef.relationInfoKey
            });
    }

    private static string GenerateLetterDescription(FoundContraband contraband, Faction affectedFaction, 
                                                    Def contrabandThingDef, bool tradedToIllegalFaction)
    {
        if (!tradedToIllegalFaction)
        {
            return TradeSession.giftMode
                ? contraband.contrabandDef.letterDescGiftKey.Translate(
                    affectedFaction.Named("FACTION"),
                    contrabandThingDef.Named("ILLEGALTHING"))
                : contraband.contrabandDef.letterDescSoldKey.Translate(
                    affectedFaction.Named("FACTION"),
                    contrabandThingDef.Named("ILLEGALTHING"));
        }
        else
        {
            return TradeSession.giftMode
                ? contraband.contrabandDef.letterDescGifIllegalFactionKey.Translate(
                    affectedFaction.Named("FACTION"),
                    contrabandThingDef.Named("ILLEGALTHING"), 
                    TradeSession.trader.Faction.Named("ILLEGALFACTION"))
                : contraband.contrabandDef.letterDescSoldIllegalFactionKey.Translate(
                    affectedFaction.Named("FACTION"),
                    contrabandThingDef.Named("ILLEGALTHING"), 
                    TradeSession.trader.Faction.Named("ILLEGALFACTION"));
        }
    }
}