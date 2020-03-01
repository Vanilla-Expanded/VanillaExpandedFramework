using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFECore
{

    public static class Patch_Cities_GenCity
    {

        public static class manual_RandomCityFaction_predicate
        {

            public static void Postfix(Faction x, ref bool __result)
            {
                // Also factor in our DefModExtension for if a faction can have cities
                if (__result)
                {
                    var factionDefExtension = FactionDefExtension.Get(x.def);
                    __result = factionDefExtension.hasCities;
                }
            }

        }

    }

}
