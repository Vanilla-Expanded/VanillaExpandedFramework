using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFECore.Abilities
{
    public class AbilityDef : Def
    {
        public Type abilityClass;
        public bool needsTicking;

        public HediffWithLevelCombination requiredHediff;
        public TraitDef                   requiredTrait;

        public AbilityTargetingMode targetMode = AbilityTargetingMode.None;

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

        public int goodwillImpact = 0;
        public bool applyGoodwillImpactToLodgers = true;

        public bool requireLineOfSight = true;
        public JobDef jobDef;
        public float distanceToTarget = 1.5f;

        public ThingDef warmupMote;
        public SoundDef warmupSound;
        public SoundDef warmupStartSound;
        public SoundDef warmupPreEndSound;
        public int warmupPreEndSoundTicks;
        public float moteOffsetAmountTowardsTarget;
        public bool drawAimPie = true;


        [Unsaved(false)] public Texture2D icon = BaseContent.BadTex;
        public                  string    iconPath;

        public SoundDef  castSound;
        public FleckDef castFleck;
        public float castFleckScale = 1f;
        public float castFleckSpeed;
        public FleckDef fleckOnTarget;
        public float fleckOnTargetScale = 1f;
        public float fleckOnTargetSpeed;

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

            if (!typeof(Ability).IsAssignableFrom(this.abilityClass))
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

            if (targetingParameters == null)
            {
                targetingParameters = new TargetingParameters
                {
                    canTargetPawns     = false,
                    canTargetBuildings = false,
                    canTargetAnimals   = false,
                    canTargetHumans    = false,
                    canTargetMechs     = false
                };

                if (targetMode == AbilityTargetingMode.None) targetMode = AbilityTargetingMode.Self;

                switch (targetMode)
                {
                    case AbilityTargetingMode.Self:
                        targetingParameters = TargetingParameters.ForSelf(null);
                        break;
                    case AbilityTargetingMode.Location:
                        targetingParameters.canTargetLocations = true;
                        break;
                    case AbilityTargetingMode.Thing:
                        targetingParameters.canTargetItems     = true;
                        targetingParameters.canTargetBuildings = true;
                        break;
                    case AbilityTargetingMode.Pawn:
                        targetingParameters.canTargetPawns = targetingParameters.canTargetHumans =
                            targetingParameters.canTargetMechs = targetingParameters.canTargetAnimals = true;
                        break;
                    case AbilityTargetingMode.Humanlike:
                        targetingParameters.canTargetPawns = targetingParameters.canTargetHumans = true;
                        break;
                    case AbilityTargetingMode.None:
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
        None,
        Self,
        Location,
        Thing,
        Pawn,
        Humanlike
    }
}