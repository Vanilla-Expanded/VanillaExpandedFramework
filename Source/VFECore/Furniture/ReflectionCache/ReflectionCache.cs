using HarmonyLib;
using RimWorld;
using Verse;



namespace VanillaFurnitureExpanded
{
    public class ReflectionCache
    {
        public static readonly AccessTools.FieldRef<Thing, Graphic> buildingGraphic =
           AccessTools.FieldRefAccess<Thing, Graphic>(AccessTools.Field(typeof(Thing), "graphicInt"));
    }
}
