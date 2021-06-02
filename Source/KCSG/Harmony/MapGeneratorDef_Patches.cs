using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace KCSG
{
    public class MapGeneratorDef_Patches
    {
        [HarmonyPatch(typeof(MapParent), nameof(MapParent.MapGeneratorDef), MethodType.Getter)]
        public static class MapParent_MapGeneratorDef_Patch
        {
            public static void Postfix(MapParent __instance, ref MapGeneratorDef __result)
            {
                if (__instance.Faction != null && __instance.Faction.def.HasModExtension<FactionSettlement>())
                {
                    __result = DefDatabase<MapGeneratorDef>.GetNamed("KCSG_Base_Faction");
                }
            }
        }

        [HarmonyPatch(typeof(Settlement), nameof(Settlement.MapGeneratorDef), MethodType.Getter)]
        public static class Settlement_MapGeneratorDef_Patch
        {
            public static void Postfix(Settlement __instance, ref MapGeneratorDef __result)
            {
                if (__instance.Faction != null && __instance.Faction.def.HasModExtension<FactionSettlement>())
                {
                    __result = DefDatabase<MapGeneratorDef>.GetNamed("KCSG_Base_Faction");
                }
            }
        }
    }
}