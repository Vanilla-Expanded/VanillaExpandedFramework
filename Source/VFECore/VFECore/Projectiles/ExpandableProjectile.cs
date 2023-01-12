using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFECore
{
	public class ExpandableProjectile : Bullet
	{
		private int curDuration;
		private Vector3 startingPosition;
		private Vector3 prevPosition;
		private int curProjectileIndex;
		private int curProjectileFadeOutIndex;
		protected bool stopped;
		private float maxRange;
		public void SetDestinationToMax(Thing equipment)
		{
			maxRange = equipment.TryGetComp<CompEquippable>().PrimaryVerb.verbProps.range;
			var origin2 = new Vector3(this.launcher.TrueCenter().x, 0, this.launcher.TrueCenter().z);
			var destination2 = new Vector3(destination.x, 0, destination.z);
			var distance = Vector3.Distance(origin2, destination2);
			var distanceDiff = maxRange - distance;
			var normalized = (destination2 - origin2).normalized;
			this.destination += normalized * distanceDiff;
			var cell = this.destination.ToIntVec3();
			if (!cell.InBounds(this.Map))
			{
				var nearestCell = CellRect.WholeMap(this.Map).EdgeCells.OrderBy(x => x.DistanceTo(cell)).First();
				this.destination = nearestCell.ToVector3();
			}
			this.ticksToImpact = Mathf.CeilToInt(StartingTicksToImpact);
		}
		public new virtual int DamageAmount
		{
			get
			{
				return def.projectile.GetDamageAmount(weaponDamageMultiplier);
			}
		}
		public bool IsMoving
		{
			get
			{
				if (!stopped && this.DrawPos != prevPosition)
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
			DrawProjectile();
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
				if (!(this.launcher is Pawn))
				{
					this.startingPosition = this.launcher.OccupiedRect().CenterVector3;
				}
				else if (!pawnMoved && this.launcher is Pawn pawn && !pawn.Dead)
				{
					if (pawn.pather.MovingNow)
					{
						pawnMoved = true;
					}
					else
					{
						this.startingPosition = pawn.OccupiedRect().CenterVector3;
					}
				}
				return this.startingPosition;
			}
		}

		public Vector3? curPosition;
		public Vector3 CurPosition
		{
			get
			{
				if (stopped)
				{
					if (curPosition.HasValue)
					{
                        return curPosition.Value;
                    }
					return DrawPos;
                }
				else if (this.def.reachMaxRangeAlways)
				{
					var origin2 = new Vector3(this.launcher.TrueCenter().x, 0, this.launcher.TrueCenter().z);
					var curPos = this.DrawPos;
					var distance = Vector3.Distance(origin2, curPos);
					var distanceDiff = maxRange - distance;
					if (distanceDiff < 0)
					{
                        if (curPosition.HasValue)
                        {
                            return curPosition.Value;
                        }
                        return DrawPos;
                    }
					else
					{
						return this.DrawPos;
					}
				}
				else
				{
					return this.DrawPos;
				}
			}
		}
		public void DrawProjectile()
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
			if (this.launcher.Rotation == Rot4.West)
			{
				Vector3 startingPositionOffsetMirrored = this.def.startingPositionOffset;
				startingPositionOffsetMirrored.x = -startingPositionOffsetMirrored.x;
				pos += Quaternion.Euler(0, (startingPosition - currentPos).AngleFlat(), 0) * startingPositionOffsetMirrored;
			}

			else
			{
				pos += Quaternion.Euler(0, (startingPosition - currentPos).AngleFlat(), 0) * def.startingPositionOffset;

			}

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
			if (this.launcher.Rotation == Rot4.West)
			{
				Vector3 startingPositionOffsetMirrored = this.def.startingPositionOffset;
				startingPositionOffsetMirrored.x = -startingPositionOffsetMirrored.x;
				pos += Quaternion.Euler(0, (startingPosition - currentPos).AngleFlat(), 0) * startingPositionOffsetMirrored;


			}
			else
			{
				pos += Quaternion.Euler(0, (startingPosition - currentPos).AngleFlat(), 0) * this.def.startingPositionOffset;

			}

			var distance = Vector3.Distance(startingPosition, currentPos) * this.def.totalSizeScale;
			var distanceToTarget = Vector3.Distance(startingPosition, currentPos);
			var widthFactor = distance / distanceToTarget;

			var width = distance * this.def.widthScaleFactor * widthFactor;
			var height = distance * this.def.heightScaleFactor;
			var centerOfLine = pos.ToIntVec3();
			var startPosition = StartingPosition.ToIntVec3();
			var endPosition = this.CurPosition.ToIntVec3();
			if (points.Any())
			{
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
			}
			return positions;
		}

		private void StopMotion()
		{
			if (!stopped)
			{
				stopped = true;
				curPosition = this.DrawPos;
				this.destination = this.curPosition.Value;
			}
		}
		public override void Tick()
		{
			base.Tick();
			if (Find.TickManager.TicksGame % this.def.tickDamageRate == 0)
			{
				var projectileLine = MakeProjectileLine(StartingPosition, DrawPos, this.Map);
				foreach (var pos in projectileLine)
				{
					if (this.Destroyed is false)
					{
                        DoDamage(pos);
                    }
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
				if (!this.def.reachMaxRangeAlways && pawnMoved)
				{
					StopMotion();
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

		public virtual bool IsDamagable(Thing t)
		{
			return t.def != this.def && t != this.launcher && (t.def.useHitPoints || t is Pawn);
		}
		public virtual void DoDamage(IntVec3 pos)
		{

		}

		protected bool customImpact;

		public List<Thing> hitThings;
		protected override void Impact(Thing hitThing, bool blockedByShield = false)
		{
			if (stopped)
			{
                return;
            }
            if (hitThings == null) 
				hitThings = new List<Thing>();
			if (this.def.dealsDamageOnce && hitThings.Contains(hitThing))
			{
				return;
			}
			Map map = base.Map;
			IntVec3 position = base.Position;
			this.NotifyImpact(hitThing, map, position);
			if (hitThing != null && (!def.disableVanillaDamageMethod || customImpact && def.disableVanillaDamageMethod))
			{
				hitThings.Add(hitThing);
                BattleLogEntry_RangedImpact battleLogEntry_RangedImpact;
                if (equipmentDef == null)
                {
                    battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(launcher, hitThing, intendedTarget.Thing, ThingDef.Named("Gun_Autopistol"), def, targetCoverDef);
                }
                else
                {
                    battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(launcher, hitThing, intendedTarget.Thing, equipmentDef, def, targetCoverDef);
                }
                Find.BattleLog.Add(battleLogEntry_RangedImpact);
                DamageInfo dinfo = new DamageInfo(def.projectile.damageDef, this.DamageAmount, base.ArmorPenetration, ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing);
				hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
				Pawn pawn = hitThing as Pawn;
				if (pawn != null && pawn.stances != null && pawn.BodySize <= def.projectile.StoppingPower + 0.001f)
				{
					pawn.stances.stagger.StaggerFor(95);
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
				if (this.def.stopWhenHitAt.Contains(hitThing.def.defName))
				{
					if (!stopped)
                    {
                        StopMotion();
					}
				}
			}

            if (hitThing != null && def.stopWhenHit && !stopped)
            {
                StopMotion();
            }
        }

		private void NotifyImpact(Thing hitThing, Map map, IntVec3 position)
		{
			BulletImpactData bulletImpactData = default(BulletImpactData);
			bulletImpactData.bullet = this;
			bulletImpactData.hitThing = hitThing;
			bulletImpactData.impactPosition = position;
			BulletImpactData impactData = bulletImpactData;
			try
			{
				hitThing?.Notify_BulletImpactNearby(impactData);
			}
			catch { };
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
						try
						{
							thingList[j].Notify_BulletImpactNearby(impactData);
						}
						catch { };
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
			Scribe_Values.Look(ref maxRange, "maxRange");
		}
	}
}
