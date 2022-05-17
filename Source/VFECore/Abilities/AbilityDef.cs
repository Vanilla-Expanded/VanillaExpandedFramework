using System;
using System.Collections.Generic;
using HarmonyLib;
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

        public int                        targetCount = 1;

        private AbilityTargetingMode       targetMode  = AbilityTargetingMode.None;
        public  List<AbilityTargetingMode> targetModes = new List<AbilityTargetingMode>();
        private TargetingParameters        targetingParameters;
        public TargetingParameters targetingParametersForAoE;
        public List<TargetingParameters>  targetingParametersList = new List<TargetingParameters>();

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


        public bool   worldTargeting;
        public bool hasAoE;
        public bool   requireLineOfSight = true;
        public JobDef jobDef;
        public float  distanceToTarget = 1.5f;

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

        public VerbProperties verbProperties;
        public float          chance                = 1f;
        public bool           autocastPlayerDefault = false;

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
            else
            {
                if (!this.needsTicking && AccessTools.DeclaredMethod(this.abilityClass, "Tick") != null)
                {
                    yield return $"{this.defName} has a Tick method but doesn't have the needsTicking field. It will not work.";
                }
            }
            /*
            if (this.GetModExtension<AbilityExtension_Projectile>() != null && (this.GetModExtension<AbilityExtension_Hediff>()?.applyAuto ?? false))
                yield return "Projectile and auto apply hediff present. Please check if that is intended.";
            */
        }

        public override void PostLoad()
        {
            if (!this.iconPath.NullOrEmpty())
                LongEventHandler.ExecuteWhenFinished(delegate { this.icon = ContentFinder<Texture2D>.Get(this.iconPath); });

            if (this.targetingParameters != null)
                if (this.targetingParametersList.Any())
                    this.targetingParametersList.Insert(0, this.targetingParameters);
                else
                    this.targetingParametersList.Add(this.targetingParameters);

            if (this.targetMode != AbilityTargetingMode.None)
                if (this.targetModes.Any())
                    this.targetModes.Insert(0, this.targetMode);
                else
                    this.targetModes.Add(this.targetMode);

            for (int i = 0; i < this.targetCount; i++)
            {
                AbilityTargetingMode targetingMode = this.targetModes.Count > i ? this.targetModes[i] : AbilityTargetingMode.Self;

                TargetingParameters parameters = this.targetingParametersList.Count > i ?
                                                     this.targetingParametersList[i] :
                                                     new TargetingParameters
                                                     {
                                                         canTargetPawns     = false,
                                                         canTargetBuildings = false,
                                                         canTargetAnimals   = false,
                                                         canTargetHumans    = false,
                                                         canTargetMechs     = false
                                                     };
                if (targetingMode == AbilityTargetingMode.None)
                    targetingMode = AbilityTargetingMode.Self;

                switch (targetingMode)
                {
                    case AbilityTargetingMode.Self:
                        parameters = TargetingParameters.ForSelf(null);
                        break;
                    case AbilityTargetingMode.Location:
                        parameters.canTargetLocations = true;
                        break;
                    case AbilityTargetingMode.Thing:
                        parameters.canTargetItems     = true;
                        parameters.canTargetBuildings = true;
                        break;
                    case AbilityTargetingMode.Pawn:
                        parameters.canTargetPawns = parameters.canTargetHumans = parameters.canTargetMechs = parameters.canTargetAnimals = true;
                        break;
                    case AbilityTargetingMode.Humanlike:
                        parameters.canTargetPawns = parameters.canTargetHumans = true;
                        break;
                    case AbilityTargetingMode.Tile:
                        break;
                    case AbilityTargetingMode.None:
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (i < this.targetModes.Count)
                    this.targetModes[i] = targetingMode;
                else
                    this.targetModes.Add(targetingMode);

                if (i < this.targetingParametersList.Count)
                    this.targetingParametersList[i] = parameters;
                else
                    this.targetingParametersList.Add(parameters);
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
        Humanlike,
        Tile
    }
}