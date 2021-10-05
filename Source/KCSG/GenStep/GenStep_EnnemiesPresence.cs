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
		public FactionDef forcedfaction;
		public float pointMultiplier = 1f;
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
			Faction fac = this.forcedfaction != null ? Find.FactionManager.FirstFactionOfDef(this.forcedfaction) : Find.FactionManager.RandomEnemyFaction(minTechLevel: TechLevel.Neolithic);
			parms.sitePart.site.SetFaction(fac);

			int h = 10, w = 10;
			
			List<Pawn> list = new List<Pawn>();
			foreach (Pawn pawn in this.GeneratePawns(map, fac))
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

		private IEnumerable<Pawn> GeneratePawns(Map map, Faction faction)
        {
			return PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
			{
				groupKind = PawnGroupKindDefOf.Combat,
				tile = map.Tile,
				faction = faction,
				points = StorytellerUtility.DefaultSiteThreatPointsNow() * this.pointMultiplier
			}, true);
		}
	}
}
