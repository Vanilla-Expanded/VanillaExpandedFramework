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

    public static class Patch_Pawn
    {

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.SetFaction))]
        public static class SetFaction
        {

            public static void Postfix(Pawn __instance, Faction newFaction)
            {
                // Re-resolve pack animal graphics
                if (__instance.RaceProps.packAnimal)
                {
                    __instance.Drawer?.renderer?.graphics?.ResolveAllGraphics();
                }
            }

        }

    }

}
