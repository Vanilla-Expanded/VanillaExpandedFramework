using UnityEngine;
using Verse;

namespace VFECore.UItils
{
    public static class UIUtility
    {
        public static Rect TakeTopPart(ref this Rect rect, float pixels)
        {
            var ret = rect.TopPartPixels(pixels);
            rect.yMin += pixels;
            return ret;
        }

        public static Rect TakeBottomPart(ref this Rect rect, float pixels)
        {
            var ret = rect.BottomPartPixels(pixels);
            rect.yMax -= pixels;
            return ret;
        }

        public static Rect TakeRightPart(ref this Rect rect, float pixels)
        {
            var ret = rect.RightPartPixels(pixels);
            rect.xMax -= pixels;
            return ret;
        }

        public static Rect TakeLeftPart(ref this Rect rect, float pixels)
        {
            var ret = rect.LeftPartPixels(pixels);
            rect.xMin += pixels;
            return ret;
        }

        public static void DrawCountAdjuster(ref int value, Rect inRect, ref string buffer, int min, int max, bool readOnly = false, int? setToMin = null, int? setToMax = null)
        {
            var temp = value;
            var rect = inRect.ContractedBy(50f, 0);
            var leftBigRect = rect.LeftPartPixels(30f);
            rect.xMin += 30f;
            var leftSmallRect = rect.LeftPartPixels(30f);
            rect.xMin += 30f;
            var rightBigRect = rect.RightPartPixels(30f);
            rect.xMax -= 30f;
            var rightSmallRect = rect.RightPartPixels(30f);
            rect.xMax -= 30f;
            var mult = GenUI.CurrentAdjustmentMultiplier();
            if (!readOnly && (setToMin.HasValue ? value > setToMin.Value : value != min) && Widgets.ButtonText(leftBigRect, "<<")) value = setToMin ?? min;
            if (!readOnly && value - mult >= min && Widgets.ButtonText(leftSmallRect, "<")) value -= mult;
            if (!readOnly && (setToMax.HasValue ? value < setToMax.Value : value != max) && Widgets.ButtonText(rightBigRect, ">>")) value = setToMax ?? max;
            if (!readOnly && value + mult <= max && Widgets.ButtonText(rightSmallRect, ">")) value += mult;
            if (value < min) value = min;
            if (value > max) value = max;
            if (value != temp || readOnly) buffer = value.ToString();
            Widgets.TextFieldNumeric(rect.ContractedBy(3f, 0f), ref temp, ref buffer, min, max);
            if (!readOnly) value = temp;
        }
    }
}