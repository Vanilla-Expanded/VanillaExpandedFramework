using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using RimWorld.Planet;

namespace VEF
{

    [StaticConstructorOnStartup]
    public static class NonPublicFields
    {
        public static readonly AccessTools.FieldRef<IntVec3> SiegeBlueprintPlacer_center = AccessTools.StaticFieldRefAccess<IntVec3>(typeof(SiegeBlueprintPlacer).Field("center"));
        public static readonly AccessTools.FieldRef<Faction> SiegeBlueprintPlacer_faction = AccessTools.StaticFieldRefAccess<Faction>(typeof(SiegeBlueprintPlacer).Field("faction"));
        public static readonly AccessTools.FieldRef<IntRange> SiegeBlueprintPlacer_NumCoverRange = AccessTools.StaticFieldRefAccess<IntRange>(typeof(SiegeBlueprintPlacer).Field("NumCoverRange"));
        public static readonly AccessTools.FieldRef<List<IntVec3>> SiegeBlueprintPlacer_placedCoverLocs = AccessTools.StaticFieldRefAccess<List<IntVec3>>(typeof(SiegeBlueprintPlacer).Field("placedCoverLocs"));
        public static readonly AccessTools.FieldRef<IntRange> SiegeBlueprintPlacer_CoverLengthRange = AccessTools.StaticFieldRefAccess<IntRange>(typeof(SiegeBlueprintPlacer).Field("CoverLengthRange"));

        public static readonly AccessTools.FieldRef<Projectile, int> Projectile_ticksToImpact = AccessTools.FieldRefAccess<Projectile, int>("ticksToImpact");
        public static readonly AccessTools.FieldRef<Projectile, Vector3> Projectile_origin = AccessTools.FieldRefAccess<Projectile, Vector3>("origin");
        public static readonly AccessTools.FieldRef<Projectile, Vector3> Projectile_destination = AccessTools.FieldRefAccess<Projectile, Vector3>("destination");
        public static readonly AccessTools.FieldRef<Projectile, LocalTargetInfo> Projectile_usedTarget = AccessTools.FieldRefAccess<Projectile, LocalTargetInfo>("usedTarget");
    }

}
