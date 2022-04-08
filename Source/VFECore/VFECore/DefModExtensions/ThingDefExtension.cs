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
        public DraftedDrawOffsets draftedDrawOffsets = new DraftedDrawOffsets();

        // For shields and apparel
        public List<PawnKindDef> useFactionColourForPawnKinds;

        // For artillery
        public float siegeBlueprintPoints = SiegeBlueprintPlacer.ArtyCost;

        // For thing that can be discovered by deep scanner
        public Color deepColor = Color.white;
        public float transparencyMultiplier = 0.5f;
    }

    public class DraftedDrawOffsets
    {
        public Offset north = new Offset();
        public Offset east = new Offset();
        public Offset south = new Offset();
        public Offset west = new Offset();
    }

    public class Offset
    {
        public Vector3 posOffset = new Vector3(-999, -999, -999);
        public float angOffset = -999;
    }

}