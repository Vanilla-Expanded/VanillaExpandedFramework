using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VEF.Storyteller
{

    public class IncidentDefExtension : DefModExtension
    {

        private static readonly IncidentDefExtension DefaultValues = new IncidentDefExtension();
        public static IncidentDefExtension Get(Def def) => def.GetModExtension<IncidentDefExtension>() ?? DefaultValues;

        public FactionDef forcedFaction;
        public IntRange forcedPointsRange = IntRange.Zero;
        public RaidStrategyDef forcedStrategy;

    }

}
