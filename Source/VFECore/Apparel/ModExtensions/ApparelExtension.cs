using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaApparelExpanded
{
    public class ApparelExtension : DefModExtension
    {
        public float? skillGainModifier;
        public List<WorkTags> workDisables;
        public List<SkillDef> skillDisables;
    }
}