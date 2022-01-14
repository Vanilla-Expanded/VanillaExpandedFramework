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

    
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("IsColonistPlayerControlled", MethodType.Getter)]
    public static class VanillaExpandedFramework_Pawn_IsColonistPlayerControlled_Patch
    {
        [HarmonyPostfix]
        static void AddAnimalAsColonist(Pawn __instance, ref bool __result)
        {
            bool flagIsCreatureDraftable = AnimalCollectionClass.draftable_animals.Contains(__instance);
          
            if (flagIsCreatureDraftable)
            {
                __result = __instance.Spawned && __instance.HostFaction == null && __instance.Faction == Faction.OfPlayer;
            }
        }
    }
}
