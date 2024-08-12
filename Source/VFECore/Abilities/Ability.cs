namespace VFECore.Abilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HarmonyLib;
    using JetBrains.Annotations;
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine;
    using Verse;
    using Verse.AI;
    using Verse.Sound;
    using UItils;
    using LudeonTK;

    public abstract class Ability : IExposable, ILoadReferenceable, ITargetingSource
    {
        public Pawn                 pawn;
        public Thing                holder;
        public Abilities.AbilityDef def;
        public int                  cooldown;

        public Abilities.Verb_CastAbility verb;

        public Hediff_Abilities hediff;

        public Hediff_Abilities Hediff => this.hediff == null && this.def.requiredHediff != null
                                              ? (this.hediff = (Hediff_Abilities)this.pawn?.health.hediffSet.GetFirstHediffOfDef(this.def.requiredHediff.hediffDef))
                                              : this.hediff;

        public List<AbilityExtension_AbilityMod> abilityModExtensions;

        public List<AbilityExtension_AbilityMod> AbilityModExtensions =>
            this.abilityModExtensions ?? (this.abilityModExtensions = this.def.modExtensions.Where(dme => dme is AbilityExtension_AbilityMod)
                                                                       .Cast<AbilityExtension_AbilityMod>().ToList());
        public Mote warmupMote;
        private Sustainer soundCast;

        private CompAbilities comp;
        public  CompAbilities Comp => this.comp ?? (this.comp = this.pawn.GetComp<CompAbilities>());

        public int                currentTargetingIndex = -1;
        public GlobalTargetInfo[] currentTargets        = Array.Empty<GlobalTargetInfo>();

        public virtual void Init()
        {
            if (this.verb == null)
                this.verb = (Abilities.Verb_CastAbility)Activator.CreateInstance(this.def.verbProperties.verbClass);
            this.verb.loadID      = this.GetUniqueLoadID() + "_Verb";
            this.verb.verbProps   = this.def.verbProperties;
            this.verb.verbTracker = this.pawn?.verbTracker;
            this.verb.caster      = this.pawn;
            this.verb.ability     = this;
            this.autoCast         = this.CanAutoCast && this.def.autocastPlayerDefault;

            this.currentTargetingIndex = -1;
            this.currentTargets        = new GlobalTargetInfo[this.def.targetCount];
        }

        public virtual bool ShowGizmoOnPawn() =>
            this.pawn != null && (this.pawn.IsColonistPlayerControlled && (this.def.showUndrafted || this.pawn.Drafted) ||
                                  this.pawn.IsCaravanMember() && this.pawn.IsColonist && !this.pawn.IsPrisoner &&
                                  !this.pawn.Downed) && this.AbilityModExtensions.All(x => x.ShowGizmoOnPawn(pawn));

        public virtual bool IsEnabledForPawn(out string reason)
        {
            if (this.cooldown > Find.TickManager.TicksGame)
            {
                reason = "VFEA.AbilityDisableReasonCooldown".Translate(this.def.LabelCap, (this.cooldown - Find.TickManager.TicksGame).ToStringTicksToPeriod());
                return false;
            }

            foreach (AbilityExtension_AbilityMod abilityMod in this.AbilityModExtensions)
            {
                if (!abilityMod.IsEnabledForPawn(this, out string s))
                {
                    reason = s;
                    return false;
                }
            }

            reason = "VFEA.AbilityDisableReasonGeneral".Translate(this.pawn?.NameShortColored ?? this.holder.LabelCap);

            return this.def.Satisfied(this.Hediff);
        }

        public virtual float CalculateStatFactorForPawn(float current, StatModifier statFactor) =>
            (statFactor.value >= 0)
                ? current * (this.pawn.GetStatValue(statFactor.stat) * statFactor.value)
                : current / (this.pawn.GetStatValue(statFactor.stat) * Math.Abs(statFactor.value));
        
        private static readonly HashSet<ToStringStyle> percentageBasedStyles = new HashSet<ToStringStyle>
        {
            ToStringStyle.PercentZero,
            ToStringStyle.PercentOne,
            ToStringStyle.PercentTwo
        };
        
        public virtual float CalculateStatOffsetForPawn(float current, StatModifier statOffset) =>
            percentageBasedStyles.Contains(statOffset.stat.toStringStyle)
                ? current + (this.pawn.GetStatValue(statOffset.stat) - statOffset.stat.defaultBaseValue) *
                statOffset.value
                : current + this.pawn.GetStatValue(statOffset.stat) * statOffset.value;
        
        public virtual float CalculateModifiedStatForPawn(float current, IEnumerable<StatModifier> statFactors, IEnumerable<StatModifier> statOffsets) =>
            statOffsets.Aggregate(0f, CalculateStatOffsetForPawn) +
            statFactors.Aggregate(current, CalculateStatFactorForPawn);
            

        public virtual float GetRangeForPawn() =>
            this.def.targetModes[this.currentTargetingIndex >= 0 ? this.currentTargetingIndex : 0] == AbilityTargetingMode.Self ?
                0f :
                Mathf.Min(CalculateModifiedStatForPawn(this.def.range, this.def.rangeStatFactors, this.def.rangeStatOffsets), def.maxRange);

        public virtual float GetRadiusForPawn() =>
            Mathf.Min(CalculateModifiedStatForPawn(this.def.radius, this.def.radiusStatFactors, this.def.radiusStatOffsets), def.maxRadius);

        public float GetAdditionalRadius() =>
            this.def.GetModExtension<AbilityExtension_AdditionalRadius>().GetRadiusFor(this.pawn);

        public virtual float GetPowerForPawn()
        {
            float power = CalculateModifiedStatForPawn(this.def.power, this.def.powerStatFactors, this.def.powerStatOffsets);
            var multiplier = this.def.GetModExtension<AbilityExtension_RandomPowerMultiplier>();
            return multiplier != null ? power * multiplier.range.RandomInRange : power;
        }

        public virtual int GetCastTimeForPawn() =>
            Mathf.RoundToInt(CalculateModifiedStatForPawn((float) this.def.castTime, this.def.castTimeStatFactors,
                this.def.castTimeStatOffsets));

        public virtual int GetCooldownForPawn() =>
            Mathf.RoundToInt(CalculateModifiedStatForPawn((float) this.def.cooldownTime,
                this.def.cooldownTimeStatFactors, this.def.cooldownTimeStatOffsets));

        public virtual int GetDurationForPawn() =>
            Mathf.RoundToInt(CalculateModifiedStatForPawn((float) this.def.durationTime,
                this.def.durationTimeStatFactors, this.def.durationTimeStatOffsets));

        private List<Pair<Effecter, TargetInfo>> maintainedEffecters = new List<Pair<Effecter, TargetInfo>>();

        public virtual string GetPowerForPawnDescription()
        {
            float power = CalculateModifiedStatForPawn(this.def.power, this.def.powerStatFactors, this.def.powerStatOffsets);
            if (power == 0)
                return "";

            var multiplier = this.def.GetModExtension<AbilityExtension_RandomPowerMultiplier>();
            if (multiplier == null)
                return $"{"VFEA.AbilityStatsPower".Translate()}: {power}".Colorize(Color.cyan);

            FloatRange range = multiplier.range;
            return $"{"VFEA.AbilityStatsPower".Translate()}: {power * range.min}-{power * range.max}".Colorize(Color.cyan);
        }

        public virtual string GetDescriptionForPawn()
        {
            var baseDesc = this.def.LabelCap.Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + this.def.description + "\n\n";
            StringBuilder sb = new StringBuilder(baseDesc);
            float rangeForPawn = this.GetRangeForPawn();
            if (rangeForPawn > 0f && rangeForPawn < 500f)
                sb.AppendLine($"{"Range".Translate()}: {rangeForPawn}".Colorize(Color.cyan));
            if (this.def.minRange > 0f)
                sb.AppendLine($"{"MinimumRange".Translate()}: {this.def.minRange}".Colorize(Color.cyan));
            float radiusForPawn = this.GetRadiusForPawn();
            if (radiusForPawn > 0f && radiusForPawn < 500f)
                sb.AppendLine($"{"radius".Translate().CapitalizeFirst()}: {radiusForPawn}".Colorize(Color.cyan));
            if (this.def.minRadius > 0f)
                sb.AppendLine($"{"VFEA.MinRadius".Translate()}: {this.def.minRadius}".Colorize(Color.cyan));
            string powerForPawnDescription = this.GetPowerForPawnDescription();
            if (!powerForPawnDescription.NullOrEmpty())
                sb.AppendLine(powerForPawnDescription);
            int castTimeForPawn = this.GetCastTimeForPawn();
            if (castTimeForPawn > 0)
                sb.AppendLine($"{"AbilityCastingTime".Translate()}: {castTimeForPawn.ToStringTicksToPeriodSpecific()}".Colorize(Color.cyan));
            int cooldownForPawn = this.GetCooldownForPawn();
            if (cooldownForPawn > 0)
                sb.AppendLine($"{"CooldownTime".Translate()}: {cooldownForPawn.ToStringTicksToPeriodSpecific()}".Colorize(Color.cyan));
            int durationForPawn = this.GetDurationForPawn();
            if (durationForPawn > 0)
                sb.AppendLine($"{"VFEA.AbilityStatsDuration".Translate()}: {durationForPawn.ToStringTicksToPeriodSpecific()}".Colorize(Color.cyan));
            else if (this.def.HasModExtension<AbilityExtension_Hediff>())
            {
                AbilityExtension_Hediff         extension            = this.def.GetModExtension<AbilityExtension_Hediff>();
                HediffCompProperties_Disappears propertiesDisappears = extension.hediff.CompProps<HediffCompProperties_Disappears>();
                if (propertiesDisappears != null)
                    sb.AppendLine($"{"VFEA.AbilityStatsDuration".Translate()}: {propertiesDisappears.disappearsAfterTicks.min.ToStringTicksToPeriodSpecific()} ~ {propertiesDisappears.disappearsAfterTicks.max.ToStringTicksToPeriodSpecific()}".Colorize(Color.cyan));
            }

            foreach (AbilityExtension_AbilityMod modExtension in this.AbilityModExtensions)
            {
                string description = modExtension.GetDescription(this);
                if (description.Length > 1)
                    sb.AppendLine(description);
            }

            if (this.CanAutoCast) sb.AppendLine((this.AutoCast ? "VFEA.RClickToNoAuto" : "VFEA.RClickToAuto").Translate());

            return sb.ToString().TrimEndNewlines();
        }

        public bool autoCast;

        public virtual bool AutoCast => this.pawn.IsColonistPlayerControlled ? this.autoCast : this.pawn.Spawned && this.CanAutoCast;

        public virtual bool CanAutoCast => this.def.targetCount == 1 && this.Chance > 0;

        public virtual float Chance => this.def.Chance;

        public virtual void Tick()
        {
            for (int num2 = maintainedEffecters.Count - 1; num2 >= 0; num2--)
            {
                Effecter first = maintainedEffecters[num2].First;
                if (first.ticksLeft > 0)
                {
                    TargetInfo second = maintainedEffecters[num2].Second;
                    first.EffectTick(second, second);
                    first.ticksLeft--;
                }
                else
                {
                    first.Cleanup();
                    maintainedEffecters.RemoveAt(num2);
                }
            }
        }

        public virtual Gizmo GetGizmo()
        {
            Command_Ability action = (Command_Ability) Activator.CreateInstance(this.def.gizmoClass, this.pawn, this);
            return action;
        }

        public virtual void GizmoUpdateOnMouseover()
        {
            float radius;
            switch (this.def.targetModes[0])
            {
                case AbilityTargetingMode.Self:
                    radius = this.GetRadiusForPawn();
                    break;
                default:
                    radius = this.GetRangeForPawn();
                    break;
            }

            if (GenRadial.MaxRadialPatternRadius > radius && radius >= 1)
                GenDraw.DrawRadiusRing(this.pawn.Position, radius, this.def.rangeRingColor);

            if (GenRadial.MaxRadialPatternRadius > this.def.minRange && this.def.minRange >= 1)
                GenDraw.DrawRadiusRing(this.pawn.Position, this.def.minRange, this.def.rangeRingColor);

            foreach (AbilityExtension_AbilityMod extension in this.AbilityModExtensions)
            {
                extension.GizmoUpdateOnMouseover(this);
            }
        }

        public virtual void WarmupToil(Toil toil)
        {
            toil.AddPreInitAction(delegate
            {
                def.warmupStartSound?.PlayOneShot(new TargetInfo(toil.actor.Position, toil.actor.Map));
            });
            toil.AddPreTickAction(delegate
            {
                if (def.warmupPreEndSound != null && this.verb.WarmupTicksLeft == def.warmupPreEndSoundTicks)
                {
                    def.warmupPreEndSound.PlayOneShot(new TargetInfo(toil.actor.Position, toil.actor.Map));
                }
                if (def.warmupMote != null)
                {
                    Vector3 vector = pawn.DrawPos;
                    vector += (verb.CurrentTarget.CenterVector3 - vector) * def.moteOffsetAmountTowardsTarget;
                    if (warmupMote == null || warmupMote.Destroyed)
                    {
                        warmupMote = MoteMaker.MakeStaticMote(vector, pawn.Map, def.warmupMote);
                    }
                    else
                    {
                        warmupMote.exactPosition = vector;
                        warmupMote.Maintain();
                    }
                }

                if (def.warmupSound != null)
                {
                    if (soundCast == null || soundCast.Ended)
                    {
                        soundCast = def.warmupSound.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(pawn.Position, pawn.Map), MaintenanceType.PerTick));
                    }
                    else
                    {
                        soundCast.Maintain();
                    }
                }
            });
            foreach (AbilityExtension_AbilityMod modExtension in this.AbilityModExtensions)
                modExtension.WarmupToil(toil);
        }

        public virtual void DoAction()
        {
            if (Event.current.button == 1)
            {
               if (this.CanAutoCast) this.autoCast = !this.autoCast;
               else this.autoCast = false;
            }
            else
            {
                this.currentTargetingIndex = -1;
                this.currentTargets        = new GlobalTargetInfo[this.def.targetCount];

                this.DoTargeting();
            }
        }

        public virtual void DoTargeting()
        {
            this.currentTargetingIndex++;

            if (this.currentTargetingIndex >= this.def.targetCount)
            {
                if (this.currentTargets.Length > 1 || (this.currentTargets.Any() && this.currentTargets.First().Map != this.Caster.Map) ||
                    this.pawn.IsCaravanMember() || this.currentTargets.Any(gti => gti.HasWorldObject))
                    this.CreateCastJob(this.currentTargets);
                else
                    this.CreateCastJob(this.currentTargets.Any() ? this.currentTargets[0].Thing != null ?
                        new LocalTargetInfo(this.currentTargets[0].Thing) : new LocalTargetInfo(this.currentTargets[0].Cell) : default);
                return;
            }

            AbilityTargetingMode targetMode = this.def.targetModes[this.currentTargetingIndex];

            if (targetMode == AbilityTargetingMode.Self)
            {
                this.currentTargets[this.currentTargetingIndex] = this.pawn;
                this.DoTargeting();
            }
            else if (targetMode == AbilityTargetingMode.Random)
            {
                var cell = this.currentTargets.Length > this.currentTargetingIndex
                    ? this.currentTargets[this.currentTargetingIndex - 1].Cell
                    : pawn.Position;
                if (this.GetTargetsAround(cell, this.targetParams, true)
                        .TryRandomElement(out var target))
                    this.currentTargets[this.currentTargetingIndex] = target;
                this.DoTargeting();
            }
            else if (this.def.worldTargeting)
            {
                var initialTarget = CameraJumper.GetWorldTarget(this.pawn);
                CameraJumper.TryJump(initialTarget);
                Find.WorldTargeter.BeginTargeting(gti =>
                                                  {
                                                      if (!this.ValidateTargetTile(gti, true)) return false;
                                                      Map map = Find.WorldObjects.MapParentAt(gti.Tile)?.Map;
                                                      if (targetMode == AbilityTargetingMode.Tile || map is null)
                                                      {
                                                          this.currentTargets[this.currentTargetingIndex] = gti;
                                                          this.DoTargeting();
                                                          return true;
                                                      }
                                                      else
                                                      {
                                                          this.currentTargets[this.currentTargetingIndex] = new GlobalTargetInfo(map.AllCells.First(), map);
                                                          CameraJumper.TryJump(map.Center, map);
                                                          Find.Targeter.BeginTargeting(this.targetParams, this.OrderForceTarget, this.DrawHighlight,
                                                              lti => this.ValidateTarget(lti), mouseAttachment:this.MouseAttachment(this.currentTargets[this.currentTargetingIndex]));
                                                          return true;
                                                      }
                                                  }, targetMode == AbilityTargetingMode.Tile, this.MouseAttachment(initialTarget),
                    targetMode == AbilityTargetingMode.Tile, this.OnUpdateWorld, this.WorldTargetingLabel, this.CanHitTargetTile);
            }
            else
            {
                Find.Targeter.BeginTargeting(this);
            }
        }

        public virtual bool AICanUseOn(Thing target)
        {
            if (def.isPositive.HasValue)
            {
                if (target.HostileTo(pawn))
                {
                    if (def.isPositive.Value)
                    {
                        return false;
                    }
                }
                else if (def.isPositive.Value is false)
                {
                    return false;
                }
            }
            return true;
        }
        protected virtual string WorldTargetingLabel(GlobalTargetInfo target)
        {
            return null;
        }

        // Careful with changing this, hook in mp compat.
        public virtual void CreateCastJob(LocalTargetInfo target) =>
            this.CreateCastJob(target.ToGlobalTargetInfo(this.Caster.Map));

        public virtual void CreateCastJob(params GlobalTargetInfo[] targets)
        {
            foreach (AbilityExtension_AbilityMod modExtension in this.AbilityModExtensions)
                if (!modExtension.Valid(targets, this, true))
                {
                    this.currentTargetingIndex--;
                    return;
                }

            this.currentTargetingIndex = -1;

            bool startAbilityJobImmediately = true;
            this.PreCast(targets, ref startAbilityJobImmediately, () => this.StartAbilityJob(targets));

            if (startAbilityJobImmediately)
                this.StartAbilityJob(targets);

            this.currentTargets        = new GlobalTargetInfo[this.def.targetCount];
        }

        [Obsolete("Use new method with GlobalTargetInfo instead")]
        public void StartAbilityJob(LocalTargetInfo target) =>
            this.StartAbilityJob(target.ToGlobalTargetInfo(this.Caster.Map));

        public void StartAbilityJob(params GlobalTargetInfo[] targets)
        {
            this.pawn.jobs?.EndCurrentJob(JobCondition.InterruptForced, false);
            Job           job  = JobMaker.MakeJob(this.def.jobDef ?? VFE_DefOf_Abilities.VFEA_UseAbility, targets.Any() && targets[0].IsMapTarget ?
                (LocalTargetInfo) targets[0] : default);
            CompAbilities comp    = this.pawn.GetComp<CompAbilities>();
            comp.currentlyCasting = this;
            ModifyTargets(ref targets);
            comp.currentlyCastingTargets = targets;
            if (this.pawn.IsCaravanMember()) this.Cast(targets);
            else this.pawn.jobs.StartJob(job, JobCondition.InterruptForced, keepCarryingThingOverride: def.keepCarryingThing);
        }

        protected bool currentAoETargeting;
        public virtual void ModifyTargets(ref GlobalTargetInfo[] targets)
        {
            if (this.def.hasAoE)
            {
                currentAoETargeting = true;
                var targetsTmp = GetTargetsAround(targets[0].Cell, this.def.targetingParametersForAoE);
                if (this.def.targetCount == 2 && this.def.targetModes[1] == AbilityTargetingMode.Random)
                {
                    targetsTmp = targetsTmp.SelectMany(target => new[]
                    {
                        target,
                        GetTargetsAround(target.Cell, this.def.targetingParametersList[1], true).RandomElement()
                    });
                }

                targets = targetsTmp.ToArray();
                currentAoETargeting = false;
            }
        }

        protected IEnumerable<GlobalTargetInfo> GetTargetsAround(IntVec3 cell, TargetingParameters parms, bool isRandom = false)
        {
            var minRadius = this.def.minRadius;
            var maxRadius = this.GetRadiusForPawn();
            if (isRandom)
            {
                var ext = this.def.GetModExtension<AbilityExtension_RandomRadius>();
                if (ext != null)
                {
                    minRadius = ext.minRadius;
                    maxRadius = ext.maxRadius;
                }
            }
            if (parms.canTargetLocations)
            {
                foreach (var c in GenRadial.RadialCellsAround(cell, minRadius, maxRadius))
                {
                    if (c.InBounds(pawn.Map))
                    {
                        if (parms is TargetingParametersForAoE aoe && !aoe.CanTarget(new TargetInfo(c, pawn.Map), this)) continue;
                        yield return new GlobalTargetInfo(c, pawn.Map);
                    }
                }
            }
            else
            {
                foreach (var thing in GenRadial.RadialDistinctThingsAround(cell, pawn.Map, maxRadius, true))
                {
                    var aoeTargetParms = parms as TargetingParametersForAoE;
                    if ((aoeTargetParms?.CanTarget(thing, this) ?? parms.CanTarget(thing)) &&
                        (aoeTargetParms is {ignoreRangeAndSight: true} 
                            || this.ValidateTarget(thing, false) && thing.OccupiedRect().ClosestDistSquaredTo(cell) > minRadius))
                    {
                        if (!parms.canTargetSelf && thing == pawn) continue;
                        yield return thing;
                    }
                }
            }
        }

        public virtual void PreCast(GlobalTargetInfo[] target, ref bool startAbilityJobImmediately, Action startJobAction)
        {
            foreach (AbilityExtension_AbilityMod modExtension in this.AbilityModExtensions)
                modExtension.PreCast(target, this, ref startAbilityJobImmediately, startJobAction);
        }

        [Obsolete("Refer to casting targets in comp instead")]
        public virtual void PreWarmupAction(LocalTargetInfo target) =>
            this.PreWarmupAction();

        public virtual void PreWarmupAction()
        {
            foreach (AbilityExtension_AbilityMod modExtension in this.AbilityModExtensions)
                modExtension.PreWarmupAction(this.pawn.GetComp<CompAbilities>().currentlyCastingTargets, this);
        }

        [Obsolete("Use the new Cast method using GlobalTargets instead")]
        public virtual void Cast(LocalTargetInfo target) =>
            this.Cast(target.ToGlobalTargetInfo(this.Caster.Map));

        public virtual void Cast(params GlobalTargetInfo[] targets)
        {
            this.cooldown = Find.TickManager.TicksGame + this.GetCooldownForPawn();

            if (this.def.goodwillImpact != 0 && targets.Any())
            {
                foreach (var target in targets)
                {
                    if (target.Thing is Pawn pawnTarget)
                    {
                        ApplyGoodwillImpact(pawnTarget);
                    }
                }
            }

            foreach (AbilityExtension_AbilityMod modExtension in this.AbilityModExtensions)
            {
                if (targets.Length > 1 || (targets.Any() && targets.First().Map != this.Caster.Map))
                    modExtension.Cast(targets, this);
                else
                    modExtension.Cast(targets.Any() ? targets[0].Thing != null ? new LocalTargetInfo(targets[0].Thing) : new LocalTargetInfo(targets[0].Cell) : default, this);
            }

            // Obsolete methods used below during transition


            bool cast;
            bool targetMote;
            bool hediffApply;
            if (targets.Length > 1 || (targets.Any() && targets.First().Map != this.Caster.Map))
                this.CheckCastEffects(targets, out cast, out targetMote, out hediffApply);
            else
                this.CheckCastEffects(targets.Any() ? targets[0].Thing != null ? new LocalTargetInfo(targets[0].Thing) : new LocalTargetInfo(targets[0].Cell) : default, out cast, out targetMote, out hediffApply);

            if (hediffApply)
                if (targets.Length > 1 || (targets.Any() && targets.First().Map != this.Caster.Map))
                    this.ApplyHediffs(targets);
                else
                    this.ApplyHediffs(targets.Any() ? targets[0].Thing != null ? new LocalTargetInfo(targets[0].Thing) : new LocalTargetInfo(targets[0].Cell) : default);

            if (cast)
                if (targets.Length > 1 || (targets.Any() && targets.First().Map != this.Caster.Map))
                    this.CastEffects(targets);
                else
                    this.CastEffects(targets.Any() ? targets[0].Thing != null ? new LocalTargetInfo(targets[0].Thing) : new LocalTargetInfo(targets[0].Cell) : default);

            if (targetMote)
                if (targets.Length > 1 || (targets.Any() && targets.First().Map != this.Caster.Map))
                    this.TargetEffects(targets);
                else
                    this.TargetEffects(targets.Any() ? targets[0].Thing != null ? new LocalTargetInfo(targets[0].Thing) : new LocalTargetInfo(targets[0].Cell) : default);
            
            PostCast(targets);
        }

        public virtual void PostCast(params GlobalTargetInfo[] targets)
        {
            foreach (AbilityExtension_AbilityMod modExtension in this.AbilityModExtensions)
            {
                modExtension.PostCast(targets, this);
            }
        }

        public void ApplyGoodwillImpact(Pawn pawnTarget)
        {
            if (!pawnTarget.IsSlaveOfColony)
            {
                Faction homeFaction = pawnTarget.HomeFaction;
                if (pawn.Faction == Faction.OfPlayer && homeFaction != null && !homeFaction.HostileTo(pawn.Faction)
                && (this.def.applyGoodwillImpactToLodgers || !pawnTarget.IsQuestLodger()) && !pawnTarget.IsQuestHelper())
                {
                    Faction.OfPlayer.TryAffectGoodwillWith(homeFaction, this.def.goodwillImpact, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.UsedHarmfulAbility);
                }
            }
        }

        public virtual void EndCastJob()
        {

        }

        [Obsolete("Use the new method that uses GlobalTargetInfo instead")]
        public virtual void CastEffects(LocalTargetInfo targetInfo) =>
            this.CastEffects(targetInfo.ToGlobalTargetInfo(this.Caster.MapHeld));

        public virtual void CastEffects(params GlobalTargetInfo[] targetInfos)
        {
            if (this.def.castFleck != null)
                MakeStaticFleck(this.pawn.DrawPos, this.pawn.MapHeld, this.def.castFleck, this.def.castFleckScaleWithRadius ?
                    this.GetRadiusForPawn() : this.def.castFleckScale, this.def.castFleckSpeed);

            if (this.def.fleckOnTarget != null && targetInfos.Any())
            {
                var vec = this.def.hasAoE ? firstTarget.CenterVector3 :
                    targetInfos[0].Thing != null ? targetInfos[0].Thing.DrawPos : targetInfos[0].Cell.ToVector3();
                var map = targetInfos[0].Thing != null ? targetInfos[0].Map : this.pawn.MapHeld;
                MakeStaticFleck(vec, map, this.def.fleckOnTarget, this.def.fleckOnTargetScaleWithRadius 
                    ? this.GetRadiusForPawn() : this.def.fleckOnTargetScale, this.def.fleckOnTargetSpeed);
            }

            if (this.def.casterHediff != null)
                this.pawn.health.AddHediff(this.def.casterHediff);
            this.def.castSound?.PlayOneShot(new TargetInfo(this.pawn.Position, this.pawn.MapHeld));
        }

        public static void MakeStaticFleck(IntVec3 cell, Map map, FleckDef fleckDef, float scale, float speed)
        {
            MakeStaticFleck(cell.ToVector3Shifted(), map, fleckDef, scale, speed);
        }

        public static void MakeStaticFleck(Vector3 loc, Map map, FleckDef fleckDef, float scale, float speed)
        {
            var data = FleckMaker.GetDataStatic(loc, map, fleckDef, scale);
            data.velocitySpeed = speed;
            map.flecks.CreateFleck(data);
        }

        public void AddEffecterToMaintain(Effecter eff, IntVec3 pos, int ticks, Map map = null)
        {
            eff.ticksLeft = ticks;
            this.maintainedEffecters.Add(new Pair<Effecter, TargetInfo>(eff, new TargetInfo(pos, map ?? pawn.Map)));
        }

        public void AddEffecterToMaintain(Effecter eff, TargetInfo target, int ticks)
        {
            eff.ticksLeft = ticks;
            this.maintainedEffecters.Add(new Pair<Effecter, TargetInfo>(eff, target));
        }

        [Obsolete("Use new Method using GlobalTargetInfo instead")]
        public virtual void TargetEffects(LocalTargetInfo targetInfo) =>
            this.TargetEffects(targetInfo.ToGlobalTargetInfo(this.Caster.Map));

        public virtual void TargetEffects(params GlobalTargetInfo[] targetInfo)
        {
            if (targetInfo.Any())
            {
                if (!this.def.targetFlecks.NullOrEmpty())
                    foreach (FleckDef fleck in this.def.targetFlecks)
                        FleckMaker.Static(targetInfo[0].Cell, this.pawn.Map, fleck);

                if ((targetInfo[0].Thing as Pawn)?.health.hediffSet.hediffs != null)
                    foreach (Hediff targetHediff in ((Pawn)targetInfo[0].Thing).health.hediffSet.hediffs)
                    {
                        if (targetHediff is HediffWithComps hediffWithComps)
                            foreach (HediffComp hediffComp in hediffWithComps.comps)
                            {
                                if (hediffComp is HediffComp_AbilityTargetReact compReact)
                                    compReact.ReactTo(this);
                            }
                    }
            }
        }


        [Obsolete("Use new method that uses GlobalTargetInfo instead")]
        public virtual void ApplyHediffs(LocalTargetInfo targetInfo) =>
            this.ApplyHediffs(targetInfo.ToGlobalTargetInfo(this.Caster.Map));

        public virtual void ApplyHediffs(params GlobalTargetInfo[] targetInfo)
        {
            AbilityExtension_Hediff hediffExtension = this.def.GetModExtension<AbilityExtension_Hediff>();
            if (hediffExtension?.applyAuto ?? false)
            {
                if (hediffExtension.applyToCaster)
                {
                    ApplyHediff(this.pawn, hediffExtension);
                }
                else
                {
                    foreach (GlobalTargetInfo target in targetInfo)
                    {
                        if (target.Thing is Pawn targetPawn)
                        {
                            ApplyHediff(targetPawn, hediffExtension);
                        }
                    }
                }
            }
        }

        public Hediff ApplyHediff(Pawn targetPawn)
        {
            AbilityExtension_Hediff hediffExtension = this.def.GetModExtension<AbilityExtension_Hediff>();
            return ApplyHediff(targetPawn, hediffExtension);
        }

        public Hediff ApplyHediff(Pawn targetPawn, AbilityExtension_Hediff hediffExtension)
        {
            BodyPartRecord bodyPart = hediffExtension.bodyPartToApply != null
                     ? targetPawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == hediffExtension.bodyPartToApply)
                     : null;
            var duration = this.GetDurationForPawn();
            if (hediffExtension.durationMultiplier != null)
            {
                duration = (int)(duration * (hediffExtension.durationMultiplierFromCaster ? 
                                                 pawn.GetStatValue(hediffExtension.durationMultiplier) : 
                                                 targetPawn.GetStatValue(hediffExtension.durationMultiplier)));
            }
            return ApplyHediff(targetPawn, hediffExtension.hediff, bodyPart, duration, hediffExtension.severity);
        }

        public virtual Hediff ApplyHediff(Pawn targetPawn, HediffDef hediffDef, BodyPartRecord bodyPart, int duration, float severity)
        {
            Hediff localHediff = HediffMaker.MakeHediff(hediffDef, targetPawn, bodyPart);
            if (localHediff is Hediff_Ability hediffAbility)
                hediffAbility.ability = this;
            if (severity > float.Epsilon)
                localHediff.Severity = severity;
            if (localHediff is HediffWithComps hwc)
                foreach (HediffComp hediffComp in hwc.comps)
                {
                    if (hediffComp is HediffComp_Ability hca)
                        hca.ability = this;
                    if (duration > 0 && hediffComp is HediffComp_Disappears hcd)
                        hcd.ticksToDisappear = duration;
                }
            targetPawn.health.AddHediff(localHediff);
            return targetPawn.health.hediffSet.GetFirstHediffOfDef(hediffDef); // accounts for merged hediffs in this case
        }

        [Obsolete("Use new method that uses GlobalTargetInfos")]
        public virtual void CheckCastEffects(LocalTargetInfo targetInfo, out bool cast, out bool target, out bool hediffApply) =>
            this.CheckCastEffects(new[] { targetInfo.ToGlobalTargetInfo(this.Caster.Map) }, out cast, out target, out hediffApply);

        public virtual void CheckCastEffects(GlobalTargetInfo[] targetsInfos, out bool cast, out bool target, out bool hediffApply) =>
            cast = target = hediffApply = true;

        public virtual void ExposeData()
        {
            Scribe_References.Look(ref this.pawn, nameof(this.pawn), saveDestroyedThings: true);
            Scribe_Values.Look(ref this.cooldown, nameof(this.cooldown));
            Scribe_Defs.Look(ref this.def, nameof(this.def));
            Scribe_Deep.Look(ref this.verb, nameof(this.verb));
            Scribe_Values.Look(ref this.autoCast, nameof(this.autoCast));
            Scribe_TargetInfo.Look(ref this.firstTarget, nameof(this.firstTarget));

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (this.verb == null) // no idea how the hell that happens
                    this.verb = (Abilities.Verb_CastAbility)Activator.CreateInstance(this.def.verbProperties.verbClass);

                this.verb.loadID      = this.GetUniqueLoadID() + "_Verb";
                this.verb.verbProps   = this.def.verbProperties;
                this.verb.verbTracker = this.pawn?.verbTracker;
                this.verb.caster      = this.pawn;
                this.verb.ability     = this;

                this.currentTargetingIndex = -1;
                this.currentTargets        = new GlobalTargetInfo[this.def.targetCount];
            }
        }

        public string GetUniqueLoadID() =>
            $"Ability_{this.def.defName}_{this.holder.GetUniqueLoadID()}";

        public virtual bool CanHitTarget(LocalTargetInfo target) =>
            this.CanHitTarget(target, this.def.requireLineOfSight);

        public virtual bool CanHitTarget(LocalTargetInfo target, bool sightCheck)
        {
            foreach (AbilityExtension_AbilityMod modExtension in this.AbilityModExtensions)
                if (!modExtension.CanApplyOn(target, this))
                    return false;

            if (currentAoETargeting)
            {
                return true;
            }
            if (this.def.worldTargeting)
            {
                return true;
            }
            var distance = target.Cell.DistanceTo(this.pawn.Position);
            if (target.IsValid && distance < this.GetRangeForPawn() && distance > this.def.minRange)
            {
                if ((this.targetParams.canTargetLocations && this.targetParams.CanTarget(new TargetInfo(target.Cell, this.Caster.Map))) ||
                    this.targetParams.CanTarget(target.ToTargetInfo(this.Caster.Map)))
                {
                    if (!sightCheck)
                        return true;

                    if (GenSight.LineOfSight(this.pawn.Position, target.Cell, this.pawn.Map))
                        return true;
                    List<IntVec3> tempSourceList = new List<IntVec3>();
                    ShootLeanUtility.LeanShootingSourcesFromTo(this.pawn.Position, target.Cell, this.pawn.Map, tempSourceList);
                    if (tempSourceList.Any(ivc => GenSight.LineOfSight(ivc, target.Cell, this.pawn.Map)))
                        return true;
                }
            }

            return false;
        }

        public virtual bool ValidateTarget(LocalTargetInfo target, bool showMessages = true) =>
            this.CanHitTarget(target) && AbilityModExtensions.All(x => x.ValidateTarget(target, this, showMessages));

        public virtual void DrawHighlight(LocalTargetInfo target)
        {
            float range = this.GetRangeForPawn();
            if (!this.def.worldTargeting && GenRadial.MaxRadialPatternRadius > range && range >= 1)
                GenDraw.DrawRadiusRing(this.pawn.Position, range, this.def.rangeRingColor);

            if (target.IsValid)
            {
                GenDraw.DrawTargetHighlight(target);

                float radius = this.GetRadiusForPawn();
                if (GenRadial.MaxRadialPatternRadius > radius && radius >= 1)
                    GenDraw.DrawRadiusRing(target.Cell, radius, this.def.radiusRingColor);


                if (GenRadial.MaxRadialPatternRadius > this.def.minRadius && this.def.minRadius >= 1)
                    GenDraw.DrawRadiusRing(target.Cell, this.def.minRadius, this.def.radiusRingColor);
            }
        }

        public LocalTargetInfo firstTarget;
        public virtual void OrderForceTarget(LocalTargetInfo target)
        {
            firstTarget = target;
            if (target.Thing != null)
                this.currentTargets[this.currentTargetingIndex] = target.Thing;
            else if (this.currentTargets[this.currentTargetingIndex].Map != null)
                this.currentTargets[this.currentTargetingIndex] = new GlobalTargetInfo(target.Cell, this.currentTargets[this.currentTargetingIndex].Map);
            else
                this.currentTargets[this.currentTargetingIndex] = new GlobalTargetInfo(target.Cell, this.Caster.Map);

            this.DoTargeting();
        }

        public virtual void OnGUI(LocalTargetInfo target)
        {
            GenUI.DrawMouseAttachment(MouseAttachment(target.ToGlobalTargetInfo(pawn.Map)));
            foreach (var abilityModExtension in AbilityModExtensions)
            {
                abilityModExtension.TargetingOnGUI(target, this);
            }
            DrawAttachmentExtraLabel(target);
        }

        public virtual string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            return null;
        }
        protected void DrawAttachmentExtraLabel(LocalTargetInfo target)
        {
            string text = ExtraLabelMouseAttachment(target);
            if (!text.NullOrEmpty())
            {
                Widgets.MouseAttachedLabel(text);
                return;
            }
            foreach (var abilityModExtension in AbilityModExtensions)
            {
                text = abilityModExtension.ExtraLabelMouseAttachment(target, this);
                if (!text.NullOrEmpty())
                {
                    Widgets.MouseAttachedLabel(text);
                    break;
                }
            }
        }

        protected virtual Texture2D MouseAttachment(GlobalTargetInfo target)
        {
            return (!target.IsValid) ? TexCommand.CannotShoot : ((!(this.UIIcon != BaseContent.BadTex)) ? TexCommand.Attack : this.UIIcon);
        }

        public virtual bool ValidateTargetTile(GlobalTargetInfo target, bool showMessages = false)
        {
            return CanHitTargetTile(target);
        }

        public virtual bool CanHitTargetTile(GlobalTargetInfo target)
        {
            foreach (AbilityExtension_AbilityMod modExtension in this.AbilityModExtensions)
                if (!modExtension.ValidTile(target, this))
                    return false;

            var distance = Find.World.grid.ApproxDistanceInTiles(target.Tile, Tile);
            return target.IsValid && distance < this.GetRangeForPawn() && distance > this.def.minRange;
        }

        protected int Tile => this.pawn.GetCaravan()?.Tile ?? this.pawn.Map?.Tile ??
            Find.Maps.FirstOrDefault(m => m.IsPlayerHome)?.Tile ??
            Find.Maps.FirstOrDefault()?.Tile ?? TileFinder.RandomStartingTile();

        public virtual void OnUpdateWorld()
        {
            float range = this.GetRangeForPawn();
            if (range >= 1)
                GenDraw.DrawWorldRadiusRing(Tile, Mathf.RoundToInt(range));
        }

        public bool      CasterIsPawn     => this.CasterPawn                                                                        != null;
        public bool      IsMeleeAttack    => this.GetRangeForPawn()                                                                 < 6;
        public bool      Targetable       => this.def.targetModes[this.currentTargetingIndex >= 0 ? this.currentTargetingIndex : 0] != AbilityTargetingMode.Self;
        public bool      MultiSelect      { get; }
        public virtual bool HidePawnTooltips 
        { 
            get
            {
                foreach (var abilityExtension in AbilityModExtensions)
                {
                    if (abilityExtension.HidePawnTooltips)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public Thing     Caster           => this.pawn ?? this.holder;
        public Pawn      CasterPawn       => this.pawn;
        public Verb      GetVerb          => this.verb;
        public Texture2D UIIcon           => this.def.icon;

        public virtual TargetingParameters targetParams
        {
            get
            {
                TargetingParameters parameters = this.def.targetingParametersList[this.currentTargetingIndex >= 0 ? this.currentTargetingIndex : 0];

                if (this.def.targetModes[this.currentTargetingIndex >= 0 ? this.currentTargetingIndex : 0] == AbilityTargetingMode.Self)
                    parameters.targetSpecificThing = this.pawn;

                return parameters;
            }
        }

        public ITargetingSource DestinationSelector { get; }


        [DebugAction("Pawns", "Give ability...", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        [UsedImplicitly]
        private static void GiveAbility()
        {
            List<DebugMenuOption> list = new List<DebugMenuOption>();
            foreach (Abilities.AbilityDef def in DefDatabase<Abilities.AbilityDef>.AllDefs)
            {
                Abilities.AbilityDef abilityDef = def;

                list.Add(new
                             DebugMenuOption($"{(abilityDef.requiredHediff != null ? $"{abilityDef.requiredHediff.hediffDef.LabelCap} ({abilityDef.requiredHediff.minimumLevel}): " : string.Empty)}{abilityDef.LabelCap}",
                                             DebugMenuOptionMode.Tool, () =>
                                                                       {
                                                                           foreach (Pawn item in (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
                                                                                                  where t is Pawn
                                                                                                  select t).Cast<Pawn>())
                                                                           {
                                                                               CompAbilities abilityComp = item.TryGetComp<CompAbilities>();
                                                                               if (abilityComp != null)
                                                                               {
                                                                                   abilityComp.GiveAbility(abilityDef);
                                                                                   DebugActionsUtility.DustPuffFrom(item);
                                                                               }
                                                                           }
                                                                       }));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
        }
    }

    public class Ability_Blank : Ability
    {
    }
}
