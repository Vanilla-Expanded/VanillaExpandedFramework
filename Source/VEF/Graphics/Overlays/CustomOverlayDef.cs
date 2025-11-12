using System;
using RimWorld;
using UnityEngine;
using VEF.Maps;
using Verse;

namespace VEF.Graphics;

public class CustomOverlayDef : Def
{
    /// <summary>
    /// The type of the custom worker. Optional.
    /// </summary>
    public Type workerClass = typeof(CustomOverlayWorker);

    /// <summary>
    /// If true, the overlay will be pulsing (or fading in and out), just like vanilla overlays.
    /// </summary>
    public bool pulsing = true;
    /// <summary>
    /// If true, the overlay will ignore positions of other overlays and use a custom offset.
    /// </summary>
    public bool useCustomOffset = false;
    /// <summary>
    /// The custom offset for the overlay. Can be customized on a per-thing basis by using a custom worker.
    /// </summary>
    public Vector3 customOffset = Vector3.zero;

    /// <summary>
    /// The path to the texture which will be used for the overlay. Optional. Can be customized on a per-thing basis by using a custom worker.
    /// </summary>
    [NoTranslate] public string overlayPath;
    /// <summary>
    /// The shader type which will be used for the overlay. Optional, defaults to MetaOverlay, matching other vanilla overlays.
    /// </summary>
    public ShaderTypeDef shaderType;

    /// <summary>
    /// The cached material setup based on overlayPath. Will be null if overlayPath is null or empty.
    /// </summary>
    public Material CachedMaterial { get; protected set; }
    /// <summary>
    /// The initialized worker.
    /// </summary>
    public CustomOverlayWorker Worker { get; protected set; }

    public override void PostLoad()
    {
        base.PostLoad();

        LongEventHandler.ExecuteWhenFinished(() =>
        {
            Worker = (CustomOverlayWorker)Activator.CreateInstance(workerClass, this);
            // Default to MetaOverlay if not specified
            shaderType ??= ShaderTypeDefOf.MetaOverlay;
            // Init
            if (!overlayPath.NullOrEmpty())
                CachedMaterial = MaterialPool.MatFrom(overlayPath, shaderType.Shader);
        });
    }
}