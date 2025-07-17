using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using static RimWorld.FleshTypeDef;


namespace VEF.Genes
{
    public class ReflectionCache 
    {
        public static readonly AccessTools.FieldRef<FleshTypeDef,List<ResolvedWound>> woundsResolved =
           AccessTools.FieldRefAccess<FleshTypeDef,List<ResolvedWound>>(AccessTools.Field(typeof(FleshTypeDef), "woundsResolved"));

        public static readonly Action<Pawn_GeneTracker> checkForOverrides = (Action < Pawn_GeneTracker > )Delegate.CreateDelegate(typeof(Action<Pawn_GeneTracker>),
            AccessTools.Method(typeof(Pawn_GeneTracker), "CheckForOverrides"));
    }
}
