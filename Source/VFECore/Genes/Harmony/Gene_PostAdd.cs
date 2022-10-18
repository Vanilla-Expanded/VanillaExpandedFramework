using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using HarmonyLib;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(Gene), "PostAdd")]
    public static class VanillaGenesExpanded_Gene_PostAdd_Patch
    {
        [HarmonyPostfix]
        public static void PostFix(Gene __instance)
        {
            GeneExtension extension = __instance.def.GetModExtension<GeneExtension>();
            if (extension?.forceFemale == true)
            {
                __instance.pawn.gender = Gender.Female;
            }
            if (extension?.forceMale == true)
            {
                __instance.pawn.gender = Gender.Male;
            }

        }
    }
}