using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VFECore
{
    public class ThingDefExtension : DefModExtension
    {
        private static readonly ThingDefExtension DefaultValues = new ThingDefExtension();

        public static ThingDefExtension Get(Def def) => def.GetModExtension<ThingDefExtension>() ?? DefaultValues;

        // For weapons
        public bool usableWithShields = false;

        // For shields and apparel
        public List<PawnKindDef> useFactionColourForPawnKinds;

        // For artillery
        public float siegeBlueprintPoints = SiegeBlueprintPlacer.ArtyCost;

        // For thing that can be discovered by deep scanner
        public Color deepColor = Color.green;
    }
}