using RimWorld;
using RimWorld.Planet;
using System.Linq;
using Verse;

namespace VFECore.Abilities
{
    public class Ability_HediffDuration : Ability
    {
        public override void ApplyHediffs(LocalTargetInfo targetInfo)
        {
            ApplyHediff(this, targetInfo);
        }
        public override void ApplyHediffs(params GlobalTargetInfo[] targetInfo)
        {
            foreach (GlobalTargetInfo target in targetInfo)
            {
                ApplyHediff(this, (LocalTargetInfo)target);
            }
        }
        public static void ApplyHediff(Ability ability, LocalTargetInfo targetInfo)
        {
            var hediffExtension = ability.def.GetModExtension<AbilityExtension_Hediff>();
            if (targetInfo.Pawn == null || !(hediffExtension?.applyAuto ?? false)) return;
            BodyPartRecord bodyPart = hediffExtension.bodyPartToApply != null
                ? ability.pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == hediffExtension.bodyPartToApply)
                : null;
            var localHediff = HediffMaker.MakeHediff(hediffExtension.hediff, targetInfo.Pawn, bodyPart);
            if (hediffExtension.severity > float.Epsilon)
                localHediff.Severity = hediffExtension.severity;
            if (localHediff is Hediff_Ability hediffAbility)
            {
                hediffAbility.ability = ability;
            }
            var duration = ability.GetDurationForPawn();
            if (hediffExtension.durationMultiplier != null)
            {
                duration = (int)(duration * targetInfo.Pawn.GetStatValue(hediffExtension.durationMultiplier));
            }
            if (localHediff is HediffWithComps hwc)
                foreach (var hediffComp in hwc.comps)
                    switch (hediffComp)
                    {
                        case HediffComp_Ability hca:
                            hca.ability = ability;
                            break;
                        case HediffComp_Disappears hcd:
                            hcd.ticksToDisappear = duration;
                            break;
                    }

            targetInfo.Pawn.health.AddHediff(localHediff);
        }
    }
}
