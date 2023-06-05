using HarmonyLib;
using RimWorld;
using Verse;

namespace KCSG
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(GenStep_Power))]
    [HarmonyPatch("Generate", MethodType.Normal)]
    public class Prefix_GenStep_Power
    {
        [HarmonyPrefix]
        public static bool Prefix(Map map)
        {
            if (Find.World.worldObjects.AnySettlementAt(map.Tile) && map.ParentFaction is Faction faction && faction.def.HasModExtension<CustomGenOption>())
            {
                return false;
            }
            return true;
        }
    }
}