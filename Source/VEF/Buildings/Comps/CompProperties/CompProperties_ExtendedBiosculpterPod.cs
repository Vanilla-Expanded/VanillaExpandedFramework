using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Buildings;

public class CompProperties_ExtendedBiosculpterPod : CompProperties_BiosculpterPod
{
    public ThingDef copyCyclesFrom = null;

    public bool drawPawn = true;
    public Rot4? pawnFacingDirectionOverride = null;

    public Vector3 pawnOffsetNorth = Vector3.zero;
    public Vector3 pawnOffsetSouth = Vector3.zero;
    public Vector3 pawnOffsetEast = Vector3.zero;
    public Vector3 pawnOffsetWest = Vector3.zero;

    public bool drawBackground = true;
    public Vector3 backgroundOffsetNorth = Vector3.zero;
    public Vector3 backgroundOffsetSouth = Vector3.zero;
    public Vector3 backgroundOffsetEast = Vector3.zero;
    public Vector3 backgroundOffsetWest = Vector3.zero;
    public Vector3 backgroundSize = Vector3.zero;
    public string backgroundMaterialPath = null;
    [Unsaved]
    public Material backgroundMaterial = null;

    public CompProperties_ExtendedBiosculpterPod() => compClass = typeof(CompExtendedBiosculpterPod);

    public override void ResolveReferences(ThingDef parentDef)
    {
        base.ResolveReferences(parentDef);

        try
        {
            if (copyCyclesFrom?.comps != null)
            {
                foreach (var comp in copyCyclesFrom.comps)
                {
                    if (comp is not CompProperties_BiosculpterPod_BaseCycle { key: not null } cycle)
                        continue;
                    if (parentDef.comps.OfType<CompProperties_BiosculpterPod_BaseCycle>().Any(x => x.key == cycle.key))
                        continue;

                    parentDef.comps.Add(Gen.MemberwiseClone(cycle));
                }
            }
        }
        catch (Exception e)
        {
            Log.Error($"Error occured trying to copy vanilla BiosculpterPod cycles to {parentDef.defName}:\n{e}");
        }

        if (backgroundSize.x <= 0f || backgroundSize.y <= 0f || backgroundSize.z <= 0f)
            backgroundSize = new Vector3(parentDef.graphicData.drawSize.x * 0.8f, 1f, parentDef.graphicData.drawSize.y * 0.8f);

        if (!backgroundMaterialPath.NullOrEmpty())
            backgroundMaterial = MaterialPool.MatFrom(backgroundMaterialPath);
        if (backgroundMaterial.NullOrBad())
            backgroundMaterial = (Material)typeof(CompBiosculpterPod).DeclaredField("BackgroundMat").GetValue(null);
    }

    public Vector3 PawnOffsetFor(Rot4 rotation)
    {
        return rotation.AsInt switch
        {
            Rot4.NorthInt => pawnOffsetNorth,
            Rot4.SouthInt => pawnOffsetSouth,
            Rot4.EastInt => pawnOffsetEast,
            Rot4.WestInt => pawnOffsetWest,
            _ => Vector3.zero,
        };
    }

    public Vector3 BackgroundOffsetFor(Rot4 rotation)
    {
        return rotation.AsInt switch
        {
            Rot4.NorthInt => backgroundOffsetNorth,
            Rot4.SouthInt => backgroundOffsetSouth,
            Rot4.EastInt => backgroundOffsetEast,
            Rot4.WestInt => backgroundOffsetWest,
            _ => Vector3.zero,
        };
    }
}