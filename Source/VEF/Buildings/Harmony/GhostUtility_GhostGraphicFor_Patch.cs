using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VEF.Buildings;

[HarmonyPatch(typeof(GhostUtility))]
[HarmonyPatch(nameof(GhostUtility.GhostGraphicFor))]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class VanillaExpandedFramework_GhostUtility_GhostGraphicFor_Patch
{
    private static readonly Dictionary<ThingDef, GhostGraphicExtension> supportedDefs = new();
    private static readonly Dictionary<int, Graphic> ghostGraphics = new();
    // Extra data cached for CustomGraphicMethodCached and CustomGraphicMethodNonCached
    private static readonly Dictionary<int, bool> isMainGraphic = new();

    [HarmonyPrepare]
    private static bool Prepare(MethodBase method)
    {
        if (method != null)
            return true;

        foreach (var def in DefDatabase<ThingDef>.AllDefs)
        {
            var extension = def.GetModExtension<GhostGraphicExtension>();
            if (extension == null
                // Disabled (or under it)
                || extension.ghostMode <= GhostGraphicExtension.CustomGhostMode.Vanilla
                // Higher than the max value
                || extension.ghostMode >= (GhostGraphicExtension.CustomGhostMode)Enum.GetNames(typeof(GhostGraphicExtension.CustomGhostMode)).Length)
            {
                continue;
            }

            supportedDefs[def] = extension;
        }

        return supportedDefs.Count > 0;
    }

    [HarmonyPrefix]
    private static bool DisplayBlueprintGraphic(Graphic baseGraphic, ThingDef thingDef, Color ghostCol, ThingDef stuff, ref Graphic __result)
    {
        if (!supportedDefs.TryGetValue(thingDef, out var extension))
            return true;

        var hash = Gen.HashCombine(0, baseGraphic);
        hash = Gen.HashCombine(hash, thingDef);
        hash = Gen.HashCombineStruct(hash, ghostCol);
        hash = Gen.HashCombine(hash, stuff);

        // Use cached graphics (rather than grabbing a new one each time)
        if (ghostGraphics.TryGetValue(hash, out __result))
        {
            if (__result == null && isMainGraphic.TryGetValue(hash, out var cachedMainGraphic))
                __result = extension.GetCustomGraphic(baseGraphic, thingDef, ghostCol, thingDef, cachedMainGraphic, hash);

            return __result == null;
        }

        // Check if the graphic is the main thing graphic
        var mainGraphic = IsMainGraphic(baseGraphic, thingDef);
        baseGraphic ??= thingDef.graphic;

        switch (mainGraphic ? extension.ghostMode : extension.extraGraphicGhostMode)
        {
            // Same as vanilla handling, except we never allow for linking graphics (useful for non-main graphics like turret tops)
            case GhostGraphicExtension.CustomGhostMode.VanillaNoLinking:
            {
                GraphicData graphicData = null;
                if (baseGraphic.data != null)
                {
                    graphicData = new GraphicData();
                    graphicData.CopyFrom(baseGraphic.data);
                    graphicData.shadowData = null;
                }

                if (baseGraphic is Graphic_Appearances appearances && stuff != null)
                    __result = GraphicDatabase.Get<Graphic_Single>(appearances.SubGraphicFor(stuff).path, ShaderTypeDefOf.EdgeDetect.Shader, thingDef.graphicData.drawSize, ghostCol, Color.white, graphicData);
                else
                    __result = GraphicDatabase.Get(baseGraphic.GetType(), baseGraphic.path, ShaderTypeDefOf.EdgeDetect.Shader, baseGraphic.drawSize, ghostCol, Color.white, graphicData, null);

                break;
            }
            // Use blueprint graphic
            case GhostGraphicExtension.CustomGhostMode.Blueprint:
            {
                var graphic = GraphicDatabase.Get(typeof(Graphic_Multi), thingDef.building.blueprintGraphicData.texPath, ShaderTypeDefOf.Cutout.Shader, baseGraphic.drawSize, Color.white, Color.white, thingDef.building.blueprintGraphicData, null);
                __result = graphic;
                break;
            }
            // Use provided custom graphic path
            case GhostGraphicExtension.CustomGhostMode.CustomGraphicPath:
            {
                var extraData = mainGraphic
                    ? extension.customGraphicData
                    : extension.extraCustomGraphicData;

                GraphicData graphicData = null;
                if (baseGraphic.data != null)
                {
                    graphicData = new GraphicData();
                    graphicData.CopyFrom(baseGraphic.data);
                    graphicData.shadowData = null;
                    if (extraData.drawRotated != null)
                        graphicData.drawRotated = extraData.drawRotated.Value;
                    if (extraData.allowFlip != null)
                        graphicData.allowFlip = extraData.allowFlip.Value;
                }

                __result = GraphicDatabase.Get(
                    extraData.graphicClass ?? typeof(Graphic_Single),
                    extraData.texPath,
                    ShaderTypeDefOf.EdgeDetect.Shader,
                    extraData.drawSize ?? thingDef.graphicData.drawSize,
                    ghostCol,
                    Color.white,
                    graphicData,
                    null);
                break;
            }
            // Call method in the extension. Will throw an exception unless using a subclass. Will be cached.
            case GhostGraphicExtension.CustomGhostMode.CustomGraphicMethodCached:
            {
                __result = extension.GetCustomGraphic(baseGraphic, thingDef, ghostCol, stuff, mainGraphic, hash);
                isMainGraphic[hash] = mainGraphic;
                break;
            }
            // Call method in the extension. Will throw an exception unless using a subclass. Won't be cached.
            case GhostGraphicExtension.CustomGhostMode.CustomGraphicMethodNonCached:
            {
                __result = extension.GetCustomGraphic(baseGraphic, thingDef, ghostCol, stuff, mainGraphic, hash);
                ghostGraphics.Add(hash, null);
                isMainGraphic.Add(hash, mainGraphic);
                return false;
            }
            // Default shouldn't happen
            default:
            // Let vanilla handle it 100%
            case GhostGraphicExtension.CustomGhostMode.Vanilla:
            {
                ghostGraphics.Add(hash, null);
                return true;
            }
        }

        // Add graphic (or null) to cached, and data on how to handle this
        ghostGraphics.Add(hash, __result);
        // Don't run vanilla method unless we're in vanilla mode
        return false;
    }

    private static bool IsMainGraphic(Graphic baseGraphic, ThingDef thingDef)
    {
        if (baseGraphic == null || baseGraphic.path == thingDef.graphic.path)
            return true;

        if (thingDef.randomStyle != null)
        {
            for (var i = 0; i < thingDef.randomStyle.Count; i++)
            {
                if (baseGraphic.path == thingDef.randomStyle[i]?.StyleDef?.Graphic.path)
                    return true;
            }
        }

        if (ModsConfig.IdeologyActive)
        {
            foreach (var category in DefDatabase<StyleCategoryDef>.AllDefs)
            {
                if (!category.thingDefStyles.NullOrEmpty())
                {
                    foreach (var style in category.thingDefStyles)
                    {
                        if (style.ThingDef == thingDef)
                        {
                            if (baseGraphic.path == style.StyleDef?.Graphic?.path)
                                return true;
                            break;
                        }
                    }
                }
            }
        }

        return false;
    }
}