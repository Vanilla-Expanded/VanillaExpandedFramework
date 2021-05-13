using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFECore
{

    public static class Patch_CompEquippable
    {

        [HarmonyPatch(typeof(CompEquippable), nameof(CompEquippable.GetVerbsCommands))]
        public static class GetVerbsCommands
        {

            public static void Postfix(CompEquippable __instance, ref IEnumerable<Command> __result)
            {
                // Clear gizmos if parent is a shield and isn't off-hand - though the verb will still be usable
                var pawn = __instance.PrimaryVerb?.CasterPawn;
                if (pawn != null && pawn.equipment.Primary != __instance.parent)
                    __result = __result.Where(g => false);
            }

        }

    }

}
