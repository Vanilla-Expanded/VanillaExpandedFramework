using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace KCSG
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(SettlementUtility))]
    [HarmonyPatch("Attack", MethodType.Normal)]
    public class SettlementUtility_Attack_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(Caravan caravan, Settlement settlement, SettlementUtility __instance)
        {
            if (settlement.Faction != null && settlement.Faction.def.HasModExtension<FactionSettlement>())
            {
                if (!settlement.HasMap)
                {
                    CurrentGenerationOption.useCustomWindowContent = true;
                    CurrentGenerationOption.dateTime = DateTime.Now;
                    LongEventHandler.QueueLongEvent(delegate ()
                    {
                        // __instance.GetType().GetMethod("AttackNow").Invoke(__instance, new object[] { caravan, settlement });
                        CurrentGenerationOption.allTip = DefDatabase<TipSetDef>.AllDefsListForReading.SelectMany((TipSetDef set) => set.tips).InRandomOrder().ToList();
                        if (CurrentGenerationOption.allTip.Count > 0)
                        {
                            CurrentGenerationOption.tipAvailable = true;
                        }
                        AttackNow(caravan, settlement);
                        LongEventHandler.ExecuteWhenFinished(() =>
                        {
                            Log.Message($"Generation done in {(DateTime.Now - CurrentGenerationOption.dateTime).Duration().TotalSeconds}");
                            CurrentGenerationOption.ClearUI();
                            CurrentGenerationOption.ClearAll();
                        });
                    }, "GeneratingMapForNewEncounter", true, null, true);
                }
                else 
                    AttackNow(caravan, settlement);
                return false;
            }
            return true;
        }

        private static void AttackNow(Caravan caravan, Settlement settlement)
        {
            bool flag = !settlement.HasMap;
            Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(settlement.Tile, null);
            TaggedString label = "LetterLabelCaravanEnteredEnemyBase".Translate();
            TaggedString text = "LetterCaravanEnteredEnemyBase".Translate(caravan.Label, settlement.Label.ApplyTag(TagType.Settlement, settlement.Faction.GetUniqueLoadID())).CapitalizeFirst();
            SettlementUtility.AffectRelationsOnAttacked_NewTmp(settlement, ref text);
            if (flag)
            {
                Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
                PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(orGenerateMap.mapPawns.AllPawns, ref label, ref text, "LetterRelatedPawnsSettlement".Translate(Faction.OfPlayer.def.pawnsPlural), true, true);
            }
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, caravan.PawnsListForReading, settlement.Faction, null, null, null);
            CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, true, null);
        }
    }
}
