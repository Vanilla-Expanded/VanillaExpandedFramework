using System.Collections.Generic;
using System.Linq;
using AnimalBehaviours;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaWeaponsExpanded
{
    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    static class FloatMenuMakerMap_AddHumanlikeOrders_Patch
    {
        static HashSet<ThingDef> allToolsList = new HashSet<ThingDef>();

        static FloatMenuMakerMap_AddHumanlikeOrders_Patch()
        {

            HashSet<ToolsUsableByNonViolentPawnsDef> allLists = DefDatabase<ToolsUsableByNonViolentPawnsDef>.AllDefsListForReading.ToHashSet();
            foreach (ToolsUsableByNonViolentPawnsDef individualList in allLists)
            {
                allToolsList.AddRange(individualList.toolsUsableByNonViolentPawns);
            }
        }

        private static bool inMenuMaker;

        public static void Prefix(Pawn pawn)
        {
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
