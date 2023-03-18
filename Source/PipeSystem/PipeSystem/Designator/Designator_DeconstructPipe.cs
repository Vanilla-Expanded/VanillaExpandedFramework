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
            var baseTexture = ContentFinder<Texture2D>.Get(pipeNet.designator.deconstructIconPath);
            if (pipeNetDef.designator.overrideColor.Equals(Color.clear))
            {
                icon = baseTexture;
            }
            else
            {
                Log.Message($"Designator_DeconstructPipe - creating color-shifted icon for {pipeNetDef.defName}");
                var mat = MaterialPool.MatFrom(pipeNetDef.designator.deconstructIconPath, Shader.Find("Sprites/Default"), pipeNetDef.designator.overrideColor);
                var renderTexture = RenderTexture.GetTemporary(baseTexture.width, baseTexture.height);

                Graphics.Blit(baseTexture, renderTexture, mat);

                var outputTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
                outputTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                outputTexture.Apply();

                icon = outputTexture;

                RenderTexture.ReleaseTemporary(renderTexture);

            }
            hotKey = null;
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