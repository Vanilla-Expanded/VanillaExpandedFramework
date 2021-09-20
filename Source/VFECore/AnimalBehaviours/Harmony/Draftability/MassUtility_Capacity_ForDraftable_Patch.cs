using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;
using RimWorld.Planet;


namespace AnimalBehaviours
{

    /*This Harmony Prefix makes the creature carry more weight
*/
    [HarmonyPatch(typeof(MassUtility))]
    [HarmonyPatch("Capacity")]
    public static class VanillaExpandedFramework_MassUtility_Capacity_ForDraftable_Patch
    {
        [HarmonyPostfix]
        public static void MakeThemCarryMore(Pawn p, ref float __result)

        {
            bool flagIsCreatureMine = p.Faction != null && p.Faction.IsPlayer;
            bool flagIsCreatureDraftable = AnimalCollectionClass.draftable_animals.ContainsKey(p);
            bool flagIsAnimalControlHubBuilt = AnimalCollectionClass.numberOfAnimalControlHubsBuilt > 0;
            bool flagCanCreatureCarryMore = false;

            if (flagIsCreatureDraftable && flagIsAnimalControlHubBuilt)
            {
                flagCanCreatureCarryMore = AnimalCollectionClass.draftable_animals[p][3];
            }

            if (flagIsCreatureDraftable && flagIsCreatureMine && flagCanCreatureCarryMore)
            {
                __result = p.BodySize * 50f;
            }

        }
    }


}
