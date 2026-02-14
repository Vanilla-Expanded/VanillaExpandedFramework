using HarmonyLib;
using RimWorld;
using Verse;


namespace VEF.Weapons
{
    [StaticConstructorOnStartup]
    internal static class VerbUtility
    {
        public static Pawn GetPawnAsHolder(this Thing thing)
        {
            var pawn = GetPawnAsHolderInt(thing);
            if (pawn?.carryTracker is not null) // we are filtering dummy outfit pawn here...
            {
                return pawn;
            }
            return pawn;
        }

        private static Pawn GetPawnAsHolderInt(Thing thing)
        {
            if (thing.ParentHolder is Pawn_EquipmentTracker pawn_EquipmentTracker) return pawn_EquipmentTracker.pawn;
            if (thing.ParentHolder is Pawn_ApparelTracker pawn_ApparelTracker) return pawn_ApparelTracker.pawn;
            return null;
        }

        public static float GetVerbRangeMultiplier(this Pawn pawn)
        {
            try
            {
                return pawn.GetStatValueForPawn(VEFDefOf.VEF_VerbRangeFactor, pawn);
            }
            catch
            {
                return 1f;
            }
        }

        [HarmonyPatch(typeof(VerbProperties), nameof(VerbProperties.AdjustedRange))]
        public static class VerbProperties_AdjustedRange_Patch
        {
            public static void Prefix(VerbProperties __instance, Verb ownerVerb, Thing attacker, out float __state)
            {
                // Use `__state` to store the original range value so we can restore it later in the finalizer. `NaN`
                // indicates that no adjustment was made.
                __state = float.NaN;

                if (__instance.Ranged && attacker is Pawn pawn)
                {
                    __state = __instance.range;
                    __instance.range *= pawn.GetVerbRangeMultiplier();
                }
            }

            public static void Finalizer(VerbProperties __instance, float __state)
            {
                if (!float.IsNaN(__state))
                {
                    __instance.range = __state;
                }
            }
        }
    }
}
