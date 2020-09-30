using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using Verse;
using Verse.AI.Group;
using RimWorld;
using RimWorld.BaseGen;

namespace KCSG
{
	class SymbolResolver_KCSG_Settlement : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			Faction faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);
			SettlementLayoutDef lDef = FactionSettlement.temp;

			List<CellRect> gridRects = KCSG_Utilities.GetRects(rp.rect, lDef, map, out rp.rect);
			FactionSettlement.tempRectList = gridRects;

			if (KCSG_Mod.settings.enableLog) Log.Message("Hostile pawns generation - PASS");
			
			// Add pawn to the base
			Lord singlePawnLord = rp.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, rp.rect.CenterCell), map, null);
			TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false);
			ResolveParams resolveParams = rp;
			resolveParams.rect = rp.rect;
			resolveParams.faction = faction;
			resolveParams.singlePawnLord = singlePawnLord;
			resolveParams.pawnGroupKindDef = (rp.pawnGroupKindDef ?? PawnGroupKindDefOf.Settlement);
			resolveParams.singlePawnSpawnCellExtraPredicate = (rp.singlePawnSpawnCellExtraPredicate ?? ((IntVec3 x) => map.reachability.CanReachMapEdge(x, traverseParms)));
			if (resolveParams.pawnGroupMakerParams == null)
			{
				resolveParams.pawnGroupMakerParams = new PawnGroupMakerParms();
				resolveParams.pawnGroupMakerParams.tile = map.Tile;
				resolveParams.pawnGroupMakerParams.faction = faction;
				resolveParams.pawnGroupMakerParams.points = (rp.settlementPawnGroupPoints ?? SymbolResolver_Settlement.DefaultPawnsPoints.RandomInRange);
				resolveParams.pawnGroupMakerParams.inhabitants = true;
				resolveParams.pawnGroupMakerParams.seed = rp.settlementPawnGroupSeed;
			}
			BaseGen.symbolStack.Push("pawnGroup", resolveParams, null);

			// Add defense
			if (lDef.vanillaLikeDefense)
            {
				int dWidth = (Rand.Bool ? 2 : 4);
				ResolveParams rp3 = rp;
				rp3.rect = new CellRect(rp.rect.minX - dWidth, rp.rect.minZ - dWidth, rp.rect.Width + (dWidth * 2), rp.rect.Height + (dWidth * 2));
				rp3.faction = faction;
				rp3.edgeDefenseWidth = dWidth;
				rp3.edgeThingMustReachMapEdge = new bool?(rp.edgeThingMustReachMapEdge ?? true);
				BaseGen.symbolStack.Push("edgeDefense", rp3, null);
			}

			if (KCSG_Mod.settings.enableLog) Log.Message("Structures generation - PASS");

			// Create the rooms
			ResolveParams rp2 = rp;
			rp2.faction = faction;
			BaseGen.symbolStack.Push("kcsg_roomsgen", rp2, null);

			// Add path

			if (KCSG_Mod.settings.enableLog) Log.Message("Adding paths - PASS");

			if (lDef.path)
            {
				ResolveParams rp1 = rp;
				rp1.floorDef = lDef.pathType ?? TerrainDefOf.Gravel;
				rp1.allowBridgeOnAnyImpassableTerrain = true;
				BaseGen.symbolStack.Push("floor", rp1, null);
			}

			// Destroy all things before spawning the base
			
			if (KCSG_Mod.settings.enableLog) Log.Message("Clearing ground - PASS");

			if (lDef.clearEverything)
            {
				foreach (IntVec3 c in rp.rect)
				{
					c.GetThingList(map).ToList().ForEach((t) => t.DeSpawn()); // Remove all things
					map.roofGrid.SetRoof(c, null); // Remove roof
				}
				map.roofGrid.RoofGridUpdate(); // Update roof grid
			}
			else
            {
				foreach (IntVec3 c in rp.rect)
				{
					c.GetThingList(map).ToList().FindAll(t1 => t1.def.category == ThingCategory.Filth || t1.def.category == ThingCategory.Item).ForEach((t) => t.DeSpawn());
				}
			}
		}
	}
}
