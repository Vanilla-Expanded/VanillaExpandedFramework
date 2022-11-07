using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using static RimWorld.FleshTypeDef;


namespace VanillaGenesExpanded
{
    public class ReflectionCache 
    {
        public static readonly AccessTools.FieldRef<FleshTypeDef,List<ResolvedWound>> woundsResolved =
           AccessTools.FieldRefAccess<FleshTypeDef,List<ResolvedWound>>(AccessTools.Field(typeof(FleshTypeDef), "woundsResolved"));
    }
}
