using HarmonyLib;
using RimWorld;
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
                    if (obj.Tile == tile && obj.def.HasModExtension<CustomGenOption>())
                    {
                        var ext = obj.def.GetModExtension<CustomGenOption>();
                        __result = ext.preventBridgeable ? DefDatabase<MapGeneratorDef>.GetNamed("KCSG_WorldObject_NoBridge") : DefDatabase<MapGeneratorDef>.GetNamed("KCSG_WorldObject");
                        return;
                    }
                    else if (obj is Site site)
                    {
                        foreach (var step in site.ExtraGenStepDefs)
                        {
                            if (step.def.genStep is GenStep_CustomStructureGen csg)
                            {
                                __result = csg.preventBridgeable ? DefDatabase<MapGeneratorDef>.GetNamed("KCSG_Encounter_NoBridge") : MapGeneratorDefOf.Encounter;
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}
