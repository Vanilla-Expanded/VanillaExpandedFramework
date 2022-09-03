using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace KCSG
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(SettlementUtility))]
    [HarmonyPatch("AttackNow", MethodType.Normal)]
    public class Postfix_SettlementUtility_Attack
    {
        [HarmonyPostfix]
        public static void Prefix(Caravan caravan, Settlement settlement)
        {
            var faction = settlement.Faction;
            if (faction != null && faction.def.HasModExtension<CustomGenOption>())
            {
                LongEventHandler.ExecuteWhenFinished(() => GenOption.RotAllThing());
            }
        }
    }
}
