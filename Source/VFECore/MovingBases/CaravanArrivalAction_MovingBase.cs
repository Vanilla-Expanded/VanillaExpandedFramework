using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VFECore
{
    public class CaravanArrivalAction_MovingBase : CaravanArrivalAction
    {
        public MovingBase movingBase;

        public override string Label => throw new System.NotImplementedException();

        public override string ReportString => throw new System.NotImplementedException();

        public override void Arrived(Caravan caravan)
        {
            Caravan_PathFollower_ExposeData_Patch.caravansToFollow.Remove(caravan.pather);
        }

        public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile)
        {
            FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(caravan, destinationTile);
            if (!floatMenuAcceptanceReport)
            {
                return floatMenuAcceptanceReport;
            }
            if (movingBase != null && movingBase.Tile != destinationTile)
            {
                return false;
            }
            return true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref movingBase, "movingBase");
        }

        public static void SetDestination(Caravan caravan, MovingBase movingBase)
        {
            Caravan_PathFollower_ExposeData_Patch.caravansToFollow[caravan.pather] = new MovingBaseDestinationAction
            {
                destination = movingBase,
                arrivalActionType = typeof(CaravanArrivalAction_AttackMovingBase)
            };
        }
    }
}