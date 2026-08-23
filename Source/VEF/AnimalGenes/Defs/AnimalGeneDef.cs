using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace VEF.AnimalGenes
{
    public class AnimalGeneDef : Def
    {
        [NoTranslate]
        public string iconPath;

        private Color? iconColor;

        [Unsaved(false)]
        private Texture2D cachedIcon;

        [Unsaved(false)]
        private string cachedDescription;

        public int stability;

        public List<StatModifier> statOffsets = new List<StatModifier>();

        public List<StatModifier> statFactors = new List<StatModifier>();

        public float marketValueFactor = 1f;

        public float animalIllnessMTB = 45;

        public float animalTantrumMTB = 0;

        public HediffDef hediffToAdd = null;

        public AbilityDef abilityToAdd = null;

        public TrainabilityDef trainabilityDef = null;

        public AnimalGeneFamilyTagDef familyTag;

        public SimpleCurve litterSizeCurveOverride;

        public float stillbirthChance = 0f;

        public List<ThingDefCountClass> extraButcherProducts;

        public bool scaleButcherProductsByMeatAmount = true;

        public bool isSpecialized = false;

        public bool singleRankGene = false;

        public bool dontGenerateInGeneTweakTools = false;

        public string extraDescriptions = "";

        public string DescriptionFull => cachedDescription ?? (cachedDescription = GetDescriptionFull());

        public int GeneLevel => 3 - stability;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string item in base.ConfigErrors())
            {
                yield return item;
            }
            if (stability < -2 || stability > 2)
            {
                yield return "stability of an AnimalGeneDef needs to be between -2 and 2.";
            }

        }

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
                        cachedIcon = ContentFinder<Texture2D>.Get(iconPath) ?? BaseContent.BadTex;
                    }
                }
                return cachedIcon;
            }
        }

        public Color IconColor
        {
            get
            {
                if (iconColor.HasValue)
                {
                    return iconColor.Value;
                }

                return Color.white;
            }
        }

        public string GetDescriptionFull()
        {
            StringBuilder sb = new StringBuilder();
            if (!description.NullOrEmpty())
            {
                sb.Append(description).AppendLine().AppendLine();
            }

            sb.AppendLineTagged("VRE_Stability".Translate().Colorize(ColoredText.TipSectionTitleColor) + ": " + stability.ToStringWithSign());
            sb.AppendLine();


            bool effectsTitleWritten = false;
            if (!statFactors.NullOrEmpty())
            {
                for (int i = 0; i < statFactors.Count; i++)
                {
                    StatModifier statModifier = statFactors[i];
                    if (statModifier.stat.CanShowWithLoadedMods())
                    {
                        AppendEffectLine(statModifier.stat.LabelCap + " " + statModifier.ToStringAsFactor);
                    }
                }
            }

            if (!statOffsets.NullOrEmpty())
            {
                for (int l = 0; l < statOffsets.Count; l++)
                {
                    StatModifier statModifier3 = statOffsets[l];
                    if (statModifier3.stat.CanShowWithLoadedMods())
                    {
                        AppendEffectLine(statModifier3.stat.LabelCap + " " + statModifier3.ValueToStringAsOffset);
                    }
                }
            }

            if (animalIllnessMTB != 45)
            {
                sb.AppendLine();
                sb.AppendLineTagged("VRE_AnimalIllness".Translate() + ": " + animalIllnessMTB + " " + "VRE_Days".Translate());
            }
            if (animalTantrumMTB != 0)
            {
                sb.AppendLine();
                sb.AppendLineTagged("VRE_AnimalTantrum".Translate() + ": " + animalTantrumMTB + " " + "VRE_Days".Translate());
            }
            if (trainabilityDef != null)
            {
                sb.AppendLine();
                sb.AppendLineTagged("VRE_Trainability".Translate() + ": " + trainabilityDef.ToString());
            }
            if (isSpecialized)
            {
                sb.AppendLine();
                sb.AppendLineTagged("VRE_SpecializedGene".Translate().Colorize(ColoredText.TipSectionTitleColor) + ": " + "VRE_SpecializedGeneDesc".Translate());
            }
            if (extraDescriptions != "")
            {
                sb.AppendLine();
                sb.AppendLineTagged(extraDescriptions);
            }
            sb.AppendLine();
            sb.AppendLineTagged("VRE_FamilyTag".Translate().Colorize(ColoredText.TipSectionTitleColor) + ": " + familyTag.LabelCap);

            return sb.ToString().TrimEndNewlines();
            void AppendEffectLine(string text)
            {
                if (!effectsTitleWritten)
                {
                    sb.AppendLineTagged(("Effects".Translate().CapitalizeFirst() + ":").Colorize(ColoredText.TipSectionTitleColor));
                    effectsTitleWritten = true;
                }
                sb.AppendLine("  - " + text);
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            foreach (StatDrawEntry item in base.SpecialDisplayStats(req))
            {
                yield return item;
            }

            yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, "VRE_Stability".Translate(), stability.ToString(), "VRE_StabilityDesc".Translate(), 4080);


            if (statOffsets != null)
            {
                for (int k = 0; k < statOffsets.Count; k++)
                {
                    StatModifier statModifier = statOffsets[k];
                    if (statModifier.stat.CanShowWithLoadedMods())
                    {
                        yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, statModifier.stat.LabelCap, statModifier.ValueToStringAsOffset, statModifier.stat.description, 4070);
                    }
                }
            }
            if (statFactors != null)
            {
                for (int k = 0; k < statFactors.Count; k++)
                {
                    StatModifier statModifier2 = statFactors[k];
                    if (statModifier2.stat.CanShowWithLoadedMods())
                    {
                        yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, statModifier2.stat.LabelCap, statModifier2.ToStringAsFactor, statModifier2.stat.description, 4070);
                    }
                }
            }

        }

    }
}
