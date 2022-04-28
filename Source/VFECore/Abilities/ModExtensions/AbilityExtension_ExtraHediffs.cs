using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFECore.Abilities
{
    public class AbilityExtension_ExtraHediffs : AbilityExtension_AbilityMod
    {
        public List<HediffDef> onTarget;
        public List<HediffDef> onCaster;
        public StatDef         durationMultiplier;

        public override void Cast(LocalTargetInfo target, Ability ability)
        {
            base.Cast(target, ability);
            var duration = ability.GetDurationForPawn();

            if (target.Thing != null && durationMultiplier != null) duration = Mathf.RoundToInt(duration * target.Thing.GetStatValue(durationMultiplier));
            foreach (var def in onCaster)
            {
                var hediff = HediffMaker.MakeHediff(def, ability.pawn);
                if (hediff.TryGetComp<HediffComp_Disappears>() is HediffComp_Disappears disappears) disappears.ticksToDisappear = duration;
                ability.pawn.health.AddHediff(hediff);
            }

            if (target.Pawn is Pawn p)
                foreach (var def in onTarget)
                {
                    var hediff = HediffMaker.MakeHediff(def, p);
                    if (hediff.TryGetComp<HediffComp_Disappears>() is HediffComp_Disappears disappears)
                        disappears.ticksToDisappear = duration;

                    p.health.AddHediff(hediff);
                }
        }
    }
}
