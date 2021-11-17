using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_PawnOverlay : CompProperties_FireOverlay
    {
        public List<GraphicData> graphicElements;

        public CompProperties_PawnOverlay()
        {
            this.compClass = typeof(CompPawnOverlay);
        }

        public override void DrawGhost(IntVec3 center, Rot4 rot, ThingDef thingDef, Color ghostCol, AltitudeLayer drawAltitude, Thing thing = null)
        {
            Vector3 centerVec = center.ToVector3ShiftedWithAltitude(drawAltitude);
            for (int i = 0; i < graphicElements.Count; i++)
            {
                GhostUtility.GhostGraphicFor(graphicElements[i].Graphic, thingDef, ghostCol).DrawFromDef(centerVec, rot, thingDef);
                centerVec.y += 0.04054054f;
            }
        }
    }
}