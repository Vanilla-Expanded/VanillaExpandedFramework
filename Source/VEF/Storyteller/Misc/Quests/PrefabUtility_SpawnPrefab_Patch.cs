using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Storyteller
{
    [HarmonyPatch(typeof(PrefabUtility), "SpawnPrefab")]
    public static class PrefabUtility_SpawnPrefab_Patch
    {
        public static void Prefix(PrefabDef prefab, Map map, IntVec3 pos, Rot4 rot)
        {
            prefab.GetThingsList().SortBy(t => (t.def.building != null && t.def.building.isEdifice) ? 1 : 0);
            rot = PrefabUtility.ValidateRotation(prefab, rot);
            var root = PrefabUtility.GetRoot(prefab, pos, rot);
            foreach (var (data, local) in prefab.GetTerrain())
            {
                if (data.def.isFoundation)
                {
                    var c = root + PrefabUtility.GetAdjustedLocalPosition(local, rot);
                    if (c.InBounds(map) && map.terrainGrid.CanRemoveTopLayerAt(c))
                    {
                        map.terrainGrid.RemoveTopLayer(c, doLeavings: false);
                    }
                }
            }
        }

        public static void Postfix(PrefabDef prefab, Map map, IntVec3 pos, Rot4 rot, Faction faction, List<Thing> spawned, Action<Thing> onSpawned)
        {
            var ext = prefab.GetModExtension<PrefabExtension>();
            Log.Error($"Spawning prefab {prefab}, pawn data count: {ext?.pawns?.Count}");
            if (ext == null) return;

            rot = PrefabUtility.ValidateRotation(prefab, rot);
            var root = PrefabUtility.GetRoot(prefab, pos, rot);
            if (ext.roofs != null)
            {
                foreach (var roofData in ext.roofs)
                {
                    roofData.SetRoof(map, root, rot);
                }
            }

            if (ext.pawns != null)
            {
                foreach (var pawn in ext.pawns)
                {
                    pawn.SpawnPawns(prefab, map, root, rot, faction, spawned, onSpawned);
                }
            }
        }
    }
}
