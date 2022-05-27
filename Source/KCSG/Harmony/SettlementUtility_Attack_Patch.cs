namespace KCSG
{
    /*[StaticConstructorOnStartup]
    [HarmonyPatch(typeof(SettlementUtility))]
    [HarmonyPatch("Attack", MethodType.Normal)]
    public class SettlementUtility_Attack_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(Caravan caravan, Settlement settlement)
        {
            if (settlement.Faction != null && settlement.Faction.def.HasModExtension<CustomGenOption>() ||
                (Find.World.worldObjects.AllWorldObjects.Find(o => o.Tile == settlement.Tile && o.def.HasModExtension<CustomGenOption>()) is WorldObject worldObject))
            {
                if (!settlement.HasMap)
                {
                    GenOption.useCustomWindowContent = true;
                    GenOption.dateTime = DateTime.Now;
                    LongEventHandler.QueueLongEvent(delegate ()
                    {
                        GenOption.allTip = DefDatabase<TipSetDef>.AllDefsListForReading.SelectMany((TipSetDef set) => set.tips).InRandomOrder().ToList();
                        if (GenOption.allTip.Count > 0)
                        {
                            GenOption.tipAvailable = true;
                        }
                        CustomAttackNowNoLetter(caravan, settlement);
                        LongEventHandler.ExecuteWhenFinished(() =>
                        {
                            KLog.Message($"Generation done in {(DateTime.Now - GenOption.dateTime).Duration().TotalSeconds}");
                            // Send letter
                            if (settlement.Faction != null) SendAttackLetter(caravan, settlement);
                            // Clear
                            GenOption.ClearUI();
                            GenOption.ClearAll();
                            LongEventHandler_Patches.LongEventsOnGUI_Prefix.structure = null;
                        });
                    }, "GeneratingMapForNewEncounter", true, delegate (Exception e)
                    {
                        Log.Error($"{e}");
                        GenOption.ClearUI();
                        GenOption.ClearAll();
                    }, true);
                }
                else
                    AccessTools.Method(typeof(SettlementUtility), "AttackNow").Invoke(null, new object[] { caravan, settlement });
                return false;
            }
            return true;
        }

        private static void CustomAttackNowNoLetter(Caravan caravan, Settlement settlement)
        {
            Map map = GetOrGenerateMapUtility.GetOrGenerateMap(settlement.Tile, null);
            if (!settlement.HasMap)
            {
                Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
            }
            CaravanEnterMapUtility.Enter(caravan, map, CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, true, null);
            Find.GoodwillSituationManager.RecalculateAll(true);
        }

        private static void SendAttackLetter(Caravan caravan, Settlement settlement)
        {
            TaggedString letterLabel = "LetterLabelCaravanEnteredEnemyBase".Translate();
            TaggedString letterText = "LetterCaravanEnteredEnemyBase".Translate(caravan.Label, settlement.Label.ApplyTag(TagType.Settlement, settlement.Faction.GetUniqueLoadID())).CapitalizeFirst();
            SettlementUtility.AffectRelationsOnAttacked(settlement, ref letterText);
            if (settlement.HasMap) PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(settlement.Map.mapPawns.AllPawns, ref letterLabel, ref letterText, "LetterRelatedPawnsSettlement".Translate(Faction.OfPlayer.def.pawnsPlural), true);
            Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent, caravan.PawnsListForReading, settlement.Faction, null, null, null);
        }
    }*/
}