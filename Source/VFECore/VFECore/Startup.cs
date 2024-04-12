using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace VFECore
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        public static Mesh plane20Flip = MeshMakerPlanes.NewPlaneMesh(2f, true);

        static Startup()
        {
            PawnShieldGenerator.Reset();
            ScenPartUtility.SetCache();
            ResearchProjectUtility.AutoAssignRules();
        }
    }
}