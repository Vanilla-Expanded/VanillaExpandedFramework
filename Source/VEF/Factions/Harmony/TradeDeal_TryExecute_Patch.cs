using System.Text;
using VEF.Factions.GameComponents;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
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
    /// Calculates the count of contraband materials for a given ThingDef and updates the trade state accordingly.
    /// </summary>
    /// <param name="def">The ThingDef to check for contraband materials</param>
    /// <param name="state">List of trade states to update with contraband material counts</param>
    /// <param name="countToTransferToDestination">Number of items being transferred</param>
    public static void GetContrabandMaterialCount(this ThingDef def, ref List<FoundContraband> state,
        int countToTransferToDestination)
    {
        if (def != null)
        {
            foreach (ContrabandDef contrabandDef in DefDatabase<ContrabandDef>.AllDefs)
            {
                if (contrabandDef.IsThingDefContraband(def, out int count, out ThingDef thingDef, out var chemicalDef))
                {
                    var ts = state.FirstOrFallback(
                        t => t.contrabandDef == contrabandDef && t.thing == thingDef && t.chemical == chemicalDef,
                        null);
                    if (ts == null)
                    {
                        ts = new FoundContraband();
                        ts.contrabandDef = contrabandDef;
                        ts.thing = thingDef;
                        ts.chemical = chemicalDef;
                        state.Add(ts);
                    }

                    ts.count += count * countToTransferToDestination;
                }
            }
        }
    }

    public static List<TaggedString> GetGiftingMaterialMessages(this ThingDef def)
    {
        List<TaggedString> messages = new List<TaggedString>();

        if (def != null)
        {
            foreach (ContrabandDef contrabandDef in DefDatabase<ContrabandDef>.AllDefs)
            {
                if (contrabandDef.IsThingDefContraband(def, out int count, out ThingDef thingDef, out var chemicalDef))
                {
                    Def contrabandThingDef = thingDef != null ? thingDef : chemicalDef;
                    foreach (FactionDef faction in contrabandDef.factions)
                    {
                        messages.Add(contrabandDef.giftWarning.Translate(contrabandThingDef.Named("ILLEGALTHING"),
                            faction.Named("FACTION")));   
                    }
                }
            }
        }

        return messages;
    }

    public static List<TaggedString> GetSellingMaterialMessages(this ThingDef def)
    {
        List<TaggedString> messages = new List<TaggedString>();

        if (def != null)
        {
            foreach (ContrabandDef contrabandDef in DefDatabase<ContrabandDef>.AllDefs)
            {
                if (contrabandDef.IsThingDefContraband(def, out int count, out ThingDef thingDef, out ChemicalDef chemicalDef))
                {
                    Def contrabandThingDef = thingDef != null ? thingDef : chemicalDef;
                    foreach (Faction faction in Find.FactionManager.AllFactions.Where(f=>contrabandDef.factions.Contains(f.def)))
                    {
                        messages.Add(contrabandDef.sellWarning.Translate(contrabandThingDef.Named("ILLEGALTHING"),
                            faction.Named("FACTION")));
                    }
                }
            }
        }

        return messages;
    }

    public static void Postfix(List<FoundContraband> __state, bool __result)
    {
        if (!__state.NullOrEmpty() && __result)
        {
            foreach (FoundContraband tradeState in __state)
            {
                if (!tradeState.contrabandDef.factions.Contains(TradeSession.trader.Faction.def) &&
                    Rand.Chance(tradeState.contrabandDef.chanceToGetCaught))
                {
                    FactionDef rand = tradeState.contrabandDef.factions.RandomElement();
                    Faction faction = Find.FactionManager.FirstFactionOfDef(rand);
                    Def contrabandThingDef = tradeState.thing != null ? tradeState.thing : tradeState.chemical;

                    Current.Game.GetComponent<GameComponent_FactionGoodwillImpactManager>().goodwillImpacts.Add(
                        new GoodwillImpactDelayed
                        {
                            factionToImpact = faction,
                            goodwillImpact = -tradeState.count,
                            historyEvent = TradeSession.giftMode
                                ? tradeState.contrabandDef.giftedHistoryEvent
                                : tradeState.contrabandDef.soldHistoryEvent,
                            impactInTicks = Find.TickManager.TicksGame +
                                            (int)(GenDate.TicksPerDay * Rand.Range(7f, 14f)),
                            letterLabel =
                                tradeState.contrabandDef.letterLabel.Translate(faction.Named("FACTION")),
                            letterDesc = TradeSession.giftMode
                                ? tradeState.contrabandDef.letterDescGift.Translate(
                                    faction.Named("FACTION"),
                                    contrabandThingDef.Named("ILLEGALTHING"))
                                : tradeState.contrabandDef.letterDescSold.Translate(
                                    faction.Named("FACTION"),
                                    contrabandThingDef.Named("ILLEGALTHING")),
                            relationInfoKey = tradeState.contrabandDef.relationInfo
                        });
                }
            }
        }
    }
}