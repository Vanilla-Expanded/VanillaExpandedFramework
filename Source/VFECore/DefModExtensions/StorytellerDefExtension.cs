using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFECore
{

    public class StorytellerDefExtension : DefModExtension
    {

        private static readonly StorytellerDefExtension DefaultValues = new StorytellerDefExtension();
        public static StorytellerDefExtension Get(Def def) => def.GetModExtension<StorytellerDefExtension>() ?? DefaultValues;

        public TechLevelRange allowedTechLevels = TechLevelRange.All;

    }

}
