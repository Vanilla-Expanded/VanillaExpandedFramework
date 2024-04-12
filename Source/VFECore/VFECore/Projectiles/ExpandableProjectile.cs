using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFECore
{
    public class ExpandableProjectile : Bullet
	{
		private int curDuration;
		public Vector3 startingPosition;
		private Vector3 prevPosition;
		private int curProjectileIndex=0;
		private int curProjectileFadeOutIndex=0;
		protected bool stopped;
		private float maxRange;
		public void SetDestinationToMax(Thing equipment)
		{
			this.maxRange = Mathf.Min(Mathf.Max(Map.Size.x, Map.Size.z), GetMaxRange(equipment));
            var origin2 = new Vector3(this.origin.x, 0, this.origin.z);
			var destination2 = new Vector3(destination.x, 0, destination.z);
			var distance = Vector3.Distance(origin2, destination2);
			var distanceDiffMax = maxRange - distance;
			var normalized = (destination2 - origin2).normalized;
			var distanceDiff = 1;
            while (true)
			{
				if (distanceDiff >= distanceDiffMax)
                {
                    this.destination += normalized * distanceDiff;
                    break;
				}
				var newCell = (this.destination + (normalized * distanceDiff)).ToIntVec3();
				if (newCell.InBounds(Map) is false)
				{
                    this.destination += normalized * (distanceDiff + 10); // goes over map edge by like 10 cells just to not cut off projectile too early
					break;
                }
				distanceDiff += 1;
            }
			this.ticksToImpact = Mathf.CeilToInt(StartingTicksToImpact);
		}

        private float GetMaxRange(Thing equipment)
        {
            if (VehicleFramework_Turret_Patch.currentFiringVehicleTurret is not null)
            {
				return (float)(VehicleFramework_Turret_Patch.maxRangeInfo
					.Invoke(VehicleFramework_Turret_Patch.currentFiringVehicleTurret, null));
            }
			var comp = equipment.TryGetComp<CompEquippable>();
			if (comp != null)
			{
                return comp.PrimaryVerb.verbProps.range;
            }
			throw new Exception("[VEF] Couldn't determine max range for " + this.Label);
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
						if (this.def.graphicData.Materials.Length - 1 != curProjectileIndex)
						{
							curProjectileIndex++;
						}						
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

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
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


		public bool LauncherIsVehicle
		{
			get
			{
                if (VehicleFramework_Turret_Patch.VFLoaded && VehicleFramework_Turret_Patch.VehicleType.IsAssignableFrom(this.launcher.GetType()))
                {
					return true;
                }
				return false;
            }
		}

		public bool pawnMoved;
		public Vector3 StartingPosition
		{
			get
			{
				if (LauncherIsVehicle)
				{
                    return this.startingPosition;
                }
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
            pos = AdjustPos(currentPos, startingPosition, pos);

            var distance = Vector3.Distance(startingPosition, currentPos) * def.totalSizeScale;
            var distanceToTarget = Vector3.Distance(startingPosition, destination);
            var widthFactor = distance / distanceToTarget;

            var vec = new Vector3(distance * def.widthScaleFactor * widthFactor, 0f, distance * def.heightScaleFactor);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(pos, quat, vec);
            Graphics.DrawMesh(MeshPool.plane10, matrix, ProjectileMat, 0);
        }

        private Vector3 AdjustPos(Vector3 currentPos, Vector3 startingPosition, Vector3 pos)
        {
            if (this.launcher is Pawn)
            {
                if (LauncherIsVehicle is false)
                {
                    if (this.launcher.Rotation == Rot4.West)
                    {
                        Vector3 startingPositionOffsetMirrored = this.def.startingPositionOffset;
                        startingPositionOffsetMirrored.x = -startingPositionOffsetMirrored.x;
                        pos += Quaternion.Euler(0, (startingPosition - currentPos).AngleFlat(), 0) * startingPositionOffsetMirrored;
                    }
                    else if (this.launcher.Rotation == Rot4.East)
                    {
                        pos += Quaternion.Euler(0, (startingPosition - currentPos).AngleFlat(), 0) * def.startingPositionOffset;
                    }

                    else if (this.launcher.Rotation == Rot4.South || this.launcher.Rotation == Rot4.North)
                    {
                        Vector3 startingPositionOffsetForSouth = this.def.startingPositionOffset;
                        startingPositionOffsetForSouth.x = 0;
                        pos += Quaternion.Euler(0, (startingPosition - currentPos).AngleFlat(), 0) * startingPositionOffsetForSouth;
                    }
                }
            }
            else
            {
                pos += Quaternion.Euler(0, (startingPosition - currentPos).AngleFlat(), 0) * def.startingPositionOffset;
            }

            return pos;
        }

        public HashSet<IntVec3> MakeProjectileLine(Vector3 start, Vector3 end, Map map)
        {
            var resultingLine = new ShootLine(start.ToIntVec3(), end.ToIntVec3());
            var points = resultingLine.Points();
            var currentPos = CurPosition;
            currentPos.y = 0;
            var startingPosition = StartingPosition;
            startingPosition.y = 0;
            Vector3 pos = (startingPosition + currentPos) / 2f;
            pos.y = 10;
			pos = AdjustPos(currentPos, startingPosition, pos);

            var distance = Vector3.Distance(startingPosition, currentPos) * this.def.totalSizeScale;
            var distanceToTarget = Vector3.Distance(startingPosition, currentPos);
            var widthFactor = distance / distanceToTarget;

            var width = distance * this.def.widthScaleFactor * widthFactor;
            var height = distance * this.def.heightScaleFactor;
            var centerOfLine = pos.ToIntVec3();
            var startPosition = StartingPosition.ToIntVec3();
            var endPosition = this.CurPosition.ToIntVec3();

            return GetCellsToDamage(start, points, width, height, centerOfLine, startPosition, endPosition);
        }

        protected virtual HashSet<IntVec3> GetCellsToDamage(Vector3 start, IEnumerable<IntVec3> points, float width, float height, IntVec3 centerOfLine, IntVec3 startPosition, IntVec3 endPosition)
        {
            HashSet<IntVec3> positions = new HashSet<IntVec3>();
            if (points.Any())
            {
                foreach (var cell in GenRadial.RadialCellsAround(start.ToIntVec3(), height, true))
                {
                    if (startPosition.DistanceTo(cell) > def.minDistanceToAffect)
                    {
                        var distanceFromStartToEnd = startPosition.DistanceToSquared(endPosition);
                        var distanceFromCellToEnd = cell.DistanceToSquared(endPosition);
                        int distanceFromCenterToEnd = centerOfLine.DistanceToSquared(endPosition);
                        if (def.wideAtStart ? distanceFromStartToEnd >= distanceFromCellToEnd : distanceFromCenterToEnd >= distanceFromCellToEnd)
                        {
                            var nearestCell = points.MinBy(x => x.DistanceToSquared(cell));
                            var widthToHeightRatio = (width / height);
                            if (widthToHeightRatio * def.arcSize > nearestCell.DistanceToSquared(cell))
                            {
                                positions.Add(cell);
                                if (def.debugMode)
                                {
                                    Map.debugDrawer.FlashCell(cell, 0.5f);
                                }
                            }
                        }
                    }
                }
                foreach (var cell in points)
                {
                    var startCellDistance = startPosition.DistanceTo(cell);
                    if (startCellDistance > def.minDistanceToAffect && startCellDistance <= startPosition.DistanceTo(endPosition))
                    {
                        if (def.debugMode)
                        {
                            Map.debugDrawer.FlashCell(cell, 0.5f);
                        }
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

        public override Vector3 ExactPosition
		{
			get
			{
				var value = base.ExactPosition;
				if (value.InBounds(Map) is false)
				{
                    var origin2 = new Vector3(this.origin.x, 0, this.origin.z);
                    var destination2 = new Vector3(value.x, 0, value.z);
                    var normalized = (destination2 - origin2).normalized;
                    var distanceDiff = 0.1f;
                    while (true)
                    {
                        var newValue = (value - (normalized * distanceDiff));
                        if (newValue.InBounds(Map))
                        {
							value = newValue;
                            break;
                        }
                        distanceDiff += 0.1f;
                    }
                }
                return value;
            }
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
				if (def.stopAtBuildingWithCover <= 0 || hitThing.def.fillPercent >= def.stopAtBuildingWithCover)
				{
                    StopMotion();
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
