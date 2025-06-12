using HarmonyLib;
using RimWorld;
using Verse;



namespace VEF.Weapons
{
    public class ReflectionCache
    {
        public static readonly AccessTools.FieldRef<Thing, Graphic> weaponGraphic =
           AccessTools.FieldRefAccess<Thing, Graphic>(AccessTools.Field(typeof(Thing), "graphicInt"));
    }
}
