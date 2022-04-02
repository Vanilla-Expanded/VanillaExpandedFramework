namespace VFECore.Abilities
{
    using System;
    using System.Collections.Generic;
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class AbilityDef : Def
    {
        public Type abilityClass;

        public HediffWithLevelCombination requiredHediff;
        public TraitDef                   requiredTrait;

        public AbilityTargetingMode targetMode = AbilityTargetingMode.Self;

        public float              range            = 0f;
        public List<StatModifier> rangeStatFactors = new List<StatModifier>();

        public float              radius            = 0f;
        public List<StatModifier> radiusStatFactors = new List<StatModifier>();

        public float              power            = 0f;
        public List<StatModifier> powerStatFactors = new List<StatModifier>();

        public int                castTime            = 0;
        public List<StatModifier> castTimeStatFactors = new List<StatModifier>();

        public int                cooldownTime            = 0;
        public List<StatModifier> cooldownTimeStatFactors = new List<StatModifier>();

        public int                durationTime            = 0;
        public List<StatModifier> durationTimeStatFactors = new List<StatModifier>();

        [Unsaved(false)] public Texture2D icon = BaseContent.BadTex;
        public                  string    iconPath;

        public SoundDef  castSound;
        public FleckDef  castFleck;
        public HediffDef casterHediff;

        public List<FleckDef> targetFlecks;

        public VerbProperties      verbProperties;
        public TargetingParameters targetingParameters;
        public float               chance                = 1f;
        public bool                autocastPlayerDefault = false;

        public string jobReportString = "Using ability: {0}";

        public string JobReportString => this.jobReportString.Formatted(this.LabelCap);

        public float Chance =>
            this.chance;

        public bool Satisfied(Hediff_Abilities hediff) =>
            ((hediff != null && hediff.SatisfiesConditionForAbility(this)) || this.requiredHediff == null) &&
            (this.requiredTrait                                                                   == null || (hediff?.pawn?.story?.traits.HasTrait(this.requiredTrait) ?? false));

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string configError in base.ConfigErrors())
                yield return configError;

            if (!typeof(Abilities.Ability).IsAssignableFrom(this.abilityClass))
                yield return $"{this.abilityClass} is not a valid ability type";
            /*
            if (this.GetModExtension<AbilityExtension_Projectile>() != null && (this.GetModExtension<AbilityExtension_Hediff>()?.applyAuto ?? false))
                yield return "Projectile and auto apply hediff present. Please check if that is intended.";
            */
        }

        public override void PostLoad()
        {
            if (!this.iconPath.NullOrEmpty())
                LongEventHandler.ExecuteWhenFinished(delegate { this.icon = ContentFinder<Texture2D>.Get(this.iconPath); });

            if (this.targetingParameters == null)
            {
                this.targetingParameters = new TargetingParameters()
                                           {
                                               canTargetPawns     = false,
                                               canTargetBuildings = false,
                                               canTargetAnimals   = false,
                                               canTargetHumans    = false,
                                               canTargetMechs     = false
                                           };

                switch (this.targetMode)
                {
                    case AbilityTargetingMode.Self:
                        this.targetingParameters = TargetingParameters.ForSelf(null);
                        break;
                    case AbilityTargetingMode.Location:
                        this.targetingParameters.canTargetLocations = true;
                        break;
                    case AbilityTargetingMode.Thing:
                        this.targetingParameters.canTargetItems     = true;
                        this.targetingParameters.canTargetBuildings = true;
                        break;
                    case AbilityTargetingMode.Pawn:
                        this.targetingParameters.canTargetPawns = this.targetingParameters.canTargetHumans = this.targetingParameters.canTargetMechs = this.targetingParameters.canTargetAnimals = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            if (this.modExtensions == null)
                this.modExtensions = new List<DefModExtension>();

            if (this.verbProperties == null)
                this.verbProperties = new VerbProperties
                                      {
                                          verbClass             = typeof(Verb_CastAbility),
                                          label                 = this.label,
                                          category              = VerbCategory.Misc,
                                          range                 = this.range,
                                          noiseRadius           = 3f,
                                          targetParams          = this.targetingParameters,
                                          warmupTime            = this.castTime / (float)GenTicks.TicksPerRealSecond,
                                          defaultCooldownTime   = this.cooldownTime,
                                          meleeDamageBaseAmount = Mathf.RoundToInt(this.power),
                                          meleeDamageDef        = DamageDefOf.Blunt
                                      };

            if (this.modExtensions != null)
                foreach (DefModExtension extension in this.modExtensions)
                {
                    if (extension is AbilityExtension_AbilityMod abilityExtension)
                        abilityExtension.abilityDef = this;
                }
        }
    }


    public class HediffWithLevelCombination
    {
        public HediffDef hediffDef;
        public int       minimumLevel;
    }

    public enum AbilityTargetingMode : byte
    {
        Self,
        Location,
        Thing,
        Pawn
    }
}