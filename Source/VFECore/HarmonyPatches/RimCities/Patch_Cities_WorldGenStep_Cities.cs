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

    public static class Patch_Cities_WorldGenStep_Cities
    {

        public static class manual_GenerateFresh
        {

            public static bool Prefix()
            {
                // Prevent world gen from bugging out with Maynard Medieval
                return NonPublicMethods.RimCities.GenCity_RandomCityFaction(null) != null;
            }

        }

    }

}
