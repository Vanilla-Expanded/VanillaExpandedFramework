using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Things;

public class Graphic_CustomFireFlicker : Graphic_Collection
{
    private const int BaseTicksPerFrameChange = 15;

    private const float MaxOffset = 0.05f;

    public override Material MatSingle => subGraphics[Rand.Range(0, subGraphics.Length)].MatSingle;

    public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
    {
        if (thingDef == null)
        {
            Log.ErrorOnce("Fire DrawWorker with null thingDef: " + loc, 3427324);
            return;
        }

        if (subGraphics == null)
        {
            Log.ErrorOnce("Graphic_Flicker has no subgraphics " + thingDef, 358773632);
            return;
        }

        var ticksGame = Find.TickManager.TicksGame;
        if (thing != null)
            ticksGame += Mathf.Abs(thing.thingIDNumber ^ 8453458);

        ticksGame /= BaseTicksPerFrameChange;
        var graphicIndex = Mathf.Abs(ticksGame ^ (thing?.thingIDNumber ?? 0) * 391) % subGraphics.Length;
        var fireSize = 1f;
        CompFireOverlayBase compFireOverlayBase = null;
        var fire = thing as CustomFire;
        var compProperties = thingDef.GetCompProperties<CompProperties_FireOverlay>();

        if (fire != null)
        {
            fireSize = fire.fireSize;
        }
        else if (thing != null)
        {
            compFireOverlayBase = thing.TryGetComp<CompFireOverlayBase>();
            if (compFireOverlayBase != null)
            {
                fireSize = compFireOverlayBase.FireSize;
            }
            else
            {
                compFireOverlayBase = thing.TryGetComp<CompDarklightOverlay>();
                if (compFireOverlayBase != null)
                    fireSize = compFireOverlayBase.FireSize;
            }
        }
        else if (compProperties != null)
        {
            fireSize = compProperties.fireSize;
        }

        if (graphicIndex < 0 || graphicIndex >= subGraphics.Length)
        {
            Log.ErrorOnce($"Fire drawing out of range: {graphicIndex}", 7453435);
            graphicIndex = 0;
        }

        var subGraphic = subGraphics[graphicIndex];
        var trsVectorValue = compFireOverlayBase == null ? Mathf.Min(fireSize / 1.2f, 1.2f) : fireSize;
        var radialOffset = GenRadial.RadialPattern[ticksGame % GenRadial.RadialPattern.Length].ToVector3() / GenRadial.MaxRadialPatternRadius * MaxOffset;

        var pos = loc + radialOffset * fireSize;
        if (thing?.Graphic?.data != null)
            pos += thing.Graphic.data.DrawOffsetForRot(rot);

        if (compFireOverlayBase != null)
            pos += compFireOverlayBase.Props.DrawOffsetForRot(rot);

        var matrix = new Matrix4x4();
        matrix.SetTRS(pos, Quaternion.identity, new Vector3(trsVectorValue, 1f, trsVectorValue));
        UnityEngine.Graphics.DrawMesh(MeshPool.plane10, matrix, subGraphic.MatSingle, 0);
    }

    public override string ToString() => $"Flicker(subGraphic[0]={subGraphics[0]}, count={subGraphics.Length})";
}