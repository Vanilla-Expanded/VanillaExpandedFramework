using VEF.Factions.GameComponents;
using System.Collections.Generic;
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
            // For debugging
            return $"Found contraband {count} of {contrabandDef} : thing = {thing} | chemical = {chemical}";
        }
    }

    public static void Prefix(List<Tradeable> ___tradeables, out List<FoundContraband> __state)
    {
        __state = [];
        foreach (Tradeable tradeable in ___tradeables)
        {
            // check all the tradables for contraband
            tradeable.AnyThing.GetContrabandMaterialCount(ref __state, tradeable.CountToTransferToDestination);
        }
    }

    /// <summary>
    /// Calculates the count of contraband materials for a given ThingDef and updates the contraband list.
    /// </summary>
    /// <param name="def">The ThingDef to check for contraband materials</param>
    /// <param name="contrabandList">List of contraband to update with material counts</param>
    /// <param name="countToTransfer">Number of items being transferred</param>
    public static void GetContrabandMaterialCount(this Thing thing, ref List<FoundContraband> contrabandList, int countToTransfer)
    {
        if (thing?.def == null) return;

        foreach (ContrabandDef contrabandDef in DefDatabase<ContrabandDef>.AllDefs)
        {
            if (!contrabandDef.IsThingContraband(thing, out int count, out ThingDef thingDef, out ChemicalDef chemicalDef))
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

    /// <summary>
    /// Builds the warning message based on the thing or chemical, and if it's gifted or sold
    /// </summary>
    /// <param name="thing">The relevant contraband</param>
    /// <param name="isGifting">Wether the operation is a gift or sale</param>
    /// <returns>The list of messages to show</returns>
    public static List<TaggedString> GetContrabandWarningMessages(this Thing thing, bool isGifting)
    {
        List<TaggedString> messages = new List<TaggedString>();
        if (thing?.def == null) return messages;

        foreach (ContrabandDef contrabandDef in DefDatabase<ContrabandDef>.AllDefs)
        {
            if (!contrabandDef.IsThingContraband(thing, out int count, out ThingDef thingDef, out ChemicalDef chemicalDef))
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
            
            foreach (FactionDef factionDef in contrabandDef.factions)
            {
                Faction faction = Find.FactionManager.FirstFactionOfDef(factionDef);
                if (faction == null) continue;
                var str = faction.Named("FACTION");
                messages.Add(warningKey.Translate(
                    contrabandThingDef.Named("ILLEGALTHING"),
                    str,
                    TradeSession.trader.Faction.Named("ILLEGALFACTION")));
            }
        }

        return messages;
    }

    public static void Postfix(List<FoundContraband> __state, bool __result)
    {
        if (__state.NullOrEmpty() || !__result) return;

        // Check through the contraband and process
        foreach (FoundContraband contraband in __state)
        {
            bool isContrabandForTraderFaction = contraband.contrabandDef.factions.Contains(TradeSession.trader.Faction.def);
            if (!isContrabandForTraderFaction && Rand.Chance(contraband.contrabandDef.chanceToGetCaught))
            {
                ProcessCaughtContraband(contraband);
            }
        }
    }

    /// <summary>
    /// Processes contraband that has been discovered during a trade, generating appropriate faction impacts and notifications.
    /// </summary>
    /// <param name="contraband">The contraband details</param>
    private static void ProcessCaughtContraband(FoundContraband contraband)
    {
        // get the faction that will discover the contraband exchange
        FactionDef randomFactionDef = contraband.contrabandDef.factions.RandomElement();
        Faction affectedFaction = Find.FactionManager.FirstFactionOfDef(randomFactionDef);
        
        Def contrabandThingDef = contraband.thing != null ? contraband.thing : contraband.chemical;
        
        bool tradedToIllegalFaction = contraband.contrabandDef.illegalFactions?.Contains(TradeSession.trader.Faction.def) ?? false;
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
                relationInfoKey = contraband.contrabandDef.relationInfoKey,
                letterType = contraband.contrabandDef.letterType
            });
    }

    /// <summary>
    /// Generates the letter description for the contraband
    /// </summary>
    /// <param name="contraband"></param>
    /// <param name="affectedFaction">The faction that finds it illegal</param>
    /// <param name="contrabandThingDef">The contraband thing</param>
    /// <param name="tradedToIllegalFaction">If the trade is to an illegal faction</param>
    /// <returns></returns>
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