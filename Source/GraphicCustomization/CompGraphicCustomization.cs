using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace GraphicCustomization
{
    public class CompProperties_GraphicCustomization : CompProperties
    {
        public List<GraphicPart> graphics;

        public bool customizable;

        public string customizationTitle;
        public CompProperties_GraphicCustomization()
        {
            compClass = typeof(CompGraphicCustomization);
        }
    }
    public class CompGraphicCustomization : ThingComp
    {
        public List<string> texPaths;
        public List<TextureVariant> texVariants;
        public List<TextureVariant> texVariantsToCustomize;
        public CompProperties_GraphicCustomization Props => base.props as CompProperties_GraphicCustomization;

        public Graphic graphicInt;
        public Graphic Graphic
        {
            get
            {
                if (graphicInt is null)
                {
                    TryInit();
                    var graphicRequest = new GraphicRequest(this.parent.def.graphicData.graphicClass, this.parent.def.graphicData.texPath,
                        this.parent.def.graphicData.shaderType.Shader, this.parent.def.graphicData.drawSize, this.parent.def.graphicData.color,
                        this.parent.def.graphicData.colorTwo, this.parent.def.graphicData, 0, this.parent.def.graphicData.shaderParameters, null);
                    graphicInt = GetGraphic(graphicRequest);
                }
                return graphicInt;
            }
        }

        public void TryInit()
        {
            if (texPaths.NullOrEmpty())
            {
                texVariants = GetRandomizedTexVariants();
                texPaths = GetTexPaths(texVariants);
            }
        }
        
        public List<string> GetTexPaths(List<TextureVariant> texVariants)
        {
            var texPaths = new List<string>();
            foreach (var texVariant in texVariants)
            {
                texPaths.Add(texVariant.outline);
            }
            foreach (var texVariant in texVariants)
            {
                texPaths.Add(texVariant.texture);
            }
            return texPaths;
        }

        public List<TextureVariant> GetRandomizedTexVariants()
        {
            var randomizedVariants = new Dictionary<string, TextureVariant>();
            foreach (var graphicPart in Props.graphics)
            {
                randomizedVariants[graphicPart.name] = graphicPart.texVariants.RandomElementByWeight((TextureVariant x) => x.chanceOverride);
            }

            List<TextureVariant> variantsToReplace = new List<TextureVariant>();
            foreach (var key in randomizedVariants.Keys.ToList())
            {
                var variant = randomizedVariants[key];
                if (variant.textureVariantOverride != null)
                {
                    if (Rand.Chance(variant.textureVariantOverride.chance))
                    {
                        var group = Props.graphics.First(x => x.name == variant.textureVariantOverride.groupName);
                        randomizedVariants[group.name] = group.texVariants.First(x => x.texName == variant.textureVariantOverride.texName);
                    }
                }
            }
            return randomizedVariants.Values.ToList();
        }

        private Texture2D textureInt;
        public Texture2D Texture
        {
            get
            {
                if (textureInt is null)
                {
                    TryInit();
                    textureInt = GetCombinedTexture(texPaths);
                }
                return textureInt;
            }
        }
        public Graphic_Single GetGraphic(GraphicRequest req)
        {
            var graphic = new Graphic_Single();
            graphic.Init(req);
            MaterialRequest req2 = default(MaterialRequest);
            req2.mainTex = Texture;
            req2.shader = req.shader;
            req2.color = this.parent.DrawColor;
            req2.colorTwo = this.parent.DrawColorTwo;
            req2.renderQueue = req.renderQueue;
            req2.shaderParameters = req.shaderParameters;
            graphic.mat = MaterialPool.MatFrom(req2);
            return graphic;
        }
        
        public Texture2D GetCombinedTexture(List<string> paths)
        {
            var texture = TextureUtils.GetReadableTexture(ContentFinder<Texture2D>.Get(paths[0]));
            for (int i = 1; i < paths.Count; i++)
            {
                var tex = TextureUtils.GetReadableTexture(ContentFinder<Texture2D>.Get(paths[i]));
                texture = TextureUtils.CombineTextures(texture, tex, 0, 0);
            }
            return texture;
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (Props.customizable)
            {
                yield return new FloatMenuOption("VEF.Customize".Translate(this.parent.LabelShort), delegate
                {
                    Find.WindowStack.Add(new Dialog_GraphicCustomization(this, selPawn));
                });
            }
        }

        public void Customize()
        {
            this.texVariants = this.texVariantsToCustomize.ListFullCopy();
            this.texVariantsToCustomize.Clear();
            this.texPaths = GetTexPaths(this.texVariants);
            textureInt = GetCombinedTexture(this.texPaths);
            var graphicRequest = new GraphicRequest(this.parent.def.graphicData.graphicClass, this.parent.def.graphicData.texPath,
                this.parent.def.graphicData.shaderType.Shader, this.parent.def.graphicData.drawSize, this.parent.def.graphicData.color,
                this.parent.def.graphicData.colorTwo, this.parent.def.graphicData, 0, this.parent.def.graphicData.shaderParameters, null);
            graphicInt = GetGraphic(graphicRequest);
            this.parent.graphicInt = graphicInt;
            if (this.parent.Spawned)
            {
                this.parent.Map.mapDrawer.MapMeshDirty(this.parent.Position, MapMeshFlagDefOf.Things);
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref texPaths, "texPaths", LookMode.Value);
            Scribe_Collections.Look(ref texVariants, "texVariants", LookMode.Deep);
            Scribe_Collections.Look(ref texVariantsToCustomize, "texVariantsToCustomize", LookMode.Deep);
        }
    }
}
