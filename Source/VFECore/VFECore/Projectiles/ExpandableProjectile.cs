using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VFECore
{
	public class ExpandableProjectile : Bullet
	{
		private int curDuration;
		private Vector3 startingPosition;
		private Vector3 prevPosition;
		private int curProjectileIndex;
		private int curProjectileFadeOutIndex;
		private bool stopped;
		public bool IsMoving
		{
			get
			{
				if (this.DrawPos != prevPosition)
				{
					prevPosition = this.DrawPos;
					return true;
				}
				return false;
			}
		}
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				startingPosition = this.Position.ToVector3Shifted();
				startingPosition.y = 0;
			}
		}

		private int prevTick;
		public bool doFinalAnimations;
		public new ExpandableProjectileDef def => base.def as ExpandableProjectileDef;
		private Material ProjectileMat
		{
			get
			{

				if (!doFinalAnimations || this.def.lifeTimeDuration - curDuration > this.def.graphicData.MaterialsFadeOut.Length - 1)
				{
					var material = this.def.graphicData.Materials[curProjectileIndex];
					if (prevTick != Find.TickManager.TicksAbs && Find.TickManager.TicksAbs - this.TickFrameRate >= prevTick)
					{
						if (curProjectileIndex == this.def.graphicData.Materials.Length - 1)
							curProjectileIndex = 0;
						else curProjectileIndex++;
						prevTick = Find.TickManager.TicksAbs;
					}
					return material;
				}
				else
				{
					var material = this.def.graphicData.MaterialsFadeOut[curProjectileFadeOutIndex];
					if (prevTick != Find.TickManager.TicksAbs && Find.TickManager.TicksAbs - this.TickFrameRate >= prevTick)
					{
						if (this.def.graphicData.MaterialsFadeOut.Length - 1 != curProjectileFadeOutIndex)
						{
							curProjectileFadeOutIndex++;
						}
						prevTick = Find.TickManager.TicksAbs;
					}
					return material;
				}
			}
		}

		public override void Draw()
		{
			DrawProjectileBackground();
		}

		public int TickFrameRate
		{
			get
			{
				if (!doFinalAnimations)
				{
					return this.def.tickFrameRate;
				}
				else if (this.def.finalTickFrameRate > 0)
				{
					return this.def.finalTickFrameRate;
				}
				return this.def.tickFrameRate;
			}
		}

		public bool pawnMoved;
		public Vector3 StartingPosition
		{
			get
			{
				if (!pawnMoved && this.launcher is Pawn pawn)
				{
					if (pawn.pather.MovingNow)
					{
						pawnMoved = true;
					}
					else
					{
						this.startingPosition = pawn.Position.ToVector3Shifted();
					}
				}
				return this.startingPosition;
			}
		}

		public Vector3 curPosition;
		public Vector3 CurPosition
		{
			get
			{
				if (!stopped)
				{
					return this.DrawPos;
				}
				return curPosition;
			}
		}
		public void DrawProjectileBackground()
		{
			var currentPos = CurPosition;
			currentPos.y = 0;
			var startingPosition = StartingPosition;
			startingPosition.y = 0;
			var destination = new Vector3(this.destination.x, this.destination.y, this.destination.z);
			destination.y = 0;

			Quaternion quat = Quaternion.LookRotation(currentPos - startingPosition);
			Vector3 pos = (startingPosition + currentPos) / 2f;
			pos.y = 10;
			pos += Quaternion.Euler(0, (startingPosition - currentPos).AngleFlat(), 0) * def.startingPositionOffset;

			var distance = Vector3.Distance(startingPosition, currentPos) * def.totalSizeScale;
			var distanceToTarget = Vector3.Distance(startingPosition, destination);
			var widthFactor = distance / distanceToTarget;

			var vec = new Vector3(distance * def.widthScaleFactor * widthFactor, 0f, distance * def.heightScaleFactor);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(pos, quat, vec);
			Graphics.DrawMesh(MeshPool.plane10, matrix, ProjectileMat, 0);
		}

		public HashSet<IntVec3> MakeProjectileLine(Vector3 start, Vector3 end, Map map)
		{
			var resultingLine = new ShootLine(start.ToIntVec3(), end.ToIntVec3());
			var points = resultingLine.Points();
			HashSet<IntVec3> positions = new HashSet<IntVec3>();

			var currentPos = CurPosition;
			currentPos.y = 0;
			var startingPosition = StartingPosition;
			startingPosition.y = 0;
			var destination = new Vector3(currentPos.x, currentPos.y, currentPos.z);

			Vector3 pos = (startingPosition + currentPos) / 2f;
			pos.y = 10;
			pos += Quaternion.Euler(0, (startingPosition - currentPos).AngleFlat(), 0) * this.def.startingPositionOffset;

			var distance = Vector3.Distance(startingPosition, currentPos) * this.def.totalSizeScale;
			var distanceToTarget = Vector3.Distance(startingPosition, currentPos);
			var widthFactor = distance / distanceToTarget;

			var width = distance * this.def.widthScaleFactor * widthFactor;
			var height = distance * this.def.heightScaleFactor;
			var centerOfLine = pos.ToIntVec3();
			var startPosition = StartingPosition.ToIntVec3();
			var endPosition = this.CurPosition.ToIntVec3();
			foreach (var cell in GenRadial.RadialCellsAround(start.ToIntVec3(), height, true))
			{
				if (centerOfLine.DistanceToSquared(endPosition) >= cell.DistanceToSquared(endPosition) && startPosition.DistanceTo(cell) > def.minDistanceToAffect)
				{
					var nearestCell = points.MinBy(x => x.DistanceToSquared(cell));
					if ((width / height) * 2.5f > nearestCell.DistanceToSquared(cell))
					{
						positions.Add(cell);
					}
				}
			}
			foreach (var cell in points)
			{
				var startCellDistance = startPosition.DistanceTo(cell);
				if (startCellDistance > def.minDistanceToAffect && startCellDistance <= startPosition.DistanceTo(endPosition))
				{
					positions.Add(cell);
				}
			}
			return positions;
		}
		public override void Tick()
		{
			base.Tick();
			if (Find.TickManager.TicksGame % this.def.tickDamageRate == 0)
			{
				var projectileLine = MakeProjectileLine(StartingPosition, DrawPos, this.Map);
				foreach (var pos in projectileLine)
				{
					DoDamage(pos);
				}
			}
			if (!doFinalAnimations && (!IsMoving || pawnMoved))
			{
				doFinalAnimations = true;
				var finalAnimationDuration = this.def.lifeTimeDuration - this.def.graphicData.MaterialsFadeOut.Length;
				if (finalAnimationDuration > curDuration)
				{
					curDuration = finalAnimationDuration;
				}
				if (pawnMoved)
				{
					stopped = true;
					curPosition = this.DrawPos;
				}
			}
			if (Find.TickManager.TicksGame % this.TickFrameRate == 0 && def.lifeTimeDuration > 0)
			{
				curDuration++;
				if (curDuration > def.lifeTimeDuration)
				{
					this.Destroy();
				}
			}
		}
		public virtual void DoDamage(IntVec3 pos)
		{
			//if (pos != this.launcher.Position)
			//{
			//	GenSpawn.Spawn(ThingDefOf.MineableGold, pos, this.Map);
			//}
		}

		protected bool customImpact;

		public HashSet<Thing> hitThings;
		protected override void Impact(Thing hitThing)
		{
			if (!stopped && !customImpact)
			{
				stopped = true;
				curPosition = this.DrawPos;
			}
			if (hitThings == null) hitThings = new HashSet<Thing>();
			if (this.def.dealsDamageOnce && hitThings.Contains(hitThing))
            {
				return;
            }
			hitThings.Add(hitThing);
			//if (!customImpact)
			//{
			//	GenSpawn.Spawn(ThingDefOf.MineableComponentsIndustrial, base.Position, this.Map);
			//}
			Map map = base.Map;
			IntVec3 position = base.Position;
			BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(launcher, hitThing, intendedTarget.Thing, equipmentDef, def, targetCoverDef);
			Find.BattleLog.Add(battleLogEntry_RangedImpact);
			this.NotifyImpact(hitThing, map, position);
			if (hitThing != null && (!def.disableVanillaDamageMethod || customImpact && def.disableVanillaDamageMethod))
			{
				DamageInfo dinfo = new DamageInfo(def.projectile.damageDef, base.DamageAmount, base.ArmorPenetration, ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing);
				hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
				Pawn pawn = hitThing as Pawn;
				if (pawn != null && pawn.stances != null && pawn.BodySize <= def.projectile.StoppingPower + 0.001f)
				{
					pawn.stances.StaggerFor(95);
				}
				if (def.projectile.extraDamages != null)
				{
					foreach (ExtraDamage extraDamage in def.projectile.extraDamages)
					{
						if (Rand.Chance(extraDamage.chance))
						{
							DamageInfo dinfo2 = new DamageInfo(extraDamage.def, extraDamage.amount, extraDamage.AdjustedArmorPenetration(), ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing);
							hitThing.TakeDamage(dinfo2).AssociateWithLog(battleLogEntry_RangedImpact);
						}
					}
				}
			}
		}

		private void NotifyImpact(Thing hitThing, Map map, IntVec3 position)
		{
			BulletImpactData bulletImpactData = default(BulletImpactData);
			bulletImpactData.bullet = this;
			bulletImpactData.hitThing = hitThing;
			bulletImpactData.impactPosition = position;
			BulletImpactData impactData = bulletImpactData;
			hitThing?.Notify_BulletImpactNearby(impactData);
			int num = 9;
			for (int i = 0; i < num; i++)
			{
				IntVec3 c = position + GenRadial.RadialPattern[i];
				if (!c.InBounds(map))
				{
					continue;
				}
				List<Thing> thingList = c.GetThingList(map);
				for (int j = 0; j < thingList.Count; j++)
				{
					if (thingList[j] != hitThing)
					{
						thingList[j].Notify_BulletImpactNearby(impactData);
					}
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref startingPosition, "startingPosition");
			Scribe_Values.Look(ref doFinalAnimations, "doFinalAnimations");
			Scribe_Values.Look(ref pawnMoved, "pawnMoved");
			Scribe_Values.Look(ref curDuration, "curDuration");
			Scribe_Values.Look(ref curProjectileIndex, "curProjectileIndex");
			Scribe_Values.Look(ref curProjectileFadeOutIndex, "curProjectileFadeOutIndex");
			Scribe_Values.Look(ref prevTick, "prevTick");
			Scribe_Values.Look(ref prevPosition, "prevPosition");
			Scribe_Values.Look(ref stopped, "stopped");
			Scribe_Values.Look(ref curPosition, "curPosition");
		}
	}
}
