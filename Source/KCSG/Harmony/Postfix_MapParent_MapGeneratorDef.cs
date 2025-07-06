using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace KCSG
{
    public class Postfix_MapParent_MapGeneratorDef
    {
        [HarmonyPatch(typeof(MapParent), nameof(MapParent.MapGeneratorDef), MethodType.Getter)]
        public static class MapParent_MapGeneratorDef_Patch
        {
            public static void Postfix(MapParent __instance, ref MapGeneratorDef __result)
            {
                var tile = __instance.Tile;
                var worldObjects = Find.World.worldObjects.AllWorldObjects;

                for (int i = 0; i < worldObjects.Count; i++)
                {
                    var obj = worldObjects[i];
                    // If we found __instance worldObject
                    if (obj.Tile == tile)
                    {
                        // If it has the extension, always modify it's MapGeneratorDef
                        if (obj.def.GetModExtension<CustomGenOption>() is CustomGenOption ext)
                        {
                            __result = DefDatabase<MapGeneratorDef>.GetNamed("KCSG_WorldObject");
                            return;
                        }
                    }
                }
            }
        }
    }
}
