using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using static Verse.DamageWorker;

namespace VanillaStorytellersExpanded
{

    [HarmonyPatch(typeof(Faction), "NaturalGoodwill", MethodType.Getter)]
    public static class Patch_NaturalGoodwill
    {
        public static void Postfix(Faction __instance, ref int __result)
        {
            if (!__instance.IsPlayer)
            {
                var options = Find.Storyteller.def.GetModExtension<StorytellerDefExtension>();
                if (options != null && options.storytellerThreat?.naturallGoodwillForAllFactions != null)
                {
                    __result = (int)options.storytellerThreat.naturallGoodwillForAllFactions.Average;
                }
            }
        }
    }
}
