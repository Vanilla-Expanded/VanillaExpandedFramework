using System.Collections.Generic;
using MVCF.Commands;
using MVCF.Comps;
using MVCF.Features;
using MVCF.Utilities;
using MVCF.VerbComps;
using UnityEngine;
using Verse;

namespace MVCF
{
    public class ManagedVerb : IExposable, ILoadReferenceable
    {
        public enum ToggleType
        {
            Separate,
            Integrated,
            None
        }

        public List<VerbComp> AllComps = new();

        private bool enabledInt = true;

        private string loadId;
        public AdditionalVerbProps Props;
        public VerbSource Source = VerbSource.None;
        public Verb Verb;

        public VerbManager Manager { get; set; }

        public virtual bool Enabled
        {
            get => enabledInt;
            set => enabledInt = value;
        }

        public virtual bool NeedsTicking => false;

        public virtual bool Independent => false;

        public void ExposeData()
        {
            Scribe_Values.Look(ref enabledInt, "enabled");
            Scribe_Values.Look(ref loadId, "loadId");
            foreach (var comp in AllComps) comp.PostExposeData();
        }

        public string GetUniqueLoadID() => loadId;

        public virtual bool SetTarget(LocalTargetInfo target) => true;

        public virtual void Notify_Spawned()
        {
        }

        public virtual void Notify_Despawned()
        {
        }

        public virtual void Initialize(Verb verb, AdditionalVerbProps props, IEnumerable<VerbCompProperties> additionalComps)
        {
            Verb = verb;
            Props = props;
            loadId = verb.loadID + "_Managed";
            this.Register();
            if (Props is {draw: true} && !Base.GetFeature<Feature_Drawing>().Enabled)
                Log.Error("[MVCF] Found a verb marked to draw while that feature is not enabled.");

            if (Props is {canFireIndependently: true} && !Base.GetFeature<Feature_IndependentVerbs>().Enabled)
                Log.Error("[MVCF] Found a verb marked to fire independently while that feature is not enabled.");

            if (Props is {separateToggle: false} && !Base.GetFeature<Feature_IntegratedToggle>().Enabled)
                Log.Error("[MVCF] Found a verb marked for an integrated toggle while that feature is not enabled.");
        }

        public virtual void Notify_Added(VerbManager man, VerbSource source)
        {
            Manager = man;
            Source = source;
        }

        public virtual void Notify_Removed()
        {
            Manager = null;
            Source = VerbSource.None;
        }

        public virtual bool Available() => true;

        public virtual void Notify_ProjectileFired()
        {
        }

        public virtual IEnumerable<CommandPart> GetCommandParts(Command_VerbTargetExtended command)
        {
            yield break;
        }

        public virtual void ModifyProjectile(ref ThingDef projectile)
        {
        }

        public bool GetToggleStatus() => enabledInt;

        public void Toggle()
        {
            enabledInt = !enabledInt;
            Manager?.RecalcSearchVerb();
        }

        public virtual void DrawOn(Pawn p, Vector3 drawPos)
        {
        }

        public virtual void Tick()
        {
        }

        public virtual IEnumerable<Gizmo> GetGizmos(Thing ownerThing)
        {
            yield return GetTargetCommand(ownerThing);

            if (GetToggleType() == ToggleType.Separate)
                yield return GetToggleCommand(ownerThing);
        }

        protected virtual Command_ToggleVerbUsage GetToggleCommand(Thing ownerThing) => new(this);

        protected virtual Command_VerbTargetExtended GetTargetCommand(Thing ownerThing) => new(this, ownerThing);

        public virtual ToggleType GetToggleType()
        {
            if (Props == null) return Verb.CasterIsPawn && Verb.CasterPawn.RaceProps.Animal ? ToggleType.Separate : ToggleType.None;

            if (!Props.canBeToggled) return ToggleType.None;
            if (Props.separateToggle) return ToggleType.Separate;
            if (Base.GetFeature<Feature_IntegratedToggle>().Enabled) return ToggleType.Integrated;

            Log.ErrorOnce(
                "[MVCF] " + (Verb.EquipmentSource.LabelShortCap ?? "Hediff verb of " + Verb.caster) +
                " wants an integrated toggle but that feature is not enabled. Using seperate toggle.",
                Verb.GetHashCode());
            return ToggleType.Separate;
        }

        public virtual float GetScore(Pawn p, LocalTargetInfo target, bool debug = false)
        {
            if (debug) Log.Message("Getting score of " + Verb + " with target " + target);
            if (Verb is IVerbScore verbScore) return verbScore.GetScore(p, target);
            var accuracy = 0f;
            if (p.Map != null)
                accuracy = ShotReport.HitReportFor(p, Verb, target).TotalEstimatedHitChance;
            else if (Verb.TryFindShootLineFromTo(p.Position, target, out var line))
                accuracy = Verb.verbProps.GetHitChanceFactor(Verb.EquipmentSource, line.Source.DistanceTo(line.Dest));

            var damage = accuracy * Verb.verbProps.burstShotCount * Verb.GetDamage();
            var timeSpent = Verb.verbProps.AdjustedCooldownTicks(Verb, p) + Verb.verbProps.warmupTime.SecondsToTicks();
            if (debug)
            {
                Log.Message("Accuracy: " + accuracy);
                Log.Message("Damage: " + damage);
                Log.Message("timeSpent: " + timeSpent);
                Log.Message("Score of " + Verb + " on target " + target + " is " + damage / timeSpent);
            }

            return damage / timeSpent;
        }

        public virtual bool PreCastShot() => true;
    }
}