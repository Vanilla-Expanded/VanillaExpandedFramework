using HarmonyLib;
using RimWorld;
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
                if (Find.World.worldObjects.AllWorldObjects.Find(o => o.Tile == __instance.Tile && o.def.HasModExtension<CustomGenOption>()) is WorldObject worldObject)
                {
                    if (VFECore.VFEGlobal.settings.enableVerboseLogging) Log.Message($"Generating worldObject {worldObject.LabelCap}");
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
                    if (VFECore.VFEGlobal.settings.enableVerboseLogging) Log.Message($"Faction: {__instance.Faction.NameColored} - Generating");
                    if (__instance.Faction.def.HasModExtension<CustomGenOption>())
                    {
                        if (VFECore.VFEGlobal.settings.enableVerboseLogging) Log.Message($"Faction: {__instance.Faction.NameColored} - Faction have CustomGenOption extension");
                        __result = DefDatabase<MapGeneratorDef>.GetNamed("KCSG_Base_Faction");
                    }
                    else if (Find.World.worldObjects.AllWorldObjects.FindAll(o => o.Tile == __instance.Tile && o.def.HasModExtension<CustomGenOption>()).Any())
                    {
                        if (VFECore.VFEGlobal.settings.enableVerboseLogging) Log.Message($"Faction: {__instance.Faction.NameColored} - Faction do not have CustomGenOption extension");
                        __result = DefDatabase<MapGeneratorDef>.GetNamed("KCSG_WorldObject_Gen");
                    }
                }
            }
        }
    }
}