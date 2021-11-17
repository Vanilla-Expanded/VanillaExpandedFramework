using RimWorld;
using UnityEngine;
using Verse;

namespace AnimalBehaviours
{
    [StaticConstructorOnStartup]
    public class CompPawnOverlay : CompFireOverlayBase
    {
        public new CompProperties_PawnOverlay Props => (CompProperties_PawnOverlay)props;

        public override void PostDraw()
        {
            base.PostDraw();
            CompProperties_PawnOverlay props = Props;
            Vector3 drawPos = parent.DrawPos;
            for (int i = 0; i < props.graphicElements.Count; i++)

            {
                drawPos.y += 0.04054054f;
                props.graphicElements[i].Graphic.Draw(drawPos, parent.Rotation, (Thing)this.parent);
            }
        }
    }
}