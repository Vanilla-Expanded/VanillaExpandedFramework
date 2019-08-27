using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace VFECore
{

    public static class Patch_PawnGenerator
    {

        [HarmonyPatch(typeof(PawnGenerator), "GenerateGearFor")]
        public static class GenerateGearFor
        {

            public static void Postfix(Pawn pawn)
            {
                // Also generate shield
                PawnShieldGenerator.TryGenerateShieldFor(pawn);
            }

        }

    }

}
