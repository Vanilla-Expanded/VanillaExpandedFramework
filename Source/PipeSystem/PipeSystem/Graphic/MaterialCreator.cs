using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Create material with given color for each ResourceDef
    /// </summary>
    [StaticConstructorOnStartup]
    public class MaterialCreator
    {
        public static readonly Material BarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));
        public static readonly Material BarFallbackMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.9f, 0.85f, 0.2f));

        public static Material transferMat;

        public static Dictionary<PipeNetDef, Material> materials = new Dictionary<PipeNetDef, Material>();

        static MaterialCreator()
        {
            transferMat = MaterialPool.MatFrom("UI/TransferStorageContent", ShaderDatabase.MetaOverlay);

            var pipeNetDefs = DefDatabase<PipeNetDef>.AllDefsListForReading;
            for (int i = 0; i < pipeNetDefs.Count; i++)
            {
                var pipeNetDef = pipeNetDefs[i];
                materials.Add(pipeNetDef, SolidColorMaterials.SimpleSolidColorMaterial(pipeNetDef.resource.color));

                if (pipeNetDef.resource.offTexPath != null)
                    pipeNetDef.offMat = MaterialPool.MatFrom(pipeNetDef.resource.offTexPath, ShaderDatabase.MetaOverlay);
            }

            var things = DefDatabase<ThingDef>.AllDefsListForReading;
            for (var i = 0; i < things.Count; i++)
            {
                if (things[i].GetCompProperties<CompProperties_ResourceStorage>() is CompProperties_ResourceStorage cP
                    && cP.extractOptions != null)
                {
                    cP.extractOptions.tex = ContentFinder<Texture2D>.Get(cP.extractOptions.texPath);
                }
            }
        }
    }
}