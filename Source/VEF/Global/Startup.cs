using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;
using VEF.Pawns;
using VEF.Factions;
using VEF.Research;

namespace VEF
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