namespace VFECore
{
    using UnityEngine;
    using Verse;

    public class Graphic_Fleck_Animated : Graphic_FleckCollection
    {
        public override void DrawFleck(FleckDrawData drawData, DrawBatch batch)
        {
            GraphicData_Animated dataAnimated = (GraphicData_Animated) this.data;

            float curTick = Current.Game?.tickManager?.TicksGame ?? 0f;
            int   frame;
            if (dataAnimated.random)
                frame = Mathf.FloorToInt(curTick / dataAnimated.ticksPerFrame) % this.subGraphics.Length;
            else
                frame = Mathf.FloorToInt(drawData.ageSecs * 60f / dataAnimated.ticksPerFrame) % this.subGraphics.Length;
            //Log.Message($"curTick: {curTick}, ageSecs: {drawData.ageSecs}, frame: {frame}, frames: {this.subGraphics.Length}");
            this.subGraphics?[frame].DrawFleck(drawData, batch);
        }
    }
}