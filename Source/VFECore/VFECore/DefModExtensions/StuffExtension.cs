using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFECore
{

    public class StuffExtension : DefModExtension
    {
        public float? structureGenerationCommonalityFactor;
        public float? weaponGenerationCommonalityFactor;
        public float? apparelGenerationCommonalityFactor;

        public float? structureGenerationCommonalityOffset;
        public float? weaponGenerationCommonalityOffset;
        public float? apparelGenerationCommonalityOffset;
    }
}
