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

    [HarmonyPatch(typeof(Pawn), "CanTakeOrder", MethodType.Getter)]
    public static class VanillaExpandedFramework_Pawn_CanTakeOrder_Patch
    {
        [HarmonyPostfix]
        public static void MakePawnControllable(Pawn __instance, ref bool __result)
        {
            bool flagIsCreatureMine = __instance.Faction != null && __instance.Faction?.IsPlayer==true;
            bool flagIsCreatureDraftable = StaticCollectionsClass.draftable_animals.Contains(__instance);

            if (flagIsCreatureDraftable && flagIsCreatureMine)
            {
                //Log.Message("You should be controllable now");
                __result = true;
            }

        }
    }
}
