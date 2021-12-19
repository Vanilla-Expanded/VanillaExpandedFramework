using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFECore
{
    using RimWorld;
    using Verse;
    using Verse.AI;

    public class JobDriver_LeaveMap : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return new Toil
                         {
                             initAction = () =>
                                          {
                                              FleckMaker.ThrowSmoke(this.pawn.Position.ToVector3(), this.Map, 2f);
                                              this.pawn.ExitMap(false, Rot4.Random);
                                              Find.World.GetComponent<HiringContractTracker>().pawns.Remove(this.pawn);
                                          }
                         }.FailOn(() => this.pawn.Dead);
        }
    }
}