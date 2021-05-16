using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace KCSG
{
    public class GenStep_EnnemiesPresence : GenStep
    {
		public Faction forcedfaction;
		public bool spawnOnEdge = false;

		public override int SeedPart
		{
			get
			{
				return 1466666193;
			}
		}

		public override void Generate(Map map, GenStepParams parms)
		{
			if (this.forcedfaction != null) parms.sitePart.site.SetFaction(forcedfaction); 

			int h = 10, w = 10;
			
			List<Pawn> list = new List<Pawn>();
			foreach (Pawn pawn in this.GeneratePawns(map, parms))
			{
				IntVec3 loc;
				if (this.spawnOnEdge)
				{
					if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => x.Standable(map) && !x.Fogged(map) && map.reachability.CanReachColony(x), map, CellFinder.EdgeRoadChance_Ignore, out loc))
					{
						Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
						break;
					}
				}
				else if (!SiteGenStepUtility.TryFindSpawnCellAroundOrNear(CellRect.CenteredOn(map.Center, w, h), map.Center, map, out loc))
				{
					Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
					break;
				}
				GenSpawn.Spawn(pawn, loc, map, WipeMode.Vanish);
				if (!this.spawnOnEdge)
				{
					for (int i = 0; i < 10; i++)
					{
						MoteMaker.ThrowAirPuffUp(pawn.DrawPos, map);
					}
				}
				list.Add(pawn);
			}
			if (!list.Any<Pawn>())
			{
				return;
			}
			Faction faction = list[0].Faction;
			LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, map.Center), map, list);
			if (!this.spawnOnEdge)
			{
				for (int k = 0; k < list.Count; k++)
				{
					list[k].jobs.StartJob(JobMaker.MakeJob(JobDefOf.Wait, 120, false), JobCondition.None, null, false, true, null, null, false, false);
					list[k].Rotation = Rot4.Random;
				}
			}
		}

		private IEnumerable<Pawn> GeneratePawns(Map map, GenStepParams parms)
        {
			Faction faction;
			faction = this.forcedfaction ?? (map.ParentFaction ?? Find.FactionManager.RandomEnemyFaction(false, false, false, TechLevel.Undefined));
			if (faction == null)
			{
				return Enumerable.Empty<Pawn>();
			}
			return PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
			{
				groupKind = PawnGroupKindDefOf.Combat,
				tile = map.Tile,
				faction = faction,
				points = StorytellerUtility.DefaultSiteThreatPointsNow()// Mathf.Max(parms.sitePart.parms.threatPoints, faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat))
			}, true);
		}

		public IntVec3 FindRect(Map map, int height, int width)
		{
			CellRect rect;
			bool shre = true;
			while (shre)
			{
				rect = CellRect.CenteredOn(CellFinder.RandomNotEdgeCell(33, map), width, height);
				if (rect.Cells.ToList().Any(i => !i.Walkable(map) || !i.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Medium))) { }
				else return rect.CenterCell;
			}
			return IntVec3.Invalid;
		}
	}
}
