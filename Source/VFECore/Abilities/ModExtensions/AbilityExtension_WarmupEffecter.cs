using Verse;
using Verse.AI;

namespace VFECore.Abilities
{
    public class AbilityExtension_WarmupEffecter : AbilityExtension_AbilityMod
    {
        public bool        onCaster;
        public EffecterDef effecterDef;
        public float       scale = 1f;
        public override void WarmupToil(Toil toil)
        {
            base.WarmupToil(toil);
            Effecter   effecter = null;
            TargetInfo target = TargetInfo.Invalid;

            void InitEffecter()
            {
                var comp    = toil.actor.GetComp<CompAbilities>();
                var ability = comp.currentlyCasting;
                if (onCaster)
                {
                    target   = toil.actor;
                    effecter = effecterDef.Spawn(ability.pawn.Position, ability.pawn.Map, scale);
                }
                else
                {
                    var castingTarget = comp.currentlyCastingTargets[0];
                    if (castingTarget.HasThing)
                    {
                        target   = new TargetInfo(castingTarget.Thing);
                        effecter = effecterDef.Spawn(castingTarget.Thing, castingTarget.Map, scale);
                    }
                    else
                    {
                        target   = new TargetInfo(castingTarget.Cell, castingTarget.Map);
                        effecter = effecterDef.Spawn(castingTarget.Cell, castingTarget.Map, scale);
                    }
                }
            }

            toil.AddPreInitAction(InitEffecter);
            toil.AddPreTickAction(delegate
            {
                if (effecter == null || !target.IsValid) InitEffecter();
                effecter?.EffectTick(target, target);
            });
            toil.AddFinishAction(delegate
            {
                effecter?.Cleanup();
            });
        }
    }
}
