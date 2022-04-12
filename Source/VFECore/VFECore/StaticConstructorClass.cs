using UnityEngine;
using Verse;

namespace VFECore
{
    [StaticConstructorOnStartup]
    public static class StaticConstructorClass
    {
        public static Mesh plane20Flip = MeshMakerPlanes.NewPlaneMesh(2f, true);

        static StaticConstructorClass()
        {
            // Cache setters
            PawnShieldGenerator.Reset();
            ScenPartUtility.SetCache();
        }
    }
}