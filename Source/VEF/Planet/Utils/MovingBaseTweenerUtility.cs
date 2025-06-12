using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VEF.Planet
{

    public static class MovingBaseTweenerUtility
    {
        private const float BaseRadius = 0.15f;

        private const float BaseDistToCollide = 0.2f;

        public static Vector3 PatherTweenedPosRoot(MovingBase movingBase)
        {
            WorldGrid worldGrid = Find.WorldGrid;
            if (!movingBase.Spawned)
            {
                return worldGrid.GetTileCenter(movingBase.Tile);
            }
            if (movingBase.pather.Moving)
            {
                float num = (movingBase.pather.IsNextTilePassable() ? (1f - movingBase.pather.nextTileCostLeft / movingBase.pather.nextTileCostTotal) : 0f);
                int tileID = ((movingBase.pather.nextTile != movingBase.Tile || movingBase.pather.previousTileForDrawingIfInDoubt == -1) ? movingBase.Tile : movingBase.pather.previousTileForDrawingIfInDoubt);
                return worldGrid.GetTileCenter(movingBase.pather.nextTile) * num + worldGrid.GetTileCenter(tileID) * (1f - num);
            }
            return worldGrid.GetTileCenter(movingBase.Tile);
        }

        public static Vector3 MovingBaseCollisionPosOffsetFor(MovingBase movingBase)
        {
            if (!movingBase.Spawned)
            {
                return Vector3.zero;
            }
            bool flag = movingBase.Spawned && movingBase.pather.Moving;
            float num = 0.15f * Find.WorldGrid.AverageTileSize;
            if (!flag || movingBase.pather.nextTile == movingBase.pather.Destination)
            {
                int num2 = ((!flag) ? movingBase.Tile : movingBase.pather.nextTile);
                int movingBasesCount = 0;
                int movingBasesWithLowerIdCount = 0;
                GetMovingBasesStandingAtOrAboutToStandAt(num2, out movingBasesCount, out movingBasesWithLowerIdCount, movingBase);
                if (movingBasesCount == 0)
                {
                    return Vector3.zero;
                }
                return WorldRendererUtility.ProjectOnQuadTangentialToPlanet(Find.WorldGrid.GetTileCenter(num2), GenGeo.RegularPolygonVertexPosition(movingBasesCount, movingBasesWithLowerIdCount) * num);
            }
            if (DrawPosCollides(movingBase))
            {
                Rand.PushState();
                Rand.Seed = movingBase.ID;
                float f = Rand.Range(0f, 360f);
                Rand.PopState();
                Vector2 point = new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * num;
                return WorldRendererUtility.ProjectOnQuadTangentialToPlanet(PatherTweenedPosRoot(movingBase), point);
            }
            return Vector3.zero;
        }

        private static void GetMovingBasesStandingAtOrAboutToStandAt(int tile, out int movingBasesCount, out int movingBasesWithLowerIdCount, MovingBase forMovingBase)
        {
            movingBasesCount = 0;
            movingBasesWithLowerIdCount = 0;
            List<MovingBase> movingBases = Find.WorldObjects.AllWorldObjects.OfType<MovingBase>().ToList();
            for (int i = 0; i < movingBases.Count; i++)
            {
                MovingBase movingBase = movingBases[i];
                if (movingBase.Tile != tile)
                {
                    if (!movingBase.pather.Moving || movingBase.pather.nextTile != movingBase.pather.Destination || movingBase.pather.Destination != tile)
                    {
                        continue;
                    }
                }
                else if (movingBase.pather.Moving)
                {
                    continue;
                }
                movingBasesCount++;
                if (movingBase.ID < forMovingBase.ID)
                {
                    movingBasesWithLowerIdCount++;
                }
            }
        }

        private static bool DrawPosCollides(MovingBase movingBase)
        {
            Vector3 a = PatherTweenedPosRoot(movingBase);
            float num = Find.WorldGrid.AverageTileSize * 0.2f;
            List<MovingBase> movingBases = Find.WorldObjects.AllWorldObjects.OfType<MovingBase>().ToList();
            for (int i = 0; i < movingBases.Count; i++)
            {
                MovingBase movingBase2 = movingBases[i];
                if (movingBase2 != movingBase && Vector3.Distance(a, PatherTweenedPosRoot(movingBase2)) < num)
                {
                    return true;
                }
            }
            return false;
        }
    }
}