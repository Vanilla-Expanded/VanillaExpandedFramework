using Verse;

namespace VEF.Abilities
{
    using System.Collections.Generic;
    using RimWorld;
    using Verse.AI;

    public class JobDriver_StandAndFaceTarget : JobDriver
    {
        private CompAbilities cachedComp;
        public CompAbilities CompAbilities
        {
            get
            {
                if (cachedComp is null)
                {
                    cachedComp = this.TargetA.Pawn.GetComp<CompAbilities>();
                }
                return cachedComp;
            }
        }
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => CompAbilities.currentlyCasting is null);
            Toil toil = ToilMaker.MakeToil();
            toil.tickAction = delegate
            {
                if (base.pawn.pather.Moving)
                {
                    base.pawn.pather.StopDead();
                }
            };
            toil.tickIntervalAction = delegate(int delta)
            {
                if (base.pawn.GetPosture() == PawnPosture.Standing)
                {
                    base.pawn.rotationTracker.FaceTarget(TargetA);
                }
                base.pawn.GainComfortFromCellIfPossible(delta);
            };
            toil.socialMode = RandomSocialMode.Off;
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.handlingFacing = true;
            yield return toil;
        }
    }
}
