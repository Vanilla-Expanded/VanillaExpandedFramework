using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFECore
{

    public class ResearchProjectDefExtension : DefModExtension
    {

        private static readonly ResearchProjectDefExtension DefaultValues = new ResearchProjectDefExtension();
        public static ResearchProjectDefExtension Get(Def def) => def.GetModExtension<ResearchProjectDefExtension>() ?? DefaultValues;

        public List<ResearchProjectTagDef> greylistedTags;

    }

}
