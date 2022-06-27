using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFECore.Abilities
{
    using RimWorld.Planet;

    public class AbilityExtension_ExtraHediffs : AbilityExtension_AbilityMod
    {
        public List<HediffDef> onTarget;
        public List<HediffDef> onCaster;
        public StatDef         durationMultiplier;
        public int?            durationTimeOverride = null;

        public override void Cast(GlobalTargetInfo[] targets, Ability ability)
        {
            base.Cast(targets, ability);
            var duration = durationTimeOverride ?? ability.GetDurationForPawn();

            foreach (GlobalTargetInfo target in targets)
            {


                if (target.Thing != null && durationMultiplier != null)
                    duration = Mathf.RoundToInt(duration * target.Thing.GetStatValue(durationMultiplier));

                if (onCaster != null)
                    foreach (var def in onCaster)
                    {
                        var hediff = HediffMaker.MakeHediff(def, ability.pawn);
                        if (hediff.TryGetComp<HediffComp_Disappears>() is HediffComp_Disappears disappears) disappears.ticksToDisappear = duration;
                        ability.pawn.health.AddHediff(hediff);
                    }

                if (target.Thing is Pawn p && onTarget != null)
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
}
