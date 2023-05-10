using System.Collections.Generic;
using MVCF.Commands;
using UnityEngine;
using Verse;

namespace MVCF.VerbComps;

public abstract class VerbComp : IExposable
{
    public ManagedVerb parent;
    public VerbCompProperties props;
    public virtual bool NeedsTicking => false;

    public virtual bool Independent => false;
    public virtual bool NeedsDrawing => false;

    public virtual void ExposeData() { }

    public virtual void DrawOnAt(Pawn p, Vector3 drawPos) { }

    public virtual void ModifyScore(Pawn p, LocalTargetInfo target, ref float score) { }

    public virtual bool ForceUse(Pawn p, LocalTargetInfo target) => false;

    public virtual void Notify_Spawned() { }

    public virtual void Notify_Despawned() { }

    public virtual bool SetTarget(LocalTargetInfo target) => true;

    public virtual IEnumerable<CommandPart> GetCommandParts(Command_VerbTargetExtended command)
    {
        yield break;
    }

    public virtual void Initialize(VerbCompProperties props)
    {
        this.props = props;
    }

    public virtual void CompTick() { }

    public virtual void Notify_ShotFired() { }

    public virtual bool PreCastShot() => true;

    public virtual bool Available() => true;

    public virtual ThingDef ProjectileOverride(ThingDef oldProjectile) => null;

    public virtual IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        yield break;
    }

    public virtual Command OverrideTargetCommand(Command old) => null;

    public virtual Command OverrideToggleCommand(Command old) => null;

    public interface IVerbCompProvider
    {
        public IEnumerable<VerbCompProperties> GetCompsFor(VerbProperties verbProps);
    }
}
