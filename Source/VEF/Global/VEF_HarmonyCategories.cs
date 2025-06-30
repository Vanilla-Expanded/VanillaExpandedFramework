using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using VEF.Genes;
using Verse;

namespace VEF;

public class VEF_HarmonyCategories
{
    public const string LateHarmonyPatchCategory = "LateHarmonyPatch";
    public const string MoveSpeedFactorByTerrainTagCategory = "MoveSpeedFactorByTerrainTag";

    internal static void TryPatchAll(Harmony harmony)
    {
        try
        {
            harmony.PatchAllUncategorized();
        }
        catch (Exception e)
        {
            Log.Error($"[VEF] Failed running uncategorized patches:\n{e}");
        }

        // Delay running patches that still need to wait, for example ones that rely on
        // some condition to run (like defs having a comp or def mod extension).
        LongEventHandler.ExecuteWhenFinished(() =>
        {
            RunPatches(harmony, LateHarmonyPatchCategory);
            RunPatches(harmony, MoveSpeedFactorByTerrainTagCategory, IsMoveSpeedFactorByTerrainTagActive);
        });
    }

    private static void RunPatches(Harmony harmony, string patchCategory, Func<bool> validator = null)
    {
        try
        {
            if (validator == null || validator())
                harmony.PatchCategory(patchCategory);
        }
        catch (Exception e)
        {
            Log.Error($"[VEF] Failed running patches from category '{patchCategory}':\n{e}");
        }
    }

    private static bool IsMoveSpeedFactorByTerrainTagActive()
    {
        // Only apply the patch if there's any GeneDef/HediffDef with
        // moveSpeedFactorByTerrainTag that isn't null/empty, and there's
        // at least a single terrain that has its terrain tag.

        foreach (var def in DefDatabase<GeneDef>.AllDefs)
        {
            var extension = def.GetModExtension<GeneExtension>();
            if (extension != null && IsThereAnyTerrainWithTag(extension.moveSpeedFactorByTerrainTag))
                return true;
        }

        foreach (var hediff in DefDatabase<HediffDef>.AllDefs)
        {
            var comp = hediff.CompProps<HediffCompProperties_MoveSpeedFactorByTerrainTag>();
            if (comp != null && IsThereAnyTerrainWithTag(comp.moveSpeedFactorByTerrainTag))
                return true;
        }

        return false;

        static bool IsThereAnyTerrainWithTag(Dictionary<string, List<MoveSpeedFactor>> moveSpeedFactorByTerrainTag)
        {
            if (moveSpeedFactorByTerrainTag.NullOrEmpty())
                return false;

            foreach (var tag in moveSpeedFactorByTerrainTag.Keys)
            {
                // Check if there's a TerrainDef that has a tag matching our tags
                if (DefDatabase<TerrainDef>.AllDefs.Any(terrain => terrain.tags != null && terrain.tags.Contains(tag)))
                    return true;
            }

            return false;
        }
    }
}