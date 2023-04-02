using System.Collections.Generic;
using System.Linq;
using MVCF.Commands;
using MVCF.Comps;
using MVCF.Features;
using MVCF.Utilities;
using MVCF.VerbComps;
using UnityEngine;
using Verse;

namespace MVCF;

public class ManagedVerb : IExposable, ILoadReferenceable
{
    public enum ToggleType
    {
        Separate, Integrated, None
    }

    public AdditionalVerbProps Props;
    public VerbSource Source = VerbSource.None;
    public Verb Verb;

    private bool enabledInt = true;

    private string loadId;

    public VerbManager Manager { get; set; }

    public virtual bool Enabled
    {
        get => enabledInt;
        set => enabledInt = value;
    }

    public virtual bool NeedsTicking => false;
    public virtual bool NeedsDrawing => false;

    public virtual bool Independent => false;

    public virtual void ExposeData()
    {
        Scribe_Values.Look(ref enabledInt, "enabled");
        Scribe_Values.Look(ref loadId, "loadId");
    }

    public string GetUniqueLoadID() => loadId;

    public virtual bool SetTarget(LocalTargetInfo target) => true;

    public virtual void Notify_Spawned() { }

    public virtual void Notify_Despawned() { }

    public virtual void Initialize(Verb verb, AdditionalVerbProps props, IEnumerable<VerbCompProperties> additionalComps)
    {
        Verb = verb;
        Props = props;
        loadId = $"{verb.loadID}_Managed";
        if (Props is { draw: true } && !MVCF.GetFeature<Feature_Drawing>().Enabled)
            Log.Error("[MVCF] Found a verb marked to draw while that feature is not enabled.");

        if (Props is { canFireIndependently: true } && !MVCF.GetFeature<Feature_IndependentVerbs>().Enabled)
            Log.Error("[MVCF] Found a verb marked to fire independently while that feature is not enabled.");

        if (Props is { separateToggle: false, canBeToggled: true } && !MVCF.GetFeature<Feature_IntegratedToggle>().Enabled)
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

    public virtual void Notify_ProjectileFired() { }

    public virtual IEnumerable<CommandPart> GetCommandParts(Command_VerbTargetExtended command)
    {
        yield break;
    }

    public virtual void ModifyProjectile(ref ThingDef projectile) { }

    public bool GetToggleStatus() => enabledInt;

    public void Toggle()
    {
        enabledInt = !enabledInt;
        Manager?.RecalcSearchVerb();
    }

    public virtual void DrawOn(Pawn p, Vector3 drawPos) { }

    public virtual void Tick() { }

    public virtual IEnumerable<Gizmo> GetGizmos(Thing ownerThing)
    {
        yield return GetTargetCommand(ownerThing);

        if (GetToggleType() == ToggleType.Separate)
            yield return GetToggleCommand(ownerThing);
    }

    protected virtual Command GetToggleCommand(Thing ownerThing) => new Command_ToggleVerbUsage(this);

    protected virtual Command GetTargetCommand(Thing ownerThing) => new Command_VerbTargetExtended(this, ownerThing);

    public virtual ToggleType GetToggleType()
    {
        var canIntegrated = MVCF.GetFeature<Feature_IntegratedToggle>().Enabled;
        if (Props == null)
        {
            if (!Verb.CasterIsPawn) return ToggleType.None;
            Manager ??= Verb.CasterPawn.Manager();
            return Verb.CasterPawn.RaceProps.Animal
                || (Manager?.ManagedVerbs.Count(mv => !mv.Verb.IsMeleeAttack && !mv.Independent) ?? 1) > 1
                ? canIntegrated ? ToggleType.Integrated : ToggleType.Separate
                : ToggleType.None;
        }

        if (!Props.canBeToggled) return ToggleType.None;
        if (Props.separateToggle) return ToggleType.Separate;
        if (canIntegrated) return ToggleType.Integrated;

        Log.ErrorOnce(
            $"[MVCF] {Verb.EquipmentSource.LabelShortCap ?? $"Hediff verb of {Verb.caster}"} wants an integrated toggle but that feature is not enabled. Using seperate toggle.",
            Verb.GetHashCode());
        return ToggleType.Separate;
    }

    public virtual float GetScore(Pawn p, LocalTargetInfo target)
    {
        MVCF.LogFormat($"Getting score of {Verb} with target {target}", LogLevel.Silly);
        if (Verb is IVerbScore verbScore) return verbScore.GetScore(p, target);
        var accuracy = 0f;
        if (target.HasThing && !target.Thing.Spawned) target = target.Thing.PositionHeld;
        if (p.Map != null)
            accuracy = ShotReport.HitReportFor(p, Verb, target).TotalEstimatedHitChance;
        else if (Verb.TryFindShootLineFromTo(p.Position, target, out var line))
            accuracy = Verb.verbProps.GetHitChanceFactor(Verb.EquipmentSource, line.Source.DistanceTo(line.Dest));

        var damage = accuracy * Verb.verbProps.burstShotCount * Verb.GetDamage();
        var timeSpent = Verb.verbProps.AdjustedCooldownTicks(Verb, p) + Verb.verbProps.warmupTime.SecondsToTicks();

        MVCF.LogFormat($"Accuracy: {accuracy}", LogLevel.Silly);
        MVCF.LogFormat($"Damage: {damage}", LogLevel.Silly);
        MVCF.LogFormat($"timeSpent: {timeSpent}", LogLevel.Silly);
        MVCF.LogFormat($"Score of {Verb} on target {target} is {damage / timeSpent}", LogLevel.Silly);


        return damage / timeSpent;
    }

    public virtual bool PreCastShot() => true;
}
