using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;



namespace AnimalBehaviours
{
    public class ReflectionCache
    {
        public static readonly AccessTools.FieldRef<Pawn, Pawn_DrawTracker> drawer =
           AccessTools.FieldRefAccess<Pawn, Pawn_DrawTracker>(AccessTools.Field(typeof(Pawn), "drawer"));
    }
}
