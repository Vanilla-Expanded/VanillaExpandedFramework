using System.Collections.Generic;
using MVCF.Comps;
using MVCF.Features;
using RimWorld;
using UnityEngine;
using Verse;

namespace MVCF
{
    public class ManagedVerb
    {
        public enum ToggleType
        {
            Separate,
            Integrated,
            None
        }

        protected readonly VerbManager man;

        private int additionalCooldownTicksLeft = -1;

        private bool enabledInt = true;
        public AdditionalVerbProps Props;
        public VerbSource Source;
        public Verb Verb;

        public ManagedVerb(Verb verb, VerbSource source, AdditionalVerbProps props, VerbManager man)
        {
            Verb = verb;
            Source = source;
            Props = props;
            this.man = man;
            if (Props is {draw: true} && !Base.GetFeature<Feature_Drawing>().Enabled)
                Log.Error("[MVCF] Found a verb marked to draw while that feature is not enabled.");

            if (Props is {canFireIndependently: true} && !Base.GetFeature<Feature_IndependentVerbs>().Enabled)
                Log.Error("[MVCF] Found a verb marked to fire independently while that feature is not enabled.");

            if (Props is {separateToggle: false} && !Base.GetFeature<Feature_IntegratedToggle>().Enabled)
                Log.Error("[MVCF] Found a verb marked for an integrated toggle while that feature is not enabled.");
        }

        public float AdditionalCooldownPercent =>
            Props?.additionalCooldownTime <= 0 ? -1f : additionalCooldownTicksLeft / (Props?.additionalCooldownTime.SecondsToTicks() ?? -1f);

        public string AdditionalCooldownDesc => additionalCooldownTicksLeft.ToStringTicksToPeriodVerbose().Colorize(ColoredText.DateTimeColor);

        public virtual bool Enabled
        {
            get => enabledInt;
            set => enabledInt = value;
        }

        public virtual bool NeedsTicking => Props?.additionalCooldownTime > 0.001f;
        public bool GetToggleStatus() => enabledInt;

        public void Toggle()
        {
            enabledInt = !enabledInt;
            man.RecalcSearchVerb();
        }

        public virtual void DrawOn(Pawn p, Vector3 drawPos)
        {
        }

        public virtual void Tick()
        {
            if (additionalCooldownTicksLeft > 0) additionalCooldownTicksLeft--;
        }


        public virtual IEnumerable<Gizmo> GetGizmos(Thing ownerThing)
        {
            yield return new Command_VerbTargetExtended(this);

            if (GetToggleType() == ToggleType.Separate)
                yield return new Command_ToggleVerbUsage(this);
        }

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
    }
}