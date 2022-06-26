using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VFECore.Abilities
{
    public class AbilityExtension_Stun : AbilityExtension_AbilityMod
    {
        public IntRange? stunTicks;
        public StatDef durationMultiplier;
        public bool durationMultiplierFromCaster;
        public override void Cast(GlobalTargetInfo[] targets, Ability ability)
        {
            base.Cast(targets, ability);
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i].Thing is Pawn p && p.Spawned)
                {
                    var stunTicks = this.stunTicks.HasValue ? this.stunTicks.Value.RandomInRange : ability.GetDurationForPawn();
                    if (durationMultiplier != null)
                    {
                        stunTicks = (int)(stunTicks * (durationMultiplierFromCaster ? ability.pawn.GetStatValue(durationMultiplier) 
                            : p.GetStatValue(durationMultiplier)));
                    }
                    p.stances.stunner.StunFor(stunTicks, ability.pawn);
                }
            }
        }
    }
}
