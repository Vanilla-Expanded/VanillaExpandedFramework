using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;
using RimWorld.Planet;

namespace VEF.AnimalBehaviours
{


    [HarmonyPatch(typeof(FloatMenuOptionProvider_WorkGivers))]
    [HarmonyPatch("GetWorkGiverOption")]
    public static class VanillaExpandedFramework_FloatMenuOptionProvider_WorkGivers_GetWorkGiverOption_Patch
    {
        [HarmonyPostfix]
        static void NoWorkBesidesAttacks(Pawn pawn, ref FloatMenuOption __result)
        {
            if (StaticCollectionsClass.draftable_animals.Contains(pawn))
            {
                __result = null;
            }

        }
    }
}
