using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace KCSG
{
    public class Postfix_Settlement_MapGeneratorDef
    {
        [HarmonyPatch(typeof(Settlement), nameof(Settlement.MapGeneratorDef), MethodType.Getter)]
        public static class Settlement_MapGeneratorDef_Patch
        {
            public static void Postfix(Settlement __instance, ref MapGeneratorDef __result)
            {
                if (__instance != null && __instance.Faction != null && __instance.Faction != Faction.OfPlayer)
                {
                    if (__instance.Faction.def.HasModExtension<CustomGenOption>())
                    {
                        var ext = __instance.Faction.def.GetModExtension<CustomGenOption>();
                        __result = ext.preventBridgeable ? DefDatabase<MapGeneratorDef>.GetNamed("KCSG_Base_Faction_NoBridge") : DefDatabase<MapGeneratorDef>.GetNamed("KCSG_Base_Faction");
                        Debug.Message($"Generating base for faction: {__instance.Faction.NameColored}. Skipping patchmaker: {ext.preventBridgeable}");
                    }
                    else if (Find.World.worldObjects.AllWorldObjects.Find(o => o.Tile == __instance.Tile && o.def.HasModExtension<CustomGenOption>()) is WorldObject wo)
                    {
                        var ext = wo.def.GetModExtension<CustomGenOption>();
                        __result = ext.preventBridgeable ? DefDatabase<MapGeneratorDef>.GetNamed("KCSG_WorldObject_NoBridge") : DefDatabase<MapGeneratorDef>.GetNamed("KCSG_WorldObject");
                        Debug.Message($"Generating world object map. Skipping patchmaker: {ext.preventBridgeable}");
                    }
                }
            }
        }
    }
}