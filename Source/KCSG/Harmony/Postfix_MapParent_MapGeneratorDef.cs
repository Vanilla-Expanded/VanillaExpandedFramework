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
                            __result = ext.preventBridgeable ? DefDatabase<MapGeneratorDef>.GetNamed("KCSG_WorldObject_NoBridge") : DefDatabase<MapGeneratorDef>.GetNamed("KCSG_WorldObject");
                            return;
                        }
                        // If it don't but is a site, check ExtraGenStepDefs
                        else if (obj is Site site)
                        {
                            foreach (var step in site.ExtraGenStepDefs)
                            {
                                // If one genstep is GenStep_CustomStructureGen and it should prevent bridgeable, return custom MapGeneratorDef
                                if (step.def.genStep is GenStep_CustomStructureGen csg && csg.preventBridgeable)
                                {
                                    __result = DefDatabase<MapGeneratorDef>.GetNamed("KCSG_Encounter_NoBridge");
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
