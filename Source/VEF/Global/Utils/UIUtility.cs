using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace VEF.Utils
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

        // TODO: Remove in next major update
        [OverloadResolutionPriority(-100)]
        [Obsolete("Will be removed. Use the overload with margin arguments. This should be automatic on next recompile due to OverloadResolutionPriority.")]
        public static void DrawCountAdjuster(ref int value, Rect inRect, ref string buffer, int min, int max, bool readOnly = false, int? setToMin = null, int? setToMax = null)
            => DrawCountAdjuster(ref value, inRect, ref buffer, min, max, readOnly, setToMin, setToMax, 50f, 0f);

        public static void DrawCountAdjuster(ref int value, Rect inRect, ref string buffer, int min, int max, bool readOnly = false, int? setToMin = null, int? setToMax = null, float marginX = 50f, float marginY = 50f)
        {
            var temp        = value;
            var rect        = inRect.ContractedBy(marginX, marginY);
            var leftBigRect = rect.LeftPartPixels(30f);
            rect.xMin += 30f;
            var leftSmallRect = rect.LeftPartPixels(30f);
            rect.xMin += 30f;
            var rightBigRect = rect.RightPartPixels(30f);
            rect.xMax -= 30f;
            var rightSmallRect = rect.RightPartPixels(30f);
            rect.xMax -= 30f;
            var mult = GenUI.CurrentAdjustmentMultiplier();
            if (!readOnly && (setToMin.HasValue ? value > setToMin.Value : value != min) && Widgets.ButtonText(leftBigRect,    "<<")) value =  setToMin ?? min;
            if (!readOnly && value - mult >= min                                         && Widgets.ButtonText(leftSmallRect,  "<")) value  -= mult;
            if (!readOnly && (setToMax.HasValue ? value < setToMax.Value : value != max) && Widgets.ButtonText(rightBigRect,   ">>")) value =  setToMax ?? max;
            if (!readOnly && value + mult <= max                                         && Widgets.ButtonText(rightSmallRect, ">")) value  += mult;
            if (value < min) value = min;
            if (value > max) value =  max;
            if (value != temp || readOnly)
            {
                buffer = value.ToString();
                temp = value;
            }
            Widgets.TextFieldNumeric(rect.ContractedBy(3f, 0f), ref temp, ref buffer, min, max);
            if (!readOnly) value = temp;
        }

        public static void DrawCountAdjuster(ref float value, Rect inRect, ref string buffer, float min, float max, float adjustment, bool inversedAdjustmentMultiplier = false, bool readOnly = false, float? setToMin = null, float? setToMax = null, float marginX = 50f, float marginY = 0f)
        {
            var temp        = value;
            var rect        = inRect.ContractedBy(marginX, marginY);
            var leftBigRect = rect.LeftPartPixels(30f);
            rect.xMin += 30f;
            var leftSmallRect = rect.LeftPartPixels(30f);
            rect.xMin += 30f;
            var rightBigRect = rect.RightPartPixels(30f);
            rect.xMax -= 30f;
            var rightSmallRect = rect.RightPartPixels(30f);
            rect.xMax -= 30f;
            if (adjustment != 0f)
            {
                if (inversedAdjustmentMultiplier)
                    adjustment /= GenUI.CurrentAdjustmentMultiplier();
                else
                    adjustment *= GenUI.CurrentAdjustmentMultiplier();
            }
            if (!readOnly && (setToMin.HasValue ? value > setToMin.Value : value != min) && Widgets.ButtonText(leftBigRect,    "<<")) value =  setToMin ?? min;
            if (!readOnly && value - adjustment >= min                                         && Widgets.ButtonText(leftSmallRect,  "<")) value  -= adjustment;
            if (!readOnly && (setToMax.HasValue ? value < setToMax.Value : value != max) && Widgets.ButtonText(rightBigRect,   ">>")) value =  setToMax ?? max;
            if (!readOnly && value + adjustment <= max                                         && Widgets.ButtonText(rightSmallRect, ">")) value  += adjustment;
            if (value < min) value = min;
            if (value > max) value =  max;
            if (value != temp || readOnly)
            {
                buffer = value.ToString();
                temp = value;
            }
            Widgets.TextFieldNumeric(rect.ContractedBy(3f, 0f), ref temp, ref buffer, min, max);
            if (!readOnly) value = temp;
        }

        public static IEnumerable<Rect> Divide(Rect rect, int items, int columns = 0, int rows = 0, bool drawLines = true)
        {
            if (columns == 0 && rows == 0)
            {
                if (!Mathf.Approximately(rect.width, rect.height)) throw new ArgumentException("Provided rect is not square!");
                var perSide = (int) Math.Ceiling(Math.Sqrt(items));
                rows    = perSide;
                columns = perSide;
            }

            if (rows         == 0) rows    = (int) Math.Ceiling((double) items / columns);
            else if (columns == 0) columns = (int) Math.Ceiling((double) items / rows);
            var curLoc = new Vector2(rect.xMin,            rect.yMin);
            var size   = new Vector2(rect.width / columns, rect.height / rows);
            var color  = Color.gray;
            for (var i = 0; i < columns; i++)
            {
                for (var j = 0; j < rows; j++)
                {
                    yield return new Rect(curLoc, size).ContractedBy(1f);
                    curLoc.y += size.y;
                    if (drawLines && i == 0 && j < rows - 1) Widgets.DrawLine(curLoc, new Vector2(rect.xMax, curLoc.y), color, 1f);
                }

                curLoc.x += size.x;
                curLoc.y =  rect.yMin;
                if (drawLines && i < columns - 1) Widgets.DrawLine(new Vector2(curLoc.x, curLoc.y + 2f), new Vector2(curLoc.x, rect.yMax), color, 1f);
            }
        }
    }
}