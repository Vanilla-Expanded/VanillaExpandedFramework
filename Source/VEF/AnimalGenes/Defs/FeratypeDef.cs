using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VEF.AnimalGenes;
using Verse;

namespace VEF.AnimalGenes
{
    public class FeratypeDef : Def
    {

        [NoTranslate]
        public string iconPath;
        [Unsaved(false)]
        private Texture2D cachedIcon;

        public PawnKindDef race;

        public FeratypeFamilyDef feratypeFamily;
        public bool canBeAlpha;
        public List<AnimalGeneDef> animalGenes = new List<AnimalGeneDef>();

        public static readonly Color IconColor = new Color(0.75f, 0.75f, 0.75f);

        public Texture2D Icon
        {
            get
            {
                if (cachedIcon == null)
                {
                    if (iconPath.NullOrEmpty())
                    {
                        cachedIcon = BaseContent.BadTex;
                    }
                    else
                    {
                        cachedIcon = ContentFinder<Texture2D>.Get(iconPath);
                    }
                }
                return cachedIcon;
            }
        }
        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string item in base.ConfigErrors())
            {
                yield return item;
            }
            if (iconPath.NullOrEmpty())
            {
                yield return "iconPath is empty.";
            }

        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            if (animalGenes.NullOrEmpty())
            {
                return;
            }
            if (descriptionHyperlinks == null)
            {
                descriptionHyperlinks = new List<DefHyperlink>();
            }
            foreach (AnimalGeneDef gene in animalGenes)
            {
                descriptionHyperlinks.Add(new DefHyperlink(gene));
            }
            List<FeratypeDef> allFeraTypes = DefDatabase<FeratypeDef>.AllDefsListForReading.Where(x => x.feratypeFamily == feratypeFamily && x != this).ToList();
            foreach (FeratypeDef feratype in allFeraTypes)
            {
                descriptionHyperlinks.Add(new DefHyperlink(feratype));
            }
        }

        public static string FeratypeDescWithExtra(FeratypeDef feratype)
        {
            return feratype.description + "\n\n" + "VRE_MoreInfoInInfoScreen".Translate().Colorize(ColoredText.SubtleGrayColor);
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            List<FeratypeDef> allFeraTypes = DefDatabase<FeratypeDef>.AllDefsListForReading.Where(x => x.feratypeFamily == feratypeFamily).ToList();

            foreach (StatDrawEntry item in base.SpecialDisplayStats(req))
            {
                yield return item;
            }
            yield return new StatDrawEntry(StatCategoryDefOf.Basics, "VRE_AnimalGenes".Translate(), animalGenes.Select((AnimalGeneDef x) => x.label).ToCommaList().CapitalizeFirst(), "VRE_AnimalGenesFeratypeDesc".Translate() + "\n\n" + animalGenes.Select((AnimalGeneDef x) => x.label).ToLineList("  - ", capitalizeItems: true), 1000);
            yield return new StatDrawEntry(StatCategoryDefOf.Basics, "VRE_FeratypeFamily".Translate(), feratypeFamily.LabelCap, "VRE_FeratypeFamilyDesc".Translate(feratypeFamily.label, feratypeFamily.description), 999);
            yield return new StatDrawEntry(StatCategoryDefOf.Basics, "VRE_AllowedPartners".Translate(), allFeraTypes.Select(x => x.label).ToCommaList().CapitalizeFirst(), "VRE_AllowedPartnersDesc".Translate(feratypeFamily.label) + "\n\n" + allFeraTypes.Select(x => x.label).ToLineList("  - ", capitalizeItems: true), 998);

        }
    }
}
