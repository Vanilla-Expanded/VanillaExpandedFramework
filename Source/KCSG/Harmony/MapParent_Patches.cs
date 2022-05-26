using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace KCSG
{
    public class MapParent_Patches
    {
        [HarmonyPatch(typeof(MapParent), nameof(MapParent.MapGeneratorDef), MethodType.Getter)]
        public static class MapParent_MapGeneratorDef_Patch
        {
            public static void Postfix(MapParent __instance, ref MapGeneratorDef __result)
            {
                if (Find.World.worldObjects.AllWorldObjects.Find(o => o.Tile == __instance.Tile && o.def.HasModExtension<CustomGenOption>()) is WorldObject worldObject)
                {
                    KLog.Message($"Generating worldObject {worldObject.LabelCap}");
                    __result = DefDatabase<MapGeneratorDef>.GetNamed("KCSG_WorldObject_Gen");
                }
            }
        }

        [HarmonyPatch(typeof(Settlement), nameof(Settlement.MapGeneratorDef), MethodType.Getter)]
        public static class Settlement_MapGeneratorDef_Patch
        {
            public static void Postfix(Settlement __instance, ref MapGeneratorDef __result)
            {
                if (__instance != null && __instance.Faction != null && __instance.Faction != Faction.OfPlayer)
                {
                    KLog.Message($"Faction: {__instance.Faction.NameColored} - Generating");
                    if (__instance.Faction.def.HasModExtension<CustomGenOption>())
                    {
                        KLog.Message($"Faction: {__instance.Faction.NameColored} - Faction have CustomGenOption extension");
                        __result = DefDatabase<MapGeneratorDef>.GetNamed("KCSG_Base_Faction");
                    }
                    else if (Find.World.worldObjects.AllWorldObjects.FindAll(o => o.Tile == __instance.Tile && o.def.HasModExtension<CustomGenOption>()).Any())
                    {
                        KLog.Message($"Generating world object map");
                        __result = DefDatabase<MapGeneratorDef>.GetNamed("KCSG_WorldObject_Gen");
                    }
                }
            }
        }
    }
}