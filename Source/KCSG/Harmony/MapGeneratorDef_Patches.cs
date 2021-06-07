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
                if (Find.World.worldObjects.AllWorldObjects.Find(o => o.Tile == __instance.Tile && o.def.HasModExtension<FactionSettlement>()) is WorldObject worldObject)
                {
                    __result = DefDatabase<MapGeneratorDef>.GetNamed("KCSG_WorldObject_Gen");
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
                    Log.Message($"Faction: {__instance.Faction.NameColored} - Faction have FactionSettlement extension.");
                    __result = DefDatabase<MapGeneratorDef>.GetNamed("KCSG_Base_Faction");
                }
                else if (Find.World.worldObjects.AllWorldObjects.Find(o => o.Tile == __instance.Tile && o.def.HasModExtension<FactionSettlement>()) is WorldObject worldObject)
                {
                    Log.Message($"Faction: {__instance.Faction.NameColored} - Faction do not have FactionSettlement extension.");
                    __result = DefDatabase<MapGeneratorDef>.GetNamed("KCSG_WorldObject_Gen");
                }
            }
        }
    }
}