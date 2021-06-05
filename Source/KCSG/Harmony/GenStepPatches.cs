using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using System;
using System.Linq;
using Verse;

namespace KCSG
{
    /*[StaticConstructorOnStartup]
    [HarmonyPatch(typeof(GenStep_Settlement))]
    [HarmonyPatch("ScatterAt", MethodType.Normal)]
    public class GenStep_Settlement_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(IntVec3 c, Map map)
        {
            if (map.ParentFaction != null && map.ParentFaction.def.HasModExtension<FactionSettlement>())
            {
                FactionSettlement factionSettlement = map.ParentFaction.def.GetModExtension<FactionSettlement>();

                if (factionSettlement.symbolResolver == null) GenStepPatchesUtils.Generate(map, c, factionSettlement);
                else GenStepPatchesUtils.Generate(map, c, factionSettlement, factionSettlement.symbolResolver);

                return false;
            }
            else if (Find.World.worldObjects.AllWorldObjects.Find(o => o.Tile == map.Tile && o.def.HasModExtension<FactionSettlement>()) is WorldObject worldObject)
            {
                FactionSettlement factionSettlement = worldObject.def.GetModExtension<FactionSettlement>();

                if (factionSettlement.symbolResolver == null) GenStepPatchesUtils.Generate(map, c, factionSettlement);
                else GenStepPatchesUtils.Generate(map, c, factionSettlement, factionSettlement.symbolResolver);

                return false;
            }
            else return true;
        }
    }*/

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(GenStep_Power))]
    [HarmonyPatch("Generate", MethodType.Normal)]
    public class GenStep_Power_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(Map map)
        {
            if (map.ParentFaction != null && map.ParentFaction.def.HasModExtension<FactionSettlement>())
            {
                return false;
            }
            else return true;
        }
    }
}