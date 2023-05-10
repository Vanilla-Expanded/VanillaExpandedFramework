using System;
using System.Collections.Generic;
using System.Linq;
using MVCF.Commands;
using MVCF.Comps;
using MVCF.VerbComps;
using UnityEngine;
using Verse;

namespace MVCF;

public class VerbWithComps : ManagedVerb
{
    private readonly List<VerbComp> drawComps = new();
    private readonly List<VerbComp> tickComps = new();
    private List<VerbComp> comps = new();
    public override bool NeedsTicking => base.NeedsTicking || comps.Any(comp => comp.NeedsTicking);

    public override bool NeedsDrawing => base.NeedsDrawing || comps.Any(comp => comp.NeedsDrawing);

    public override bool Independent => base.Independent || comps.Any(comp => comp.Independent);
    public IEnumerable<VerbComp> GetComps() => comps;

    public override void Initialize(Verb verb, AdditionalVerbProps props, IEnumerable<VerbCompProperties> additionalComps)
    {
        base.Initialize(verb, props, additionalComps);
        var newComps = (props?.comps ?? Enumerable.Empty<VerbCompProperties>()).Concat(additionalComps ?? Enumerable.Empty<VerbCompProperties>()).ToList();

        if (comps.Any())
        {
            var oldTypes = comps.Select(c => c.GetType()).ToList();
            var newTypes = newComps.Select(c => c.compClass).ToList();

            MVCF.Log(oldTypes.Select(t => t.FullName).ToLineList("  - "), LogLevel.Silly);
            MVCF.Log("---- VS ----", LogLevel.Silly);
            MVCF.Log(newTypes.Select(t => t.FullName).ToLineList("  - "), LogLevel.Silly);
            if (!newTypes.SequenceEqual(oldTypes))
            {
                Log.Warning($"[MVCF] VerbWithComps: comps list changed, replacing. verb={verb}");
                comps.Clear();
            }
        }

        var lastIndex = 0;
        foreach (var compProps in newComps)
        {
            var comp = comps.Skip(lastIndex).FirstOrDefault(c => c.GetType() == compProps.compClass);
            if (comp == null)
            {
                comp = (VerbComp)Activator.CreateInstance(compProps.compClass);
                comps.Add(comp);
            }
            else
                lastIndex = comps.IndexOf(comp);

            comp.parent = this;
            comp.Initialize(compProps);
            if (comp.NeedsDrawing) drawComps.Add(comp);
            if (comp.NeedsTicking) tickComps.Add(comp);
        }
    }

    public override float GetScore(Pawn p, LocalTargetInfo target)
    {
        var score = base.GetScore(p, target);

        foreach (var comp in comps) comp.ModifyScore(p, target, ref score);

        return score;
    }

    public override bool ForceUse(Pawn pawn, LocalTargetInfo target) => base.ForceUse(pawn, target) || comps.Any(comp => comp.ForceUse(pawn, target));

    public override bool SetTarget(LocalTargetInfo target)
    {
        return !comps.Any(comp => !comp.SetTarget(target)) && base.SetTarget(target);
    }

    public override void Notify_Spawned()
    {
        foreach (var comp in comps) comp.Notify_Spawned();
    }

    public override void Notify_Despawned()
    {
        foreach (var comp in comps) comp.Notify_Despawned();
    }

    public override void DrawOn(Pawn p, Vector3 drawPos)
    {
        base.DrawOn(p, drawPos);
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < drawComps.Count; i++) drawComps[i].DrawOnAt(p, drawPos);
    }

    public override bool Available() => base.Available() && comps.All(comp => comp.Available());

    public override void Notify_ProjectileFired()
    {
        base.Notify_ProjectileFired();
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < comps.Count; i++) comps[i].Notify_ShotFired();
    }

    public override bool PreCastShot()
    {
        var flag = base.PreCastShot();
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < comps.Count; i++)
            if (!comps[i].PreCastShot())
                flag = false;

        return flag;
    }

    protected override Command GetToggleCommand(Thing ownerThing)
    {
        var command = base.GetToggleCommand(ownerThing);
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < comps.Count; i++)
        {
            var newCommand = comps[i].OverrideToggleCommand(command);
            if (newCommand is not null) return newCommand;
        }

        return command;
    }

    public override IEnumerable<CommandPart> GetCommandParts(Command_VerbTargetExtended command) =>
        base.GetCommandParts(command).Concat(comps.SelectMany(comp => comp.GetCommandParts(command)));

    protected override Command GetTargetCommand(Thing ownerThing)
    {
        var command = base.GetTargetCommand(ownerThing);
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < comps.Count; i++)
        {
            var newCommand = comps[i].OverrideTargetCommand(command);
            if (newCommand is not null) return newCommand;
        }

        return command;
    }

    public override void Tick()
    {
        base.Tick();
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < tickComps.Count; i++) tickComps[i].CompTick();
    }

    public override IEnumerable<Gizmo> GetGizmos(Thing ownerThing) => base.GetGizmos(ownerThing).Concat(comps.SelectMany(comp => comp.CompGetGizmosExtra()));

    public override void ModifyProjectile(ref ThingDef projectile)
    {
        base.ModifyProjectile(ref projectile);
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < comps.Count; i++)
        {
            var newProj = comps[i].ProjectileOverride(projectile);
            if (newProj is null) continue;
            projectile = newProj;
            return;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref comps, nameof(comps), LookMode.Deep);
        comps ??= new List<VerbComp>();
    }
}
