using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Weapons;

[HarmonyPatch(typeof(StatDef), "PopulateMutableStats")]
public static class VanillaExpandedFramework_StatDef_PopulateMutableStats
{
    // As opposed to other WeaponTraitDefExtension patches, this one isn't conditional. It only runs
    // when clearing static caches (minimal impact), and it either needs to run early (before we run
    // the patch) or we need run StatDef:SetImmutability method again at startup, which is honestly wasteful.

    public static void Postfix(HashSet<StatDef> ___mutableStats)
    {
        if (___mutableStats == null)
        {
            Log.Error("[VEF] Trying to mark relevant stats as mutable, but the mutable stat hash set is null.");
            return;
        }

        foreach (var trait in DefDatabase<WeaponTraitDef>.AllDefsListForReading)
        {
            var extension = trait.GetModExtension<WeaponTraitDefExtension>();
            if (extension?.conditionalStatAffecters != null)
            {
                foreach (var statAffecter in extension.conditionalStatAffecters)
                {
                    AddStatsFromModifiers(statAffecter.statOffsets);
                    AddStatsFromModifiers(statAffecter.statFactors);
                }
            }
        }

        void AddStatsFromModifiers(List<StatModifier> mods)
        {
            if (mods != null)
                ___mutableStats.AddRange(mods.Select(mod => mod.stat));
        }
    }
}