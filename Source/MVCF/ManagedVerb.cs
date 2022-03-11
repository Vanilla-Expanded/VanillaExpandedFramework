using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public List<VerbComp> Comps = new();

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

        public virtual bool NeedsTicking => Comps.Any(comp => comp.NeedsTicking);

        public void ExposeData()
        {
            Scribe_Values.Look(ref enabledInt, "enabled");
            Scribe_Values.Look(ref loadId, "loadId");
            foreach (var comp in Comps) comp.PostExposeData();
        }

        public string GetUniqueLoadID() => loadId;

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

            var comps = (props?.comps ?? Enumerable.Empty<VerbCompProperties>()).Concat(additionalComps ?? Enumerable.Empty<VerbCompProperties>());
            foreach (var compProps in comps)
            {
                var comp = (VerbComp) Activator.CreateInstance(compProps.compClass);
                comp.parent = this;
                Comps.Add(comp);
                comp.Initialize(compProps);
            }
        }

        public void Notify_Added(VerbManager man, VerbSource source)
        {
            Manager = man;
            Source = source;
        }

        public void Notify_Removed()
        {
            Manager = null;
            Source = VerbSource.None;
        }

        public virtual bool Available() => Comps.All(comp => comp.Available());

        public virtual void Notify_ProjectileFired()
        {
            for (var i = 0; i < Comps.Count; i++) Comps[i].Notify_ShotFired();
        }

        public virtual void ModifyProjectile(ref ThingDef projectile)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Comps.Count; i++)
            {
                var newProj = Comps[i].ProjectileOverride(projectile);
                if (newProj is null) continue;
                projectile = newProj;
                return;
            }
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
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Comps.Count; i++) Comps[i].CompTick();
        }


        public virtual IEnumerable<Gizmo> GetGizmos(Thing ownerThing)
        {
            yield return GetTargetCommand(ownerThing);

            if (GetToggleType() == ToggleType.Separate)
                yield return GetToggleCommand(ownerThing);

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Comps.Count; i++)
                foreach (var gizmo in Comps[i].CompGetGizmosExtra())
                    yield return gizmo;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Command_ToggleVerbUsage GetToggleCommand(Thing ownerThing)
        {
            var command = new Command_ToggleVerbUsage(this);

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Comps.Count; i++)
            {
                var newCommand = Comps[i].OverrideToggleCommand(command);
                if (newCommand is not null) return newCommand;
            }

            return command;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Command_VerbTargetExtended GetTargetCommand(Thing ownerThing)
        {
            var command = new Command_VerbTargetExtended(this, ownerThing);
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Comps.Count; i++)
            {
                var newCommand = Comps[i].OverrideTargetCommand(command);
                if (newCommand is not null) return newCommand;
            }

            return command;
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