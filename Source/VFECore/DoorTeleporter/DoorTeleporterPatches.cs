using HarmonyLib;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VFECore
{
    [HarmonyPatch]
    public static class DoorTeleporterPatches
    {
        [HarmonyPatch(typeof(Settlement), nameof(Settlement.GetFloatMenuOptions))]
        [HarmonyPostfix]
        public static void SettlementFloatOptions_Postfix(ref IEnumerable<FloatMenuOption> __result, Settlement __instance, Caravan caravan)
        {
            if (!__instance.HasMap) return;
            DoorTeleporter origin = null;
            HashSet<Map> maps = new();
            List<DoorTeleporter> doorTeleporters = new();
            foreach (DoorTeleporter skipdoor in WorldComponent_DoorTeleporterManager.Instance.DoorTeleporters)
                if (skipdoor.Map == __instance.Map) origin = skipdoor;
                else if (!maps.Contains(skipdoor.Map))
                {
                    maps.Add(skipdoor.Map);
                    doorTeleporters.Add(skipdoor);
                }

            if (origin != null)
                __result = __result.Concat(
                    doorTeleporters.SelectMany(skipdoor => CaravanArrivalAction_UseDoorTeleporter.GetFloatMenuOptions(caravan, origin, skipdoor)));
        }

        [HarmonyPatch(typeof(MapDeiniter), nameof(MapDeiniter.Deinit))]
        [HarmonyPrefix]
        public static void Deinit_Prefix(Map map)
        {
            WorldComponent_DoorTeleporterManager.Instance.DoorTeleporters.RemoveWhere(skipdoor => skipdoor.Map == map);
        }
    }
}
