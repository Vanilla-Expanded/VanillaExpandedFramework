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
                if (Find.World.worldObjects.AllWorldObjects.Find(o => o.Tile == __instance.Tile && o.def.HasModExtension<CustomGenOption>()) is WorldObject wo)
                {
                    Debug.Message($"Generating worldObject {wo.LabelCap}");
                    var ext = wo.def.GetModExtension<CustomGenOption>();
                    __result = ext.preventBridgeable ? DefDatabase<MapGeneratorDef>.GetNamed("KCSG_WorldObject_Gen_NoBridge") : DefDatabase<MapGeneratorDef>.GetNamed("KCSG_WorldObject_Gen");
                }
            }
        }
    }
}
