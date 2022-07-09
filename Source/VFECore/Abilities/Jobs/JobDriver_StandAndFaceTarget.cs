namespace VFECore.Abilities
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
            Toil toil = new Toil();
            toil.tickAction = delegate
            {
                if (base.pawn.pather.Moving)
                {
                    base.pawn.pather.StopDead();
                }
                if (base.pawn.GetPosture() == PawnPosture.Standing)
                {
                    base.pawn.rotationTracker.FaceTarget(TargetA);
                }
                base.pawn.GainComfortFromCellIfPossible();
            };
            toil.socialMode = RandomSocialMode.Off;
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.handlingFacing = true;
            yield return toil;
        }
    }
}
