﻿using RimWorld;
using Verse;

namespace VEF.Utils
{
    public static class FormatUtils
    {
        public static string ToStringTicksToPeriodSpecific(this int ticks)
        {
            return ticks >= 2500 ? ticks.ToStringTicksToPeriod() : (ticks.TicksToSeconds().ToStringDecimalIfSmall() + "LetterSecond".Translate());
        }
    }
}
