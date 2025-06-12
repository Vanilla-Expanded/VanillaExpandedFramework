using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;
using RimWorld;
using static UnityEngine.CullingGroup;
using HarmonyLib;

namespace VEF.Graphics
{
    [HarmonyPatch]
    public static class VanillaExpandedFramework_DynamicGraphicPatches
    {
        // This should probably be replaced with a Transpiler, so we don't reinvent the wheel so much. We've feature creeped well enough for now...
        [HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAiming))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        public static bool DrawEquipmentAimingPrefix(Thing eq, Vector3 drawLoc, float aimAngle)
        {
            if (eq is DynamicGraphicThing dgt)
            {
                foreach (var graphic in dgt.GetDynamicGraphics())
                {
                    float targetAngle = aimAngle - 90f;
                    Mesh mesh;
                    if (aimAngle > 20f && aimAngle < 160f)
                    {
                        mesh = MeshPool.plane10;
                        targetAngle += eq.def.equippedAngleOffset;
                    }
                    else if (aimAngle > 200f && aimAngle < 340f)
                    {
                        mesh = MeshPool.plane10Flip;
                        targetAngle -= 180f;
                        targetAngle -= eq.def.equippedAngleOffset;
                    }
                    else
                    {
                        mesh = MeshPool.plane10;
                        targetAngle += eq.def.equippedAngleOffset;
                    }
                    targetAngle %= 360f;
                    CompEquippable compEquippable = eq.TryGetComp<CompEquippable>();
                    if (compEquippable != null)
                    {
                        EquipmentUtility.Recoil(eq.def, EquipmentUtility.GetRecoilVerb(compEquippable.AllVerbs), out var drawOffset, out var angleOffset, aimAngle);
                        drawLoc += drawOffset;
                        targetAngle += angleOffset;
                    }
                    var preRotationOffset = graphic.DrawOffset(Rot4.South);
                    var postRotationOffset = preRotationOffset.RotatedBy(targetAngle);

                    Material material = graphic.MatSingleFor(eq);
                    Matrix4x4 matrix = Matrix4x4.TRS(s: new Vector3(graphic.drawSize.x, 0f, graphic.drawSize.y), pos: drawLoc + postRotationOffset, q: Quaternion.AngleAxis(targetAngle, Vector3.up));
                    UnityEngine.Graphics.DrawMesh(mesh, matrix, material, 0);
                }
                return false;
            }
            return true;
        }
    }

    public interface IDynamicGraphic
    {
        public List<Graphic> GetDynamicGraphics();
    }

    public class DynamicGraphicBuilding : Building, IDynamicGraphic
    {
        protected readonly DynamicGraphicBaseThing baseThing = new();

        public List<Graphic> GetDynamicGraphics() => baseThing.DynamicGraphics(this);
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            List<Graphic> idx = baseThing.DynamicGraphics(this);
            for (int i = 0; i < idx.Count; i++)
            {
                Graphic graphic = idx[i];
                if (graphic == null) continue;
                graphic.Draw(drawLoc + Altitudes.AltIncVect * i, Rotation, this);
            }
        }

        public override void Notify_ColorChanged()
        {
            baseThing.Dirty();
            base.Notify_ColorChanged();
        }
    }
    public class DynamicGraphicThing : ThingWithComps, IDynamicGraphic
    {
        protected readonly DynamicGraphicBaseThing baseThing = new();
        Pawn pawn = null;
        Faction faction = null;
        bool stateChanged = true;

        public List<Graphic> GetDynamicGraphics()
        {
            var result = baseThing.DynamicGraphics(this, force: stateChanged, parentThing: pawn, faction: faction);
            stateChanged = false;
            return result;
        }
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            List<Graphic> idx = baseThing.DynamicGraphics(this, force: stateChanged, parentThing: pawn, faction: faction);
            for (int i = 0; i < idx.Count; i++)
            {
                Graphic graphic = idx[i];
                if (graphic == null) continue;
                graphic.Draw(drawLoc + Altitudes.AltIncVect * i, Rotation, this);
            }
            stateChanged = false;
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            this.pawn = pawn;
            faction = pawn.Faction; // Mostly so it has a fallback if the pawn gets deleted.
            base.Notify_Equipped(pawn);
            stateChanged = true;
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            this.pawn = null;
            // Still saves faction so not-ownable stuff will retain it as a fallback.
        }

        public override void Notify_ColorChanged()
        {
            baseThing.Dirty();
            base.Notify_ColorChanged();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_References.Look(ref faction, "faction");
        }
    }

    public class DynamicGraphicBaseThing
    {
        private List<ExtendedGraphicData> dynamicGraphicsData = null;
        private List<Graphic> dynamicGraphics = null;

        protected List<ExtendedGraphicData> DynamicGraphicsData(Def def)
        {
            if (dynamicGraphicsData == null)
            {
                dynamicGraphicsData = [];
                foreach (var extension in def.GetModExtensions<DynamicGraphicProps>())
                {
                    dynamicGraphicsData.AddRange(extension.dataList);
                }
            };
            return dynamicGraphicsData;
        }
        public List<Graphic> DynamicGraphics(Thing thing, bool force=false, Thing parentThing = null, Faction faction=null)
        {
            if (dynamicGraphics == null || force)
            {
                dynamicGraphics = [];
                var dynamicData = DynamicGraphicsData(thing.def);
                if (dynamicData.Count == 0)
                {
                    dynamicGraphics = [];
                    throw (new Exception($"Thing {thing.def.defName} is {GetType()} but declares no {typeof(DynamicGraphicProps)} entries!"));
                }
                foreach (var graphicData in dynamicData)
                {
                    dynamicGraphics.Add(GenerateDynamicGraphic(thing, graphicData, parentThing, faction));
                }
            }
            return dynamicGraphics.Where(x=>x != null).ToList();
        }

        public void Dirty()
        {
            dynamicGraphics = null;
        }

        public Graphic GenerateDynamicGraphic(Thing thing, ExtendedGraphicData data, Thing pThing = null, Faction pFaction = null)
        {
            var baseData = thing.def.graphicData;

            Color colorA = data.color;
            Color colorB = data.colorTwo;
            string texPath = data.texPath;
            string maskPath = data.maskPath;
            Shader shader = data.shaderType?.Shader;
            shader ??= ShaderTypeDefOf.CutoutComplex.Shader;


            Color? colorAOverride = null;
            Color? colorBOverride = null;
            string texPathOverride = null;
            string maskPathOverride = null;

            GetTagged(data, thing, ref colorAOverride, ref colorBOverride, ref texPathOverride, ref maskPathOverride);

            if (pThing != null)
            {
                GetTagged(data, pThing, ref colorAOverride, ref colorBOverride, ref texPathOverride, ref maskPathOverride);
            }
            else if (pFaction is Faction pFa) // In case the pawn is null, but the faction is not.
            {
                GetTagged(data, pFa, ref colorAOverride, ref colorBOverride, ref texPathOverride, ref maskPathOverride);
            }
            colorA = colorAOverride ?? colorA;
            colorB = colorBOverride ?? colorB;
            texPath = texPathOverride.NullOrEmpty() ? texPath : texPathOverride;
            maskPath = maskPathOverride.NullOrEmpty() ? maskPath : maskPathOverride;

            Log.Message($"DEBUG: Getting graphic for {thing}.\n" +
                $"BaseData: {baseData}\n" +
                $"Data: {data}\n" +
                $"ColorA: {colorA}\n" +
                $"ColorB: {colorB}\n" +
                $"TexPath: {texPath}\n" +
                $"MaskPath: {maskPath}\n" +
                $"Shader: {shader}\n" +
                $"Overrides: {colorAOverride}, {colorBOverride}, {texPathOverride}, {maskPathOverride}");

            if (texPath.NullOrEmpty())
            {
                return null;
            }

            if (data.graphicClass == typeof(Graphic_Multi))
            {
                return GraphicDatabase.Get<Graphic_Multi>(texPath, shader, data.drawSizeAbsolute ?? data.drawSize * baseData.drawSize, colorA, colorB, data, maskPath: maskPath);
            }
            else
            {
                return GraphicDatabase.Get<Graphic_Single>(texPath, shader, data.drawSizeAbsolute ?? data.drawSize * baseData.drawSize, colorA, colorB, data, maskPath: maskPath);
            }

            static void GetTagged(ExtendedGraphicData data, ILoadReferenceable tagThing, ref Color? colorAOverride, ref Color? colorBOverride, ref string texPathOverride, ref string maskPathOverride)
            {
                if (data.taggedColorA != null && colorAOverride == null && tagThing.GetColorByTag(data.taggedColorA) is TaggedColor taggedColorA)
                {
                    colorAOverride = taggedColorA.value;
                }
                if (data.taggedColorB != null && colorBOverride == null && tagThing.GetColorByTag(data.taggedColorB) is TaggedColor taggedColorB)
                {
                    colorBOverride = taggedColorB.value;
                }
                if (data.taggedTexPath != null && texPathOverride.NullOrEmpty() && tagThing.GetStringByTag(data.taggedTexPath, x => x != null
                    && ContentFinder<Texture2D>.Get(x?.value, reportFailure:false) != null) is TaggedText taggedTexPath)
                {
                    texPathOverride = taggedTexPath.value;
                }
                if (data.taggedMaskPath != null && maskPathOverride.NullOrEmpty() && tagThing.GetStringByTag(data.taggedMaskPath, x => x != null
                    && ContentFinder<Texture2D>.Get(x?.value, reportFailure: false) != null) is TaggedText taggedMaskPath)
                {
                    maskPathOverride = taggedMaskPath.value;
                }
            }

        }

        
    }

    public class DynamicGraphicProps : DefModExtension
    {
        public List<ExtendedGraphicData> dataList;
    }

    public class ExtendedGraphicData : GraphicData
    {
        public string taggedColorA;
        public string taggedColorB;
        public string taggedTexPath;
        public string taggedMaskPath;

        public Vector2? drawSizeAbsolute = null;
    }
}
