using RimWorld;
using System.Linq;
using Verse;
using static HarmonyLib.Code;

namespace VFECore.Abilities
{
    public class Ability_HediffDuration : Ability
    {
        public override void ApplyHediffs(LocalTargetInfo targetInfo)
        {
            var hediffExtension = def.GetModExtension<AbilityExtension_Hediff>();
            if (targetInfo.Pawn == null || !(hediffExtension?.applyAuto ?? false)) return;
            BodyPartRecord bodyPart = hediffExtension.bodyPartToApply != null
                ? pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == hediffExtension.bodyPartToApply)
                : null;            
            var localHediff = HediffMaker.MakeHediff(hediffExtension.hediff, targetInfo.Pawn, bodyPart);
            if (hediffExtension.severity > float.Epsilon)
                localHediff.Severity = hediffExtension.severity;
            if (localHediff is Hediff_Ability hediffAbility)
            {
                hediffAbility.ability = this;
            }
            var duration = GetDurationForPawn();
            if (hediffExtension.durationMultiplier != null)
            {
                duration = (int)(duration * targetInfo.Pawn.GetStatValue(hediffExtension.durationMultiplier));
            }
            if (localHediff is HediffWithComps hwc)
                foreach (var hediffComp in hwc.comps)
                    switch (hediffComp)
                    {
                        case HediffComp_Ability hca:
                            hca.ability = this;
                            break;
                        case HediffComp_Disappears hcd:
                            hcd.ticksToDisappear = duration;
                            break;
                    }

            targetInfo.Pawn.health.AddHediff(localHediff);
        }
    }
}
