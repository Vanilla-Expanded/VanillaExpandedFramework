using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF;

public static class MergeablePatches
{
    private static void MergeList(IList list)
    {
        // Make sure modExtensions is not null and has at least 2 extensions
        if (list is not { Count: > 1 })
            return;

        // Don't use a static dictionary, it's not the main thread right now.
        Dictionary<Type, List<IMergeable>> allMergeable = null;

        for (var i = 0; i < list.Count; i++)
        {
            var obj = list[i];
            if (obj is not IMergeable mergeable)
                continue;

            allMergeable ??= [];

            var type = obj.GetType();
            if (!allMergeable.TryGetValue(type, out var mergeableList))
                allMergeable[type] = mergeableList = [];

            mergeableList.Add(mergeable);
        }

        if (allMergeable == null)
            return;

        foreach (var (_, mergeableList) in allMergeable)
        {
            if (mergeableList.Count <= 1)
                continue;

            // Take negative priority so the order is descending
            mergeableList.SortBy(x => -x.Priority);

            var main = mergeableList[0];
            for (var i = 1; i < mergeableList.Count; i++)
            {
                var other = mergeableList[i];
                if (main.CanMerge(other))
                {
                    list.Remove(other);
                    main.Merge(other);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Def), nameof(Def.ResolveReferences))]
    public static class VanillaExpandedFramework_Def_ResolveReferences_Patch
    {
        private static void Prefix(Def __instance)
        {
            try
            {
                MergeList(__instance.modExtensions);
            }
            catch (Exception e)
            {
                Log.Error($"[VEF] Failed merging {nameof(Def)}.{nameof(Def.modExtensions)} for {__instance}. Exception:\n{e}");
            }

            // Defs that don't implement ResolveReferences
            if (__instance is AbilityDef abilityDef)
            {
                try
                {
                    MergeList(abilityDef.comps);
                }
                catch (Exception e)
                {
                    Log.Error($"[VEF] Failed merging {nameof(AbilityDef)}.{nameof(AbilityDef.comps)} for {abilityDef}. Exception:\n{e}");
                }
            }

            if (__instance is RitualVisualEffectDef ritualVisualEffectDef)
            {
                try
                {
                    MergeList(ritualVisualEffectDef.comps);
                }
                catch (Exception e)
                {
                    Log.Error($"[VEF] Failed merging {nameof(RitualVisualEffectDef)}.{nameof(RitualVisualEffectDef.comps)} for {ritualVisualEffectDef}. Exception:\n{e}");
                }
            }

            if (__instance is RitualOutcomeEffectDef ritualOutcomeEffectDef)
            {
                try
                {
                    MergeList(ritualOutcomeEffectDef.comps);
                }
                catch (Exception e)
                {
                    Log.Error($"[VEF] Failed merging {nameof(RitualOutcomeEffectDef)}.{nameof(RitualOutcomeEffectDef.comps)} for {ritualOutcomeEffectDef}. Exception:\n{e}");
                }
            }

            if (__instance is SurgeryOutcomeEffectDef surgeryOutcomeEffectDef)
            {
                try
                {
                    MergeList(surgeryOutcomeEffectDef.comps);
                }
                catch (Exception e)
                {
                    Log.Error($"[VEF] Failed merging {nameof(SurgeryOutcomeEffectDef)}.{nameof(SurgeryOutcomeEffectDef.comps)} for {surgeryOutcomeEffectDef}. Exception:\n{e}");
                }
            }
        }
    }

    [HarmonyPatch(typeof(ThingDef), nameof(ThingDef.ResolveReferences))]
    public static class VanillaExpandedFramework_ThingDef_ResolveReferences_Patch
    {
        private static void Prefix(ThingDef __instance)
        {
            try
            {
                MergeList(__instance.comps);
            }
            catch (Exception e)
            {
                Log.Error($"[VEF] Failed merging {nameof(ThingDef)}.{nameof(ThingDef.comps)} for {__instance}. Exception:\n{e}");
            }
        }
    }

    [HarmonyPatch(typeof(HediffDef), nameof(HediffDef.ResolveReferences))]
    public static class VanillaExpandedFramework_HediffDef_ResolveReferences_Patch
    {
        private static void Prefix(HediffDef __instance)
        {
            try
            {
                MergeList(__instance.comps);
            }
            catch (Exception e)
            {
                Log.Error($"[VEF] Failed merging {nameof(HediffDef)}.{nameof(HediffDef.comps)} for {__instance}. Exception:\n{e}");
            }
        }
    }

    [HarmonyPatch(typeof(StorytellerDef), nameof(StorytellerDef.ResolveReferences))]
    public static class VanillaExpandedFramework_StorytellerDef_ResolveReferences_Patch
    {
        private static void Prefix(StorytellerDef __instance)
        {
            try
            {
                MergeList(__instance.comps);
            }
            catch (Exception e)
            {
                Log.Error($"[VEF] Failed merging {nameof(StorytellerDef)}.{nameof(StorytellerDef.comps)} for {__instance}. Exception:\n{e}");
            }
        }
    }

    [HarmonyPatch(typeof(WorldObjectDef), nameof(WorldObjectDef.ResolveReferences))]
    public static class VanillaExpandedFramework_WorldObjectDef_ResolveReferences_Patch
    {
        private static void Prefix(WorldObjectDef __instance)
        {
            try
            {
                MergeList(__instance.comps);
            }
            catch (Exception e)
            {
                Log.Error($"[VEF] Failed merging {nameof(WorldObjectDef)}.{nameof(WorldObjectDef.comps)} for {__instance}. Exception:\n{e}");
            }
        }
    }
}