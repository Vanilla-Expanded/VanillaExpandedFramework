using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace GraphicCustomization
{
    public class TextureVariant
    {
        public string texName;
        public string texture;
        public string outline;
    }
    public class GraphicPart
    {
        public string name;
        public List<TextureVariant> texVariants;
    }
    public class CompProperties_GraphicCustomization : CompProperties
    {
        public List<GraphicPart> graphics;
        public CompProperties_GraphicCustomization()
        {
            compClass = typeof(CompGraphicCustomization);
        }
    }
    public class CompGraphicCustomization : ThingComp
    {
        public List<string> texPaths;
        public CompProperties_GraphicCustomization Props => base.props as CompProperties_GraphicCustomization;

        public Graphic graphicInt;
        public Graphic Graphic
        {
            get
            {
                if (graphicInt is null)
                {
                    TryInitTexPaths();
                    var graphicRequest = new GraphicRequest(this.parent.def.graphicData.graphicClass, this.parent.def.graphicData.texPath,
                        this.parent.def.graphicData.shaderType.Shader, this.parent.def.graphicData.drawSize, this.parent.def.graphicData.color,
                        this.parent.def.graphicData.colorTwo, this.parent.def.graphicData, 0, this.parent.def.graphicData.shaderParameters, null);
                    graphicInt = GetGraphic(graphicRequest);
                }
                return graphicInt;
            }
        }

        private void TryInitTexPaths()
        {
            if (texPaths.NullOrEmpty())
            {
                texPaths = new List<string>();
                var variants = new List<TextureVariant>();
                foreach (var graphicPart in Props.graphics)
                {
                    variants.Add(graphicPart.texVariants.RandomElement());
                }
                foreach (var variant in variants)
                {
                    texPaths.Add(variant.outline);
                }
                foreach (var variant in variants)
                {
                    texPaths.Add(variant.texture);
                }
            }
        }

        private Texture2D textureInt;
        public Texture2D Texture
        {
            get
            {
                if (textureInt is null)
                {
                    textureInt = GetCombinedTexture();
                }
                return textureInt;
            }
        }
        public Graphic_Single GetGraphic(GraphicRequest req)
        {
            var graphic = new Graphic_Single();
            MaterialRequest req2 = default(MaterialRequest);
            req2.mainTex = Texture;
            req2.shader = req.shader;
            req2.color = graphic.color;
            req2.colorTwo = graphic.colorTwo;
            req2.renderQueue = req.renderQueue;
            req2.shaderParameters = req.shaderParameters;
            graphic.mat = MaterialPool.MatFrom(req2);
            return graphic;
        }
        public Texture2D GetCombinedTexture()
        {
            var texture = GetReadableTexture(ContentFinder<Texture2D>.Get(texPaths[0]));
            for (int i = 1; i < texPaths.Count; i++)
            {
                var tex = GetReadableTexture(ContentFinder<Texture2D>.Get(texPaths[i]));
                texture = CombineTextures(texture, tex, 0, 0);
            }
            return texture;
        }

        public static Texture2D GetReadableTexture(Texture2D texture)
        {
            RenderTexture previous = RenderTexture.active;
            RenderTexture temporary = RenderTexture.GetTemporary(
                    texture.width,
                    texture.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

            Graphics.Blit(texture, temporary);
            RenderTexture.active = temporary;
            Texture2D texture2D = new Texture2D(texture.width, texture.height);
            texture2D.ReadPixels(new Rect(0f, 0f, (float)temporary.width, (float)temporary.height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(temporary);
            return texture2D;
        }
        public static Texture2D CombineTextures(Texture2D background, Texture2D overlay, int startX, int startY)
        {
            Texture2D newTex = new Texture2D(background.width, background.height, background.format, false);
            for (int x = 0; x < background.width; x++)
            {
                for (int y = 0; y < background.height; y++)
                {
                    if (x >= startX && y >= startY && x < overlay.width && y < overlay.height)
                    {
                        Color bgColor = background.GetPixel(x, y);
                        Color wmColor = overlay.GetPixel(x - startX, y - startY);

                        Color final_color = Color.Lerp(bgColor, wmColor, wmColor.a / 1.0f);

                        newTex.SetPixel(x, y, final_color);
                    }
                    else
                        newTex.SetPixel(x, y, background.GetPixel(x, y));
                }
            }

            newTex.Apply();
            return newTex;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref texPaths, "texPaths", LookMode.Value);
        }
    }
}
