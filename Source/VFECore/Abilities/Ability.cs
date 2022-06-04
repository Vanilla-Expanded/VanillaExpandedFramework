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

        public void Init()
        {
            if (this.verb == null)
                this.verb = (Abilities.Verb_CastAbility)Activator.CreateInstance(this.def.verbProperties.verbClass);
            this.verb.loadID      = this.GetUniqueLoadID() + "_Verb";
            this.verb.verbProps   = this.def.verbProperties;
            this.verb.verbTracker = this.pawn?.verbTracker;
            this.verb.caster      = this.pawn;
            this.verb.ability     = this;
            this.autoCast         = this.def.autocastPlayerDefault;

            this.currentTargetingIndex = -1;
            this.currentTargets        = new GlobalTargetInfo[this.def.targetCount];
        }

        public virtual bool ShowGizmoOnPawn() =>
            this.pawn != null && (this.pawn.IsColonistPlayerControlled && this.pawn.Drafted ||
                                  this.pawn.IsCaravanMember() && this.pawn.IsColonist && !this.pawn.IsPrisoner &&
                                  !this.pawn.Downed);

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

        public virtual float GetRangeForPawn() =>
            this.def.targetModes[this.currentTargetingIndex >= 0 ? this.currentTargetingIndex : 0] == AbilityTargetingMode.Self ?
                0f :
                this.def.rangeStatFactors.Aggregate(this.def.range, (current, statFactor) => current * (this.pawn.GetStatValue(statFactor.stat) * statFactor.value));

        public virtual float GetRadiusForPawn() =>
            this.def.radiusStatFactors.Aggregate(this.def.radius, (current, statFactor) => current * (this.pawn.GetStatValue(statFactor.stat) * statFactor.value));

        public float GetAdditionalRadius() =>
            this.def.GetModExtension<AbilityExtension_AdditionalRadius>().GetRadiusFor(this.pawn);

        public virtual float GetPowerForPawn() =>
            this.def.powerStatFactors.Aggregate(this.def.power, (current, statFactor) => current * (this.pawn.GetStatValue(statFactor.stat) * statFactor.value));

        public virtual int GetCastTimeForPawn() =>
            Mathf.RoundToInt(this.def.castTimeStatFactors.Aggregate((float)this.def.castTime, (current, statFactor) => current * (this.pawn.GetStatValue(statFactor.stat) * statFactor.value)));

        public virtual int GetCooldownForPawn() =>
            Mathf.RoundToInt(this.def.cooldownTimeStatFactors.Aggregate((float)this.def.cooldownTime,
                                                                        (current, statFactor) => current * (this.pawn.GetStatValue(statFactor.stat) * statFactor.value)));

        public virtual int GetDurationForPawn() =>
            Mathf.RoundToInt(this.def.durationTimeStatFactors.Aggregate((float)this.def.durationTime,
                                                                        (current, statFactor) => current * (this.pawn.GetStatValue(statFactor.stat) * statFactor.value)));

        private List<Pair<Effecter, TargetInfo>> maintainedEffecters = new List<Pair<Effecter, TargetInfo>>();

        public virtual string GetDescriptionForPawn()
        {
            StringBuilder sb = new StringBuilder(this.def.description);

            sb.AppendLine();

            float rangeForPawn = this.GetRangeForPawn();
            if (rangeForPawn > 0f)
                sb.AppendLine($"{"Range".Translate()}: {rangeForPawn}".Colorize(Color.cyan));
            if (this.def.minRange > 0f)
                sb.AppendLine($"{"MinimumRange".Translate()}: {this.def.minRange}".Colorize(Color.cyan));
            float radiusForPawn = this.GetRadiusForPawn();
            if (radiusForPawn > 0f)
                sb.AppendLine($"{"radius".Translate()}: {radiusForPawn}".Colorize(Color.cyan));
            if (this.def.minRadius > 0f)
                sb.AppendLine($"{"VFEA.MinRadius".Translate()}: {this.def.minRadius}".Colorize(Color.cyan));
            float powerForPawn = this.GetPowerForPawn();
            if (powerForPawn > 0f)
                sb.AppendLine($"{"VFEA.AbilityStatsPower".Translate()}: {powerForPawn}".Colorize(Color.cyan));
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

            return sb.ToString();
        }

        public bool autoCast;

        public virtual bool AutoCast => this.pawn.IsColonistPlayerControlled ? this.autoCast : this.pawn.Spawned;

        public virtual bool CanAutoCast => this.Chance > 0;

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
                GenDraw.DrawRadiusRing(this.pawn.Position, radius, Color.cyan);

            if (GenRadial.MaxRadialPatternRadius > this.def.minRange && this.def.minRange >= 1)
                GenDraw.DrawRadiusRing(this.pawn.Position, this.def.minRange, Color.cyan);

            foreach (AbilityExtension_AbilityMod extension in this.AbilityModExtensions)
                extension.GizmoUpdateOnMouseover(this);
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
                CameraJumper.TryJump(CameraJumper.GetWorldTarget(this.pawn));
                Find.WorldTargeter.BeginTargeting(gti =>
                                                  {
                                                      if (targetMode == AbilityTargetingMode.Tile)
                                                      {
                                                          this.currentTargets[this.currentTargetingIndex] = gti;
                                                          this.DoTargeting();
                                                          return true;
                                                      }
                                                      else
                                                      {
                                                          Map map = Find.WorldObjects.MapParentAt(gti.Tile).Map;
                                                          this.currentTargets[this.currentTargetingIndex] = new GlobalTargetInfo(map.AllCells.First(), map);
                                                          CameraJumper.TryJump(map.Center.ToIntVec2.ToIntVec3, map);
                                                          Find.Targeter.BeginTargeting(this.targetParams, this.OrderForceTarget, this.DrawHighlight, lti => this.ValidateTarget(lti));
                                                          return true;
                                                      }
                                                  }, targetMode == AbilityTargetingMode.Tile, closeWorldTabWhenFinished: targetMode == AbilityTargetingMode.Tile, canSelectTarget: this.ValidateTargetTile, onUpdate: this.OnUpdateWorld);
            }
            else
            {
                Find.Targeter.BeginTargeting(this);
            }
        }

        // Careful with changing this, hook in mp compat.
        public virtual void CreateCastJob(LocalTargetInfo target) =>
            this.CreateCastJob(target.ToGlobalTargetInfo(this.Caster.Map));

        public virtual void CreateCastJob(params GlobalTargetInfo[] targets)
        {
            foreach (AbilityExtension_AbilityMod modExtension in this.AbilityModExtensions)
                if (!modExtension.Valid(targets, this, true))
                    return;

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
            else this.pawn.jobs.StartJob(job, JobCondition.InterruptForced);
        }
        protected virtual void ModifyTargets(ref GlobalTargetInfo[] targets)
        {
            if (this.def.hasAoE)
            {
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
                    if ((parms is TargetingParametersForAoE aoe ? aoe.CanTarget(thing, this) : parms.CanTarget(thing))            &&
                        (parms is TargetingParametersForAoE aoe2 && aoe2.ignoreRangeAndSight 
                        || AbilityModExtensions.All(x => x.ValidateTarget(thing, this, false)) &&
                            thing.OccupiedRect().ClosestDistSquaredTo(cell) > minRadius))
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

            if (this.def.goodwillImpact != 0 && targets.Any() && targets[0].Thing is Pawn pawnTarget)
            {
                Pawn pawn = this.pawn;
                if (!pawnTarget.IsSlaveOfColony)
                {
                    Faction homeFaction = pawnTarget.HomeFaction;
                    if (pawn.Faction == Faction.OfPlayer                                      && homeFaction != null && !homeFaction.HostileTo(pawn.Faction)
                    && (this.def.applyGoodwillImpactToLodgers || !pawnTarget.IsQuestLodger()) && !pawnTarget.IsQuestHelper())
                    {
                        Faction.OfPlayer.TryAffectGoodwillWith(homeFaction, this.def.goodwillImpact, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.UsedHarmfulAbility);
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
        }

        public virtual void EndCastJob()
        {

        }

        [Obsolete("Use the new method that uses GlobalTargetInfo instead")]
        public virtual void CastEffects(LocalTargetInfo targetInfo) =>
            this.CastEffects(targetInfo.ToGlobalTargetInfo(this.Caster.Map));

        public virtual void CastEffects(params GlobalTargetInfo[] targetInfos)
        {
            if (this.def.castFleck != null)
                MakeStaticFleck(this.pawn.DrawPos, this.pawn.Map, this.def.castFleck, this.def.castFleckScale, this.def.castFleckSpeed);

            if (this.def.fleckOnTarget != null && targetInfos.Any())
                MakeStaticFleck(targetInfos[0].Thing.DrawPos, targetInfos[0].Map, this.def.fleckOnTarget, this.def.fleckOnTargetScale, this.def.fleckOnTargetSpeed);

            if (this.def.casterHediff != null)
                this.pawn.health.AddHediff(this.def.casterHediff);
            this.def.castSound?.PlayOneShot(new TargetInfo(this.pawn.Position, this.pawn.Map));
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
            foreach (GlobalTargetInfo target in targetInfo)
            {
                if (target.Thing is Pawn targetPawn)
                {
                    ApplyHediff(targetPawn);
                }
            }
        }
        public virtual Hediff ApplyHediff(Pawn targetPawn)
        {
            AbilityExtension_Hediff hediffExtension = this.def.GetModExtension<AbilityExtension_Hediff>();
            if (hediffExtension?.applyAuto ?? false)
            {
                BodyPartRecord bodyPart = hediffExtension.bodyPartToApply != null
                                              ? targetPawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == hediffExtension.bodyPartToApply)
                                              : null;
                Hediff localHediff = HediffMaker.MakeHediff(hediffExtension.hediff, targetPawn, bodyPart);
                if (localHediff is Hediff_Ability hediffAbility)
                    hediffAbility.ability = this;

                var duration = this.GetDurationForPawn();
                if (hediffExtension.durationMultiplier != null)
                {
                    duration = (int)(duration * (hediffExtension.durationMultiplierFromCaster 
                        ? pawn.GetStatValue(hediffExtension.durationMultiplier) 
                        :  targetPawn.GetStatValue(hediffExtension.durationMultiplier)));
                }
                if (hediffExtension.severity > float.Epsilon)
                    localHediff.Severity = hediffExtension.severity;
                if (localHediff is HediffWithComps hwc)
                    foreach (HediffComp hediffComp in hwc.comps)
                    {
                        if (hediffComp is HediffComp_Ability hca)
                            hca.ability = this;
                        if (hediffComp is HediffComp_Disappears hcd)
                            hcd.ticksToDisappear = duration;
                    }
                targetPawn.health.AddHediff(localHediff);
                return pawn.health.hediffSet.GetFirstHediffOfDef(hediffExtension.hediff); // accounts for merged hediffs in this case
            }
            return null;
        }

        [Obsolete("Use new method that uses GlobalTargetInfos")]
        public virtual void CheckCastEffects(LocalTargetInfo targetInfo, out bool cast, out bool target, out bool hediffApply) =>
            this.CheckCastEffects(new[] { targetInfo.ToGlobalTargetInfo(this.Caster.Map) }, out cast, out target, out hediffApply);

        public virtual void CheckCastEffects(GlobalTargetInfo[] targetsInfos, out bool cast, out bool target, out bool hediffApply) =>
            cast = target = hediffApply = true;

        public virtual void ExposeData()
        {
            Scribe_References.Look(ref this.pawn, nameof(this.pawn));
            Scribe_Values.Look(ref this.cooldown, nameof(this.cooldown));
            Scribe_Defs.Look(ref this.def, nameof(this.def));
            Scribe_Deep.Look(ref this.verb, nameof(this.verb));
            Scribe_Values.Look(ref this.autoCast, nameof(this.autoCast));

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

            var distance = target.Cell.DistanceTo(this.pawn.Position);
            if (target.IsValid && (this.def.worldTargeting || (distance < this.GetRangeForPawn() && distance > this.def.minRange)))
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
                GenDraw.DrawRadiusRing(this.pawn.Position, range, Color.cyan);

            if (target.IsValid)
            {
                GenDraw.DrawTargetHighlight(target);

                float radius = this.GetRadiusForPawn();
                if (GenRadial.MaxRadialPatternRadius > radius && radius >= 1)
                    GenDraw.DrawRadiusRing(target.Cell, radius, Color.red);


                if (GenRadial.MaxRadialPatternRadius > this.def.minRadius && this.def.minRadius >= 1)
                    GenDraw.DrawRadiusRing(target.Cell, this.def.minRadius, Color.red);
            }
        }

        public virtual void OrderForceTarget(LocalTargetInfo target)
        {
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
            Texture2D icon = (!target.IsValid) ? TexCommand.CannotShoot : ((!(this.UIIcon != BaseContent.BadTex)) ? TexCommand.Attack : this.UIIcon);
            GenUI.DrawMouseAttachment(icon);
        }

        public virtual bool ValidateTargetTile(GlobalTargetInfo target)
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
        public bool      HidePawnTooltips { get; }
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