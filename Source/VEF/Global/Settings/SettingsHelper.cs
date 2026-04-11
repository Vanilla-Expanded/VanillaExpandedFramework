using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VEF
{
    [StaticConstructorOnStartup]
    public static class SettingsHelper
    {
        public static bool Settings_Button(this Listing_Standard ls, string label, Rect rect)
        {
            bool result = Widgets.ButtonText(rect, label, true, true, true);
            ls.Gap((2f));
            return result;
        }

        public static Rect LabelPlusButton(this Listing_Standard ls, string label, string tooltip = null)
        {
            float num = Text.CalcHeight(label, ls.ColumnWidth);
            Rect rect = ls.GetRect(num);
            Widgets.Label(rect, label);
            if (tooltip != null)
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }
            ls.Gap(50);
            return rect;
        }
    }
}
