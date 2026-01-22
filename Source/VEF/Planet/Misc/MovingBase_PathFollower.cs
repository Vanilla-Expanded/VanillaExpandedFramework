using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace VEF.Planet
{
    public class MovingBase_PathFollower : IExposable
    {
        private MovingBase movingBase;
        private bool moving;

        private bool paused;

        public PlanetTile nextTile = PlanetTile.Invalid;

        public PlanetTile previousTileForDrawingIfInDoubt = PlanetTile.Invalid;

        public float nextTileCostLeft;

        public float nextTileCostTotal = 1f;

        private PlanetTile destTile;

        public WorldPath curPath;

        public PlanetTile lastPathedTargetTile;

        public const int MaxMoveTicks = 30000;

        private const int MaxCheckAheadNodes = 20;

        private const int MinCostWalk = 50;

        private const int MinCostAmble = 60;

        public const float DefaultPathCostToPayPerTick = 1f;

        public const int FinalNoRestPushMaxDurationTicks = 10000;

        public PlanetTile Destination => destTile;

        public bool Moving
        {
            get
            {
                if (moving)
                {
                    return movingBase.Spawned;
                }
                return false;
            }
        }

        public bool MovingNow
        {
            get
            {
                if (moving && !paused)
                {
                    return true;
                }
                return false;
            }
        }

        public bool Paused
        {
            get
            {
                if (Moving)
                {
                    return paused;
                }
                return false;
            }
            set
            {
                if (value != paused)
                {
                    if (!value)
                    {
                        paused = false;
                    }
                    else if (!Moving)
                    {
                        Log.Error("Tried to pause movingBase movement of " + movingBase.ToStringSafe() + " but it's not moving.");
                    }
                    else
                    {
                        paused = true;
                    }
                }
            }
        }

        public MovingBase_PathFollower(MovingBase movingBase)
        {
            this.movingBase = movingBase;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref moving, "moving", defaultValue: true);
            Scribe_Values.Look(ref paused, "paused", defaultValue: false);
            Scribe_Values.Look(ref nextTile, "nextTile", 0);
            Scribe_Values.Look(ref previousTileForDrawingIfInDoubt, "previousTileForDrawingIfInDoubt", 0);
            Scribe_Values.Look(ref nextTileCostLeft, "nextTileCostLeft", 0f);
            Scribe_Values.Look(ref nextTileCostTotal, "nextTileCostTotal", 0f);
            Scribe_Values.Look(ref destTile, "destTile", 0);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && Current.ProgramState != 0 && moving
                && !StartPath(destTile, repathImmediately: true, resetPauseStatus: false))
            {
                StopDead();
            }
        }

        private static readonly AccessTools.FieldRef<WorldPathPool, List<WorldPath>> pathsField = AccessTools.FieldRefAccess<WorldPathPool, List<WorldPath>>("paths");
        public bool StartPath(PlanetTile destTile, bool repathImmediately = false, bool resetPauseStatus = true)
        {
            var paths = pathsField(Find.WorldPathPool);
            paths.Clear();
            if (resetPauseStatus)
            {
                paused = false;
            }

            if (!IsPassable(movingBase.Tile) && !TryRecoverFromUnwalkablePosition())
            {
                return false;
            }
            if (moving && curPath != null && this.destTile == destTile)
            {
                return true;
            }
            if (!movingBase.CanReach(destTile))
            {
                PatherFailed();
                return false;
            }
            this.destTile = destTile;
            if (!nextTile.Valid || !IsNextTilePassable())
            {
                nextTile = movingBase.Tile;
                nextTileCostLeft = 0f;
                previousTileForDrawingIfInDoubt = PlanetTile.Invalid;
            }
            if (AtDestinationPosition())
            {
                PatherArrived();
                return true;
            }
            if (curPath != null)
            {
                curPath.ReleaseToPool();
            }
            curPath = null;
            moving = true;
            if (repathImmediately && TrySetNewPath() && nextTileCostLeft <= 0f && moving)
            {
                TryEnterNextPathTile();
            }
            return true;
        }

        public void StopDead()
        {
            if (curPath != null)
            {
                curPath.ReleaseToPool();
            }
            curPath = null;
            moving = false;
            paused = false;
            nextTile = movingBase.Tile;
            previousTileForDrawingIfInDoubt = PlanetTile.Invalid;
            nextTileCostLeft = 0f;
        }

        public void PatherTick(int delta)
        {
            if (!paused)
            {
                if (nextTileCostLeft > 0f)
                {
                    nextTileCostLeft -= CostToPayThisTick(delta);
                }
                else if (moving)
                {
                    TryEnterNextPathTile();
                }
            }
        }

        public void Notify_Teleported_Int()
        {
            StopDead();
        }

        private bool IsPassable(PlanetTile tile)
        {
            return !Find.World.Impassable(tile);
        }

        public bool IsNextTilePassable()
        {
            return IsPassable(nextTile);
        }

        private bool TryRecoverFromUnwalkablePosition()
        {
            if (GenWorldClosest.TryFindClosestTile(movingBase.Tile, (PlanetTile t) => IsPassable(t), out var foundTile))
            {
                Log.Warning(string.Concat(movingBase, " on unwalkable tile ", movingBase.Tile, ". Teleporting to ", foundTile));
                movingBase.Tile = foundTile;
                return true;
            }
            Log.Error(string.Concat(movingBase, " on unwalkable tile ", movingBase.Tile, ". Could not find walkable position nearby. Removed."));
            movingBase.Destroy();
            return false;
        }

        private void PatherArrived()
        {
            StopDead();
        }

        private void PatherFailed()
        {
            StopDead();
        }

        private void TryEnterNextPathTile()
        {
            if (!IsNextTilePassable())
            {
                PatherFailed();
                return;
            }
            movingBase.Tile = nextTile;
            if (!NeedNewPath() || TrySetNewPath())
            {
                if (AtDestinationPosition())
                {
                    PatherArrived();
                }
                else if (curPath.NodesLeftCount == 0)
                {
                    Log.Error(string.Concat(movingBase, " ran out of path nodes. Force-arriving."));
                    PatherArrived();
                }
                else
                {
                    SetupMoveIntoNextTile();
                }
            }
        }

        private void SetupMoveIntoNextTile()
        {
            if (curPath.NodesLeftCount < 2)
            {
                Log.Error(string.Concat(movingBase, " at ", movingBase.Tile, " ran out of path nodes while pathing to ", destTile, "."));
                PatherFailed();
                return;
            }
            nextTile = curPath.ConsumeNextNode();
            previousTileForDrawingIfInDoubt = PlanetTile.Invalid;
            if (Find.World.Impassable(nextTile))
            {
                Log.Error(string.Concat(movingBase, " entering ", nextTile, " which is unwalkable."));
            }
            int num = CostToMove(movingBase.Tile, nextTile);
            nextTileCostTotal = num;
            nextTileCostLeft = num;
        }

        private int CostToMove(PlanetTile start, PlanetTile end)
        {
            return CostToMove(movingBase, start, end);
        }

        public static int CostToMove(MovingBase movingBase, PlanetTile start, PlanetTile end, int? ticksAbs = null)
        {
            return CostToMove(movingBase.TicksPerMove, start, end, ticksAbs, perceivedStatic: false, null,
                null, false);
        }

        public static int CostToMove(int movingBaseTicksPerMove, PlanetTile start, PlanetTile end, int? ticksAbs = null, bool perceivedStatic = false, StringBuilder explanation = null, string movingBaseTicksPerMoveExplanation = null, bool immobile = false)
        {
            if (start == end)
            {
                return 0;
            }
            if (explanation != null)
            {
                explanation.Append(movingBaseTicksPerMoveExplanation);
                explanation.AppendLine();
            }
            StringBuilder stringBuilder = ((explanation != null) ? new StringBuilder() : null);
            float num = ((!perceivedStatic || explanation != null) ? WorldPathGrid.CalculatedMovementDifficultyAt(end, perceivedStatic, ticksAbs, stringBuilder) : Find.WorldPathGrid.PerceivedMovementDifficultyAt(end));
            float roadMovementDifficultyMultiplier = Find.WorldGrid.GetRoadMovementDifficultyMultiplier(start, end, stringBuilder);
            if (explanation != null && !immobile)
            {
                explanation.AppendLine();
                explanation.Append("TileMovementDifficulty".Translate() + ":");
                explanation.AppendLine();
                explanation.Append(stringBuilder.ToString().Indented("  "));
                explanation.AppendLine();
                explanation.Append("  = " + (num * roadMovementDifficultyMultiplier).ToString("0.#"));
            }
            int value = (int)((float)movingBaseTicksPerMove * num * roadMovementDifficultyMultiplier);
            value = Mathf.Clamp(value, 1, 30000);
            if (explanation != null)
            {
                explanation.AppendLine();
                if (immobile)
                {
                    explanation.Append("EncumberedMerchantGuildTilesPerDayTip".Translate());
                }
                else
                {
                    explanation.AppendLine();
                    explanation.Append("FinalMerchantGuildMovementSpeed".Translate() + ":");
                    int num2 = Mathf.CeilToInt((float)value / 1f);
                    explanation.AppendLine();
                    explanation.Append("  " + (60000f / (float)movingBaseTicksPerMove).ToString("0.#") + " / " + (num * roadMovementDifficultyMultiplier).ToString("0.#") + " = " + (60000f / (float)num2).ToString("0.#") + " " + "TilesPerDay".Translate());
                }
            }
            return value;
        }

        public static bool IsValidFinalPushDestination(PlanetTile tile)
        {
            List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
            for (int i = 0; i < allWorldObjects.Count; i++)
            {
                if (allWorldObjects[i].Tile == tile && allWorldObjects[i] is not MovingBase)
                {
                    return true;
                }
            }
            return false;
        }

        private float CostToPayThisTick(int delta)
        {
            // 1 cost per tick (implied) times delta.
            float num = delta;
            if (num < nextTileCostTotal / 30000f)
            {
                num = nextTileCostTotal / 30000f;
            }
            return num;
        }

        private bool TrySetNewPath()
        {
            WorldPath worldPath = GenerateNewPath();
            if (!worldPath.Found)
            {
                PatherFailed();
                return false;
            }
            if (curPath != null)
            {
                curPath.ReleaseToPool();
            }
            curPath = worldPath;
            return true;
        }

        private WorldPath GenerateNewPath()
        {
            pathsField(Find.WorldPathPool)?.Clear();
            var tile = ((moving && nextTile.Valid && IsNextTilePassable()) ? nextTile : movingBase.Tile);
            lastPathedTargetTile = destTile;
            WorldPath worldPath = movingBase.Tile.Layer.Pather.FindPath(tile, destTile, null);
            if (worldPath.Found && tile != movingBase.Tile)
            {
                if (worldPath.NodesLeftCount >= 2 && worldPath.Peek(1) == movingBase.Tile)
                {
                    worldPath.ConsumeNextNode();
                    if (moving)
                    {
                        previousTileForDrawingIfInDoubt = nextTile;
                        nextTile = movingBase.Tile;
                        nextTileCostLeft = nextTileCostTotal - nextTileCostLeft;
                    }
                }
                else
                {
                    worldPath.AddNodeAtStart(movingBase.Tile);
                }
            }
            return worldPath;
        }

        private bool AtDestinationPosition()
        {
            return movingBase.Tile == destTile;
        }

        private bool NeedNewPath()
        {
            if (!moving)
            {
                return false;
            }
            if (curPath == null || !curPath.Found || curPath.NodesLeftCount == 0)
            {
                return true;
            }
            for (int i = 0; i < 20 && i < curPath.NodesLeftCount; i++)
            {
                var tile = curPath.Peek(i);
                if (Find.World.Impassable(tile))
                {
                    return true;
                }
            }
            return false;
        }
    }
}