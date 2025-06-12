using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;



namespace VEF.Buildings
{
    public class ReflectionCache
    {
        public static readonly AccessTools.FieldRef<Thing, Graphic> buildingGraphic =
           AccessTools.FieldRefAccess<Thing, Graphic>(AccessTools.Field(typeof(Thing), "graphicInt"));

        public static readonly AccessTools.FieldRef<ThingDef, List<RecipeDef>> ThingDef_allRecipesCached =
           AccessTools.FieldRefAccess<ThingDef, List<RecipeDef>>(AccessTools.Field(typeof(ThingDef), "allRecipesCached"));
    }
}
