using HarmonyLib;
using RimWorld.Planet;

namespace KCSG
{
    [HarmonyPatch(typeof(WorldObjectsHolder))]
    [HarmonyPatch("Add", MethodType.Normal)]
    public class Prefix_WorldObjectsHolder_Add
    {
        public static bool Prefix(WorldObject o)
        {
            return (o?.Faction?.def.GetModExtension<CustomGenOption>()?.canSpawnSettlements) != false;
        }
    }
}