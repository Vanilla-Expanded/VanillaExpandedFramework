using System.Collections.Generic;
using System.Linq;
using VEF.AnimalBehaviours;
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;

namespace VEF.Weapons
{
    [HarmonyPatch(typeof(FloatMenuOptionProvider_Equip), "GetSingleOptionFor")]
    static class VanillaExpandedFramework_FloatMenuOptionProvider_Equip_GetSingleOptionFor_Patch
    {
        static HashSet<ThingDef> allToolsList = new HashSet<ThingDef>();

         static VanillaExpandedFramework_FloatMenuOptionProvider_Equip_GetSingleOptionFor_Patch()
         {
             HashSet<ToolsUsableByNonViolentPawnsDef> allLists = DefDatabase<ToolsUsableByNonViolentPawnsDef>.AllDefsListForReading.ToHashSet();
             foreach (ToolsUsableByNonViolentPawnsDef individualList in allLists)
             {
                 allToolsList.AddRange(individualList.toolsUsableByNonViolentPawns);
             }
         }

         private static bool inMenuMaker;

         public static void Prefix(FloatMenuContext context)
         {
            var pawn = context.FirstSelectedPawn;
            inMenuMaker = pawn.WorkTagIsDisabled(WorkTags.Violent);
         }

         public static void Postfix()
         {
             inMenuMaker = false;
         }

         [HarmonyPatch(typeof(ThingDef), nameof(ThingDef.IsWeapon), MethodType.Getter)]
         [HarmonyPostfix]
         public static bool IsWeapon(bool __result, ThingDef __instance)
         {
             if (inMenuMaker && IsToolDef(__instance))
             {
                 return false;
             }

             return __result;
         }

         [HarmonyPatch(typeof(ThingDef), nameof(ThingDef.IsRangedWeapon), MethodType.Getter)]
         [HarmonyPostfix]
         public static bool IsRangedWeapon(bool __result, ThingDef __instance)
         {
             if (inMenuMaker && IsToolDef(__instance))
             {
                 return false;
             }

             return __result;
         }



         public static bool IsToolDef(ThingDef thingDef)
         {
             return allToolsList?.Contains(thingDef)==true;
         }
    }
}
