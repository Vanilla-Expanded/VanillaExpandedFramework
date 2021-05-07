using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld.Planet;
using HarmonyLib;


namespace KCSG
{
    [HarmonyPatch(typeof(WorldObjectsHolder))]
    [HarmonyPatch("Add", MethodType.Normal)]
    public class HideSettlementsForNomadicFactionsPatch
    {
        public static bool Prefix(WorldObject o)
        {
            return (o?.Faction?.def.GetModExtension<FactionSettlement>()?.canSpawnSettlements == false) ? false : true;
        }
    }
}
