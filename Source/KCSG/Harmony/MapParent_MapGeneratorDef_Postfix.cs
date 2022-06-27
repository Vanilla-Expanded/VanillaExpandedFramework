using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace KCSG
{
    public class MapParent_MapGeneratorDef_Postfix
    {
        [HarmonyPatch(typeof(MapParent), nameof(MapParent.MapGeneratorDef), MethodType.Getter)]
        public static class MapParent_MapGeneratorDef_Patch
        {
            public static void Postfix(MapParent __instance, ref MapGeneratorDef __result)
            {
                if (Find.World.worldObjects.AllWorldObjects.Find(o => o.Tile == __instance.Tile && o.def.HasModExtension<CustomGenOption>()) is WorldObject worldObject)
                {
                    Debug.Message($"Generating worldObject {worldObject.LabelCap}");
                    __result = DefDatabase<MapGeneratorDef>.GetNamed("KCSG_WorldObject_Gen");
                }
            }
        }
    }
}
