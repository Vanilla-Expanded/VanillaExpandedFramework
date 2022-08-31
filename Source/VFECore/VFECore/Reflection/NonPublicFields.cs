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

namespace VFECore
{

    [StaticConstructorOnStartup]
    public static class NonPublicFields
    {
        public static FieldInfo SiegeBlueprintPlacer_center = AccessTools.Field(typeof(SiegeBlueprintPlacer), "center");
        public static FieldInfo SiegeBlueprintPlacer_faction = AccessTools.Field(typeof(SiegeBlueprintPlacer), "faction");
        public static FieldInfo SiegeBlueprintPlacer_NumCoverRange = AccessTools.Field(typeof(SiegeBlueprintPlacer), "NumCoverRange");
        public static FieldInfo SiegeBlueprintPlacer_placedCoverLocs = AccessTools.Field(typeof(SiegeBlueprintPlacer), "placedCoverLocs");
        public static FieldInfo SiegeBlueprintPlacer_CoverLengthRange = AccessTools.Field(typeof(SiegeBlueprintPlacer), "CoverLengthRange");

        public static FieldInfo Projectile_ticksToImpact = AccessTools.Field(typeof(Projectile), "ticksToImpact");
        public static FieldInfo Projectile_origin = AccessTools.Field(typeof(Projectile), "origin");
        public static FieldInfo Projectile_destination = AccessTools.Field(typeof(Projectile), "destination");
        public static FieldInfo Projectile_usedTarget = AccessTools.Field(typeof(Projectile), "usedTarget");
    }

}
