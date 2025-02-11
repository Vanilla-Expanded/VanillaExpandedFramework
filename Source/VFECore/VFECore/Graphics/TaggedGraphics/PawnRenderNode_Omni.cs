using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;

namespace VFECore
{
    /// <summary>
    /// Advanced PawnRenderNode for conditional picking of data according to complex setting such as DLC availability, faction, etc.
    /// 
    /// Designed to be a bit less verbose and... overkill than the variants used in Big and Small.
    /// 
    /// 
    /// 
    /// - RedMattis, 2025-01-09
    /// </summary>
    public class PawnRenderNode_Omni : PawnRenderNode
    {
        private bool useHeadMesh;

        PawnRenderNodeProperties_Omni OProps => props as PawnRenderNodeProperties_Omni;

        public PawnRenderNode_Omni(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree) { }
        public PawnRenderNode_Omni(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree, Apparel apparel) : base(pawn, props, tree)
        {
            base.apparel = apparel;
            useHeadMesh = props.parentTagDef == PawnRenderNodeTagDefOf.ApparelHead;
            meshSet = MeshSetFor(pawn);
        }
        public PawnRenderNode_Omni(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree, Apparel apparel, bool useHeadMesh) : base(pawn, props, tree)
        {
            base.apparel = apparel;
            this.useHeadMesh = useHeadMesh;
            meshSet = MeshSetFor(pawn);
        }

        protected override string TexPathFor(Pawn pawn)
        {
            throw new NotImplementedException($"TexPath is not meant to be used with the {nameof(PawnRenderNode_Omni)} RenderNode.");
        }

        public override Graphic GraphicFor(Pawn pawn)
        {
            var activeSet = OProps.conditionalGraphics.GetActiveGraphicsSet(pawn, this);

            if (activeSet == null)
            {
                Log.Warning($"No active set of graphics found for {pawn} in {this}");
                return null;
            }

            var texPath = activeSet.TexPathFor(pawn, this);
            var maskPath = activeSet.MaskPathFor(pawn, this);
            var shader = activeSet.ShaderFor(pawn);
            shader ??= ShaderTypeDefOf.CutoutComplex.Shader;
            var colorA = activeSet.GetColorA(this, Color.white);
            var colorB = activeSet.GetColorB(this, Color.white);
            if (!texPath.NullOrEmpty() && pawn.GetStringByTag(texPath) is TaggedText newPath)
            {
                // If the path is actually a tag, AND the tag is found, use that instead.
                texPath = newPath.value;
            }
            if (!maskPath.NullOrEmpty() && pawn.GetStringByTag(maskPath) is TaggedText newMaskPath)
            {
                // If the path is actually a tag, AND the tag is found, use that instead.
                maskPath = newMaskPath.value;
            }
            if (OProps.autoBodyTypeMasks == true)
            {
                maskPath ??= texPath; // In the unlikely event that the masks have bodytypes but the texPath doesn't.
                maskPath = GetBodyTypedPath(pawn.story.bodyType, maskPath);
            }
            if (OProps.autoBodyTypePaths == true)
            {
                texPath = GetBodyTypedPath(pawn.story.bodyType, texPath);
            }
            //Log.Message($"DEBUG: Getting graphic for {pawn} with texPath {texPath}, maskPath {maskPath}, shader {shader}, colorA {colorA}, colorB {colorB}");
            return GraphicDatabase.Get<Graphic_Multi>(texPath, shader, Vector2.one, colorA, colorB, null, maskPath: maskPath);
        }

        public override GraphicMeshSet MeshSetFor(Pawn pawn)
        {
            if (apparel == null)
            {
                return base.MeshSetFor(pawn);
            }
            if (Props.overrideMeshSize.HasValue)
            {
                return MeshPool.GetMeshSetForSize(base.Props.overrideMeshSize.Value.x, base.Props.overrideMeshSize.Value.y);
            }
            if (useHeadMesh)
            {
                return HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn(pawn);
            }
            return HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn(pawn);
        }

        public string GetBodyTypedPath(BodyTypeDef bodyType, string basePath)
        {
            if (bodyType == null)
            {
                Log.Error("Attempted to get graphic with undefined body type.");
                bodyType = BodyTypeDefOf.Male;
            }
            if (basePath.NullOrEmpty())
            {
                return basePath;
            }
            return basePath + "_" + bodyType.defName;
        }
    }

    public class PawnRenderNodeProperties_Omni : PawnRenderNodeProperties
    {
        public ConditionalGraphicSet conditionalGraphics = new();
        public bool autoBodyTypePaths = false;  
        public bool autoBodyTypeMasks = false;
    }

    /// <summary>
    /// Pure Copy-Paste from PawnRenderNodeWorker_Apparel_Body with just the PawnRenderNode_Omni casted instead.
    /// </summary>
    public class PawnRenderNodeWorker_OmniBodyApparel : PawnRenderNodeWorker_Body
    {
        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            if (!base.CanDrawNow(node, parms))
            {
                return false;
            }
            if (!parms.flags.FlagSet(PawnRenderFlags.Clothes))
            {
                return false;
            }
            return true;
        }

        public override Vector3 OffsetFor(PawnRenderNode n, PawnDrawParms parms, out Vector3 pivot)
        {
            Vector3 result = base.OffsetFor(n, parms, out pivot);
            PawnRenderNode_Omni pawnRenderNode_Apparel = (PawnRenderNode_Omni)n;
            if (pawnRenderNode_Apparel.apparel.def.apparel.wornGraphicData != null && pawnRenderNode_Apparel.apparel.RenderAsPack())
            {
                Vector2 vector = pawnRenderNode_Apparel.apparel.def.apparel.wornGraphicData.BeltOffsetAt(parms.facing, parms.pawn.story.bodyType);
                result.x += vector.x;
                result.z += vector.y;
            }
            return result;
        }

        public override Vector3 ScaleFor(PawnRenderNode n, PawnDrawParms parms)
        {
            Vector3 result = base.ScaleFor(n, parms);
            PawnRenderNode_Omni pawnRenderNode_Apparel = (PawnRenderNode_Omni)n;
            if (pawnRenderNode_Apparel.apparel.def.apparel.wornGraphicData != null && pawnRenderNode_Apparel.apparel.RenderAsPack())
            {
                Vector2 vector = pawnRenderNode_Apparel.apparel.def.apparel.wornGraphicData.BeltScaleAt(parms.facing, parms.pawn.story.bodyType);
                result.x *= vector.x;
                result.z *= vector.y;
            }
            return result;
        }

        public override float LayerFor(PawnRenderNode n, PawnDrawParms parms)
        {
            if (parms.flipHead && n.Props.oppositeFacingLayerWhenFlipped)
            {
                PawnDrawParms parms2 = parms;
                parms2.facing = parms.facing.Opposite;
                parms2.flipHead = false;
                return base.LayerFor(n, parms2);
            }
            return base.LayerFor(n, parms);
        }
    }
}
