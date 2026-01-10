using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VEF.Buildings;

public class GhostGraphicExtension : DefModExtension
{
    /// <summary>
    /// How the main graphic should be handled
    /// </summary>
    public CustomGhostMode ghostMode = CustomGhostMode.Vanilla;
    /// <summary>
    /// How non-main graphic (like turret top) should be handled
    /// </summary>
    public CustomGhostMode extraGraphicGhostMode = CustomGhostMode.Vanilla;
    /// <summary>
    /// Extra data for main graphic using CustomGraphicPath mode.
    /// </summary>
    public GraphicDataOverride customGraphicData;
    /// <summary>
    /// Extra data for non-main graphic using CustomGraphicPath mode.
    /// </summary>
    public GraphicDataOverride extraCustomGraphicData;

    /// <summary>
    /// Used when CustomGhostMode is set to either CustomGraphicMethodStatic or CustomGraphicMethodDynamic.
    /// </summary>
    /// <param name="baseGraphic">The graphic that we're trying to provide graphic for. It may be a different graphic (like a turret top) or the same as the def itself.</param>
    /// <param name="thingDef">The def that we want to make a ghost for.</param>
    /// <param name="ghostCol">The color that was used for the ghost.</param>
    /// <param name="stuff">The stuff used for the building.</param>
    /// <param name="main">If the graphic is considered main one, or non-main (like turret top). May not be 100% accurate with some comps, thing classes, etc.</param>
    /// <param name="hash">Hash based off of all the input arguments. Can be used for caching.</param>
    /// <returns>Graphic that will be used instead of the usual ghost graphic. Return null to let vanilla handle it.</returns>
    public virtual Graphic GetCustomGraphic(Graphic baseGraphic, ThingDef thingDef, Color ghostCol, ThingDef stuff, bool main, int hash)
    {
        return null;
    }

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var error in base.ConfigErrors())
            yield return error;

        // Disabled completely
        if (!CustomGhostModeEnabled(ghostMode) && !CustomGhostModeEnabled(extraGraphicGhostMode))
            yield return $"both {nameof(ghostMode)} and {nameof(extraGraphicGhostMode)} are {ghostMode} and {extraGraphicGhostMode}, the extension won't work.";
        // Using custom path, but custom path is null or empty
        if (ghostMode == CustomGhostMode.CustomGraphicPath)
        {
            if (customGraphicData == null)
                yield return $"{nameof(ghostMode)} is {ghostMode}, but {nameof(customGraphicData)} is null.";
            else if (customGraphicData.texPath.NullOrEmpty())
                yield return $"{nameof(ghostMode)} is {ghostMode}, but {nameof(customGraphicData)}.{nameof(customGraphicData.texPath)} is null or empty";
        }
        if (extraGraphicGhostMode == CustomGhostMode.CustomGraphicPath)
        {
            if (extraCustomGraphicData == null)
                yield return $"{nameof(extraGraphicGhostMode)} is {extraGraphicGhostMode}, but {nameof(extraCustomGraphicData)} is null.";
            else if (extraCustomGraphicData.texPath.NullOrEmpty())
                yield return $"{nameof(extraGraphicGhostMode)} is {extraGraphicGhostMode}, but {nameof(extraCustomGraphicData)}.{nameof(extraCustomGraphicData.texPath)} is null or empty";
        }
    }

    public static bool CustomGhostModeEnabled(CustomGhostMode ghostMode)
    {
        return ghostMode > CustomGhostMode.Vanilla && ghostMode < (CustomGhostMode)Enum.GetNames(typeof(CustomGhostMode)).Length;
    }

    public enum CustomGhostMode
    {
        /// <summary>
        /// Let vanilla handle it (no replacement at all).
        /// </summary>
        Vanilla,
        /// <summary>
        /// Same as vanilla, but don't allow for linking graphics.
        /// Useful for non-main graphics like turret tops, since vanilla will treat those as linked graphics.
        /// </summary>
        VanillaNoLinking,
        /// <summary>
        /// Same as the old ShowBlueprintExtension. 
        /// </summary>
        Blueprint,
        /// <summary>
        /// Use custom graphic, either from customGraphicPath or extraCustomGraphicPath
        /// </summary>
        CustomGraphicPath,
        /// <summary>
        /// Uses the GetCustomGraphic method (requires overriding in code). Guaranteed to never change and result will be cached.
        /// </summary>
        CustomGraphicMethodCached,
        /// <summary>
        /// Uses the GetCustomGraphic method (requires overriding in code). It may change, so it'll never be cached.
        /// </summary>
        CustomGraphicMethodNotCached,
    }

    /// <summary>
    /// Data and overrides for the custom graphic path mode.
    /// </summary>
    public class GraphicDataOverride
    {
        /// <summary>
        /// Graphic to use as a replacement. Required to work.
        /// </summary>
        [NoTranslate]
        public string texPath;
        /// <summary>
        /// Graphic class that will be used for the ghost. Defaults to Graphic_Single.
        /// </summary>
        public Type graphicClass;
        /// <summary>
        /// If specified, this will be used for the ghost's draw size rather than using the value of its ThingDef.
        /// </summary>
        public Vector2? drawSize;
        /// If specified, this will enable/disable rotation rather than using the value of its ThingDef.
        public bool? drawRotated;
        /// If specified, this will enable/disable flipping of the graphic rather than using the value of its ThingDef.
        public bool? allowFlip;
    }
}