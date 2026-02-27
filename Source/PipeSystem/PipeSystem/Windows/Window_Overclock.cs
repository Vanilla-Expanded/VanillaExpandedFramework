using RimWorld;
using UnityEngine;
using Verse;

namespace PipeSystem
{
    public class Window_Overclock : Window
    {


        public override Vector2 InitialSize => new Vector2(500f, 180f);
        private Vector2 scrollPosition = new Vector2(0, 0);

        CompAdvancedResourceProcessor building;

        private static readonly Color borderColor = new Color(0.13f, 0.13f, 0.13f);
        private static readonly Color fillColor = new Color(0, 0, 0, 0.1f);

        public Window_Overclock(CompAdvancedResourceProcessor building)
        {

            this.building = building;
            draggable = false;
            resizeable = false;
            preventCameraMotion = false;
            closeOnClickedOutside = true;
        }


        public override void DoWindowContents(Rect inRect)
        {

            var outRect = new Rect(inRect);
            outRect.yMin += 40f;
            outRect.yMax -= 40f;
            outRect.width -= 16f;

            Text.Font = GameFont.Medium;
            var IntroLabel = new Rect(0, 0, 300, 32f);
            Widgets.Label(IntroLabel, building.Props.overclockWindowLabel.Translate().CapitalizeFirst());
            Text.Font = GameFont.Small;
            var IntroLabel2 = new Rect(0, 40, 450, 64f);
            Widgets.Label(IntroLabel2, building.Props.overclockDesc.Translate(building.overclockMultiplier.ToStringPercent()).CapitalizeFirst());      
          
            if (Widgets.ButtonImage(new Rect(outRect.xMax - 18f - 4f, 2f, 18f, 18f), TexButton.CloseXSmall))
            {
               
                Close();
            }
            var SliderContainer1 = new Rect(0, 100, 450, 32f);
            HorizontalSliderLabeled(SliderContainer1, ref building.overclockMultiplier, new FloatRange(building.Props.minOverclock, building.Props.maxOverclock), building.Props.minOverclock.ToStringPercent(), building.Props.maxOverclock.ToStringPercent(), roundTo:0.1f);

        }

        public static void HorizontalSliderLabeled(Rect rect, ref float value, FloatRange range, string leftLabel, string righLabel, string label = null, float roundTo = -1f)
        {
            float trueMin = range.TrueMin;
            float trueMax = range.TrueMax;
            value = Widgets.HorizontalSlider(rect, value, trueMin, trueMax, middleAlignment: false, label, leftLabel, righLabel, roundTo);
        }
    }
}