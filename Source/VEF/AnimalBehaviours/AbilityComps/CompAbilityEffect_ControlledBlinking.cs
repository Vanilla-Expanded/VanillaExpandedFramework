
using RimWorld;
using System.Collections.Generic;
using Verse;
namespace VEF.AnimalBehaviours
{
    public class CompAbilityEffect_ControlledBlinking : CompAbilityEffect
    {
        public new CompProperties_ControlledBlinking Props => (CompProperties_ControlledBlinking)props;

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!target.Cell.InBounds(parent.pawn.Map))
            {
                return false;
            }
            if(target.Cell.Impassable(parent.pawn.Map))
            {
                return false;
            }
            return true;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (Props.warpEffect)
            {
                FleckMaker.Static(parent.pawn.Position, parent.pawn.Map, FleckDefOf.PsycastAreaEffect, 10f);
            }
            parent.pawn.pather.StopDead();
            parent.pawn.Position = target.Cell;
            parent.pawn.pather.ResetToCurrentPosition();
        }
    }
}
