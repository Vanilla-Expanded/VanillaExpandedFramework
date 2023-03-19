using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Create a deconstruct designator specific for a pipe def.
    /// </summary>
    public class Designator_DeconstructPipe : Designator_Deconstruct
    {
        public readonly PipeNetDef pipeNetDef;

        public Designator_DeconstructPipe(PipeNetDef pipeNet)
        {
            pipeNetDef = pipeNet;
            defaultLabel = "PipeSystem_DeconstructLabel".Translate(pipeNet.resource.name);
            defaultDesc = "PipeSystem_DeconstructDesc".Translate(pipeNet.resource.name);
            hotKey = null;

            var baseTexture = ContentFinder<Texture2D>.Get(pipeNet.designator.deconstructIconPath);
            if (string.IsNullOrEmpty(pipeNetDef.designator.shaderPath))
            {
                icon = baseTexture;
            }
            else
            {
                Log.Message($"Designator_DeconstructPipe - creating color-shifted icon for {pipeNetDef.defName}");

                var shader = Shader.Find(pipeNetDef.designator.shaderPath);
                if (shader == null)
                {
                    Log.Warning($"Designator_DeconstructPipe - invalid shader path {pipeNetDef.designator.shaderPath} - falling back to stock icon");
                    icon = baseTexture;
                    return;
                }

                var mat = MaterialPool.MatFrom(pipeNetDef.designator.deconstructIconPath, shader);
                mat.SetColor(ShaderPropertyIDs.Color, pipeNetDef.designator.color);

                // TODO: This checks specific, hardcoded shaders. It probably won't support fancy custom ones. It does work for CutoutComplex
                if (ShaderUtility.SupportsMaskTex(shader))
                {
                    Log.Message($"Designator_DeconstructPipe - complex shader requested '{pipeNetDef.designator.shaderPath}' Mask Path: {pipeNet.designator.maskTexturePath} /// color2: {pipeNet.designator.colorTwo}");
                    var maskTexture = ContentFinder<Texture2D>.Get(pipeNet.designator.maskTexturePath);
                    mat.SetTexture(ShaderPropertyIDs.MaskTex, maskTexture);
                    mat.SetColor(ShaderPropertyIDs.ColorTwo, pipeNetDef.designator.colorTwo);
                }

                var renderTexture = RenderTexture.GetTemporary(baseTexture.width, baseTexture.height);

                Graphics.Blit(baseTexture, renderTexture, mat);

                var outputTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
                outputTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                outputTexture.Apply();

                icon = outputTexture;

                RenderTexture.ReleaseTemporary(renderTexture);

            }
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            if (!pipeNetDef.pipeDefs.Contains(t.def)) return false;
            if (t is Blueprint_Build blueprint && !pipeNetDef.pipeDefs.Contains(blueprint.def.entityDefToBuild)) return false;
            return base.CanDesignateThing(t);
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
            SectionLayer_Resource.UpdateAndDrawFor(pipeNetDef);
        }
    }
}