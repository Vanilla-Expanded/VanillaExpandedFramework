using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public static bool Prefix(Caravan caravan, Settlement settlement)
        {
            if (settlement.Faction != null && settlement.Faction.def.HasModExtension<FactionSettlement>() ||
                (Find.World.worldObjects.AllWorldObjects.Find(o => o.Tile == settlement.Tile && o.def.HasModExtension<FactionSettlement>()) is WorldObject worldObject))
            {
                if (!settlement.HasMap)
                {
                    CurrentGenerationOption.useCustomWindowContent = true;
                    CurrentGenerationOption.dateTime = DateTime.Now;
                    LongEventHandler.QueueLongEvent(delegate ()
                    {
                        CurrentGenerationOption.allTip = DefDatabase<TipSetDef>.AllDefsListForReading.SelectMany((TipSetDef set) => set.tips).InRandomOrder().ToList();
                        if (CurrentGenerationOption.allTip.Count > 0)
                        {
                            CurrentGenerationOption.tipAvailable = true;
                        }
                        AccessTools.Method(typeof(SettlementUtility), "AttackNow").Invoke(null, new object[] { caravan, settlement });
                        LongEventHandler.ExecuteWhenFinished(() =>
                        {
                            Log.Message($"Generation done in {(DateTime.Now - CurrentGenerationOption.dateTime).Duration().TotalSeconds}");
                            CurrentGenerationOption.ClearUI();
                            CurrentGenerationOption.ClearAll();
                            LongEventHandler_Patches.LongEventsOnGUI_Prefix.structure = null;
                        });
                    }, "GeneratingMapForNewEncounter", true, null, true);
                }
                else
                    AccessTools.Method(typeof(SettlementUtility), "AttackNow").Invoke(null, new object[] { caravan, settlement });
                return false;
            }
            return true;
        }
    }
}
