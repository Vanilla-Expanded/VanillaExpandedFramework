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
        public DraftedDrawOffsets draftedDrawOffsets = null;

        // For shields and apparel
        public List<PawnKindDef> useFactionColourForPawnKinds;

        // For artillery
        public float siegeBlueprintPoints = SiegeBlueprintPlacer.ArtyCost;

        // For thing that can be discovered by deep scanner
        public Color deepColor = Color.white;
        public float transparencyMultiplier = 0.5f;

        // For skyfallers that can fall into shield fields
        public int shieldDamageIntercepted = -1;

    }

    public class DraftedDrawOffsets
    {
        public Offset north = null;
        public Offset east = null;
        public Offset south = null;
        public Offset west = null;
    }

    public class Offset
    {
        public Vector3 posOffset = new Vector3(0, 0, 0);
        public float angOffset = 0;
    }

}