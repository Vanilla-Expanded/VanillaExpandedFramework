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

    public static class Patch_Verb
    {

        [HarmonyPatch(typeof(Verb), nameof(Verb.Available))]
        public static class Available
        {

            public static void Postfix(Verb __instance, ref bool __result)
            {
                // Unusable shield verbs don't get counted
                if (__result && __instance.EquipmentSource != null && __instance.EquipmentSource.IsShield(out CompShield shieldComp))
                    __result = shieldComp.UsableNow;
            }
        }

        [HarmonyPatch(typeof(VerbProperties), "AdjustedCooldown", new Type[]
        {
            typeof(Verb), typeof(Pawn)
        })]
        public static class VerbProperties_AdjustedCooldown_Patch
        {
            public static void Postfix(ref float __result, Verb ownerVerb, Pawn attacker)
            {
                var pawn = ownerVerb.CasterPawn;
                if (pawn != null)
                {
                    __result *= pawn.GetStatValue(VFEDefOf.VEF_VerbCooldownFactor);
                }
            }
        }
    }

}
