using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFECore
{

    [StaticConstructorOnStartup]
    public static class NonPublicFields
    {

        public static FieldInfo Pawn_EquipmentTracker_equipment = typeof(Pawn_EquipmentTracker).GetField("equipment", BindingFlags.NonPublic | BindingFlags.Instance);

        public static FieldInfo Pawn_HealthTracker_pawn = typeof(Pawn_HealthTracker).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);

        public static FieldInfo PawnRenderer_pawn = typeof(PawnRenderer).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);

        public static FieldInfo SiegeBlueprintPlacer_faction = typeof(SiegeBlueprintPlacer).GetField("faction", BindingFlags.NonPublic | BindingFlags.Static);

    }

}
