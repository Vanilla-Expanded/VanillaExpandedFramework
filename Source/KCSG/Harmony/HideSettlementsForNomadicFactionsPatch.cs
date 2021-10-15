using HarmonyLib;
using RimWorld.Planet;

namespace KCSG
{
    [HarmonyPatch(typeof(WorldObjectsHolder))]
    [HarmonyPatch("Add", MethodType.Normal)]
    public class HideSettlementsForNomadicFactionsPatch
    {
        public static bool Prefix(WorldObject o)
        {
            return (o?.Faction?.def.GetModExtension<CustomGenOption>()?.canSpawnSettlements == false) ? false : true;
        }
    }
}