using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Storyteller
{
    [HarmonyPatch(typeof(PrefabUtility), "SpawnPrefab")]
    public static class PrefabUtility_SpawnPrefab_Patch
    {
        public static void Postfix(PrefabDef prefab, Map map, IntVec3 pos, Rot4 rot)
        {
            var ext = prefab.GetModExtension<PrefabExtension>();
            if (ext == null) return;

            rot = PrefabUtility.ValidateRotation(prefab, rot);
            var root = PrefabUtility.GetRoot(prefab, pos, rot);
            if (ext.roofs != null)
            {
                foreach (var roofData in ext.roofs)
                {
                    foreach (var rect in roofData.rects)
                    {
                        foreach (var cell in rect.Cells)
                        {
                            map.roofGrid.SetRoof(root + PrefabUtility.GetAdjustedLocalPosition(cell, rot), roofData.def);
                        }
                    }
                }
            }
        }
    }
}