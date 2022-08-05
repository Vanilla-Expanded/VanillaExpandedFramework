namespace VFECore
{
    using UnityEngine;
    using Verse;

    public class Graphic_Animated : Graphic_Collection
    {
        private readonly int offset = Rand.Range(1, 1000);

        public override Material MatSingle => this.CurFrame?.MatSingle;
        private Graphic CurFrame =>
            this.subGraphics?[
                Mathf.FloorToInt(((Current.Game?.tickManager?.TicksGame ?? 0f) + this.offset) / ((GraphicData_Animated) this.data).ticksPerFrame) %
                this.subGraphics.Length];
        public int SubGraphicCount => this.subGraphics.Length - 1;
        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            if (thing is IAnimationOneTime animation)
            {
                var index = animation.CurrentIndex();
                this.subGraphics?[index]?.DrawWorker(loc, rot, thingDef, thing, extraRotation);
            }
            else
            {
                this.CurFrame?.DrawWorker(loc, rot, thingDef, thing, extraRotation);
            }
        }
    }

    public class GraphicData_Animated : GraphicData
    {
        public int  ticksPerFrame;
        public bool random;
    }
}