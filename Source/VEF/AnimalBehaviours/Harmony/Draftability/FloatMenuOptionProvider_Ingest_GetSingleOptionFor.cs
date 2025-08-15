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


    [HarmonyPatch(typeof(FloatMenuOptionProvider_Ingest))]
    [HarmonyPatch("GetSingleOptionFor")]
    public static class VanillaExpandedFramework_FloatMenuOptionProvider_Ingest_GetSingleOptionFor_Patch
    {
        [HarmonyPostfix]
        static void RemoveErrorForNonForbiddables(FloatMenuContext context, Thing clickedThing, ref FloatMenuOption __result)
        {

            if (StaticCollectionsClass.draftable_animals.Contains(context.FirstSelectedPawn))
            {

                CompForbiddable comp = clickedThing.TryGetComp<CompForbiddable>();
                if (comp == null)
                {
                    __result = null;
                }
                
            }

        }
    }
}
