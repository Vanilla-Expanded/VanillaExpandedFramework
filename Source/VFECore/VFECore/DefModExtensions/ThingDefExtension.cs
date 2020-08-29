using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

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

    }

}
