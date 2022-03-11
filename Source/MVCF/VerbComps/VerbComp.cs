using System.Collections.Generic;
using MVCF.Commands;
using Verse;

namespace MVCF.VerbComps
{
    public abstract class VerbComp
    {
        public ManagedVerb parent;
        public VerbCompProperties props;
        public virtual bool NeedsTicking => false;

        public virtual void PostExposeData()
        {
        }

        public virtual void Initialize(VerbCompProperties props)
        {
            this.props = props;
        }

        public virtual void CompTick()
        {
        }

        public virtual void Notify_ShotFired()
        {
        }

        public virtual bool Available() => true;

        public virtual ThingDef ProjectileOverride(ThingDef oldProjectile) => null;

        public virtual IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield break;
        }

        public virtual Command_VerbTargetExtended OverrideTargetCommand(Command_VerbTargetExtended old) => null;

        public virtual Command_ToggleVerbUsage OverrideToggleCommand(Command_ToggleVerbUsage old) => null;

        public interface IVerbCompProvider
        {
            public IEnumerable<VerbCompProperties> GetCompsFor(VerbProperties verbProps);
        }
    }
}