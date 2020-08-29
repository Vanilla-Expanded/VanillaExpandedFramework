using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFECore
{

    public class CompProperties_Shield : CompProperties
    {

        public CompProperties_Shield()
        {
            compClass = typeof(CompShield);
        }

        public List<string> shieldTags;
        public bool useDeflectMetalEffect;
        public List<BodyPartGroupDef> coveredBodyPartGroups;
        public GraphicData offHandGraphicData;
        public HoldOffsetSet offHandHoldOffset;

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            if (coveredBodyPartGroups.NullOrEmpty())
                yield return "coveredBodyPartGroups is not defined or is empty.";
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            string valueString = coveredBodyPartGroups.Select(p => p.label).ToCommaList(true).CapitalizeFirst();
            yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "VanillaFactionsExpanded.Protects".Translate(), valueString, String.Empty, 100);
        }

    }

}
