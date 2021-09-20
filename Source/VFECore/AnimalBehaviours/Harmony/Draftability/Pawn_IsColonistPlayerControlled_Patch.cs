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

    /*This first Harmony postfix deals with adding a Pawn_DraftController if it detects the creature
     * belongs to the player and to the custom class CompDraftable. It also adds a Pawn_EquipmentTracker
     * because some ugly errors are produced otherwise, though it is basically unused
     * 
     */
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("IsColonistPlayerControlled", MethodType.Getter)]
    public static class VanillaExpandedFramework_Pawn_IsColonistPlayerControlled_Patch
    {
        [HarmonyPostfix]
        static void AddAnimalAsColonist(Pawn __instance, ref bool __result)
        {
            bool flagIsCreatureDraftable = AnimalCollectionClass.draftable_animals.ContainsKey(__instance);
            bool flagIsMindControlBuildingPresent = AnimalCollectionClass.numberOfAnimalControlHubsBuilt > 0;
            if (flagIsCreatureDraftable && flagIsMindControlBuildingPresent)
            {
                __result = __instance.Spawned && __instance.HostFaction == null;
            }
        }
    }
}
