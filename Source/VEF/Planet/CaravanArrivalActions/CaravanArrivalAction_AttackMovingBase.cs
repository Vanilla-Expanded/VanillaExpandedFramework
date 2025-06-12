using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace VEF.Planet
{
    public class CaravanArrivalAction_AttackMovingBase : CaravanArrivalAction_MovingBase
    {
        public override string Label => "AttackSettlement".Translate(movingBase.Label);

        public override string ReportString => "CaravanAttacking".Translate(movingBase.Label);

        public CaravanArrivalAction_AttackMovingBase()
        {
        }

        public CaravanArrivalAction_AttackMovingBase(MovingBase movingBase)
        {
            this.movingBase = movingBase;
        }

        public override FloatMenuAcceptanceReport StillValid(Caravan caravan, PlanetTile destinationTile)
        {
            return base.StillValid(caravan, destinationTile) && CanAttack(caravan, movingBase);
        }

        public override void Arrived(Caravan caravan)
        {
            movingBase.Attack(caravan);
        }


        public static FloatMenuAcceptanceReport CanAttack(Caravan caravan, MovingBase movingBase)
        {
            if (movingBase == null || !movingBase.Spawned || !movingBase.Attackable)
            {
                return false;
            }
            return true;
        }

        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, MovingBase movingBase)
        {
            return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanAttack(caravan, movingBase),
                () => CreateCaravanArrivalAction(new CaravanArrivalAction_AttackMovingBase(movingBase), caravan, movingBase), "AttackSettlement".Translate(movingBase.Label),
                caravan, movingBase.Tile, movingBase, movingBase.Faction.AllyOrNeutralTo(Faction.OfPlayer) ? ((Action<Action>)delegate (Action action)
                {
                    var confirmationMessage = movingBase.def.attackConfirmationMessage.NullOrEmpty()
                    ? "ConfirmAttackFriendlyFaction".Translate(movingBase.LabelCap, movingBase.Faction.Name) :
                    (TaggedString)movingBase.def.attackConfirmationMessage.Formatted(movingBase.LabelCap, movingBase.Faction.Name);
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(confirmationMessage, delegate
                    {
                        action();
                    }));
                }) : null);
        }
    }
}