using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using VFE.Mechanoids.Buildings;
using VFE.Mechanoids.Needs;

namespace VFE.Mechanoids.AI.JobDrivers
{
	[StaticConstructorOnStartup]
	public class JobDriver_Recharge : JobDriver
	{
		public const TargetIndex BedOrRestSpotIndex = TargetIndex.A;
		public static ThingDef MoteRecharge = ThingDef.Named("VFE_Mechanoids_Mote_Recharge");
		public static ThingDef MoteRepair = ThingDef.Named("VFE_Mechanoids_Mote_Repair");

		private Building_BedMachine bed;
		public Building_BedMachine Bed
		{
			get
            {
				if (bed == null)
                {
					bed = (Building_BedMachine)job.GetTarget(TargetIndex.A).Thing;
                }
				return bed;
            }
		}

		private CompPowerTrader compPowerTrader;
		public CompPowerTrader BedCompPowerTrader
		{
			get
			{
				if (compPowerTrader == null)
				{
					compPowerTrader = Bed.TryGetComp<CompPowerTrader>();
				}
				return compPowerTrader;
			}
		}

		private Need_Power pawnNeed_Power;
		public Need_Power PawnNeed_Power
		{
			get
			{
				if (pawnNeed_Power == null)
				{
					pawnNeed_Power = pawn.needs.TryGetNeed<Need_Power>();
				}
				return pawnNeed_Power;
			}
		}

		private CompPowerTrader myBuildingCompPowerTrader;
		public CompPowerTrader MyBuildingCompPowerTrader
		{
			get
			{
				if (myBuildingCompPowerTrader == null)
				{
					this.myBuildingCompPowerTrader = CompMachine.cachedMachinesPawns[pawn].myBuilding.TryGetComp<CompPowerTrader>();
				}
				return myBuildingCompPowerTrader;
			}
		}

		private CompMachineChargingStation myBuildingCompMachineChargingStation;
		public CompMachineChargingStation MyBuildingCompMachineChargingStation
		{
			get
			{
				if (myBuildingCompMachineChargingStation == null)
				{
					this.myBuildingCompMachineChargingStation = CompMachine.cachedMachinesPawns[pawn].myBuilding.TryGetComp<CompMachineChargingStation>();
				}
				return myBuildingCompMachineChargingStation;
			}
		}
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (!job.GetTarget(TargetIndex.A).HasThing)
			{
				return false;
			}
			return true;
		}

		public override bool CanBeginNowWhileLyingDown()
		{
			return InBedOrRestSpotNow(pawn, job.GetTarget(TargetIndex.A));
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoCell(TargetA.Cell, PathEndMode.OnCell);
			yield return LayDown(TargetIndex.A);
		}

		public static bool InBedOrRestSpotNow(Pawn pawn, LocalTargetInfo bedOrRestSpot)
		{
			if (!bedOrRestSpot.IsValid || !pawn.Spawned)
			{
				return false;
			}
			if (bedOrRestSpot.HasThing)
			{
				if (bedOrRestSpot.Thing.Map != pawn.Map)
				{
					return false;
				}
				return bedOrRestSpot.Thing.Position == pawn.Position;
			}
			return false;
		}

		public Toil LayDown(TargetIndex bedOrRestSpotIndex) //Largely C&P'ed from vanilla LayDown toil
		{
			Toil layDown = new Toil();
			layDown.initAction = delegate
			{
				Pawn actor3 = layDown.actor;
				Job curJob = actor3.CurJob;
				actor3.pather.StopDead();
				MyBuildingCompMachineChargingStation.CompTickRare();
			};
			layDown.tickAction = delegate
			{
				Pawn actor2 = layDown.actor;
				Job curJob = actor2.CurJob;
				JobDriver curDriver2 = actor2.jobs.curDriver;
				if (BedCompPowerTrader.PowerOn)
				{
					PawnNeed_Power.TickResting(1f);
					if (actor2.IsHashIntervalTick(300) && !actor2.Position.Fogged(actor2.Map))
					{
						if (actor2.health.hediffSet.GetNaturallyHealingInjuredParts().Any())
						{
							MoteMaker.ThrowMetaIcon(actor2.Position, actor2.Map, MoteRepair);
						}
						else
                        {
							MoteMaker.ThrowMetaIcon(actor2.Position, actor2.Map, MoteRecharge);
                        }
					}
				}
				actor2.Rotation = Rot4.South;
				if (!MyBuildingCompMachineChargingStation.forceStay && actor2.IsHashIntervalTick(300))
				{
					actor2.jobs.CheckForJobOverride();
				}
			};
			layDown.AddFinishAction(delegate
			{
				Pawn actor = layDown.actor;
				if ((PawnNeed_Power.CurLevelPercentage > 0.99f && !layDown.actor.health.hediffSet.HasNaturallyHealingInjury() && MyBuildingCompMachineChargingStation.turretToInstall==null))
				{
					MyBuildingCompMachineChargingStation.wantsRest = false;
				}
			});
			layDown.handlingFacing = true;
			layDown.FailOn(() => !MyBuildingCompMachineChargingStation.forceStay && (!MyBuildingCompPowerTrader.PowerOn || 
				(PawnNeed_Power.CurLevelPercentage > 0.99f && !layDown.actor.health.hediffSet.HasNaturallyHealingInjury() && MyBuildingCompMachineChargingStation.turretToInstall==null)));

			layDown.defaultCompleteMode = ToilCompleteMode.Never;
			return layDown;
		}
	}
}

