using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFECore
{

    [StaticConstructorOnStartup]
    public static class StaticConstructorClass
    {

        static StaticConstructorClass()
        {
            // Cache setters
            PawnShieldGenerator.Reset();
            ScenPartUtility.SetCache();
        }
        public static Mesh plane20Flip = MeshMakerPlanes.NewPlaneMesh(2f, flipped: true);
    }

}
