using Verse;

namespace VFECore.Abilities
{
    public class Ability_HediffDuration : Ability
    {
        public override void ApplyHediffs(LocalTargetInfo targetInfo)
        {
            var hediffExtension = def.GetModExtension<AbilityExtension_Hediff>();
            if (targetInfo.Pawn == null || !(hediffExtension?.applyAuto ?? false)) return;
            var localHediff = HediffMaker.MakeHediff(hediffExtension.hediff, targetInfo.Pawn);
            if (hediffExtension.severity > float.Epsilon)
                localHediff.Severity = hediffExtension.severity;
            if (localHediff is HediffWithComps hwc)
                foreach (var hediffComp in hwc.comps)
                    switch (hediffComp)
                    {
                        case HediffComp_Ability hca:
                            hca.ability = this;
                            break;
                        case HediffComp_Disappears hcd:
                            hcd.ticksToDisappear = GetDurationForPawn();
                            break;
                    }

            targetInfo.Pawn.health.AddHediff(localHediff);
        }
    }
}
