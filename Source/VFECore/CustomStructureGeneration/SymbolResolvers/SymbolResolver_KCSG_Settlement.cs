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
			SettlementLayoutDef lDef = map.ParentFaction.def.GetModExtension<FactionSettlement>().temp;

			List<CellRect> gridRects = KCSG_Utilities.GetRects(rp.rect, lDef, map, out rp.rect);
			map.ParentFaction.def.GetModExtension<FactionSettlement>().tempRectList = gridRects;

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

			// Create the rooms
			ResolveParams rp2 = rp;
			rp2.faction = faction;
			BaseGen.symbolStack.Push("kcsg_roomsgen", rp2, null);

			// Add path
			if (lDef.path)
            {
				ResolveParams rp1 = rp;
				rp1.floorDef = lDef.pathType ?? TerrainDefOf.Gravel;
				rp1.allowBridgeOnAnyImpassableTerrain = true;
				BaseGen.symbolStack.Push("floor", rp1, null);
			}

			// Destroy all things before spawning the base
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
            
			/* int num = 0;
			if (rp.edgeDefenseWidth != null)
			{
				num = rp.edgeDefenseWidth.Value;
			}
			else if (rp.rect.Width >= 20 && rp.rect.Height >= 20 && (faction.def.techLevel >= TechLevel.Industrial || Rand.Bool))
			{
				num = (Rand.Bool ? 2 : 4);
			}
			float num2 = (float)rp.rect.Area / 144f * 0.17f;
			BaseGen.globalSettings.minEmptyNodes = ((num2 < 1f) ? 0 : GenMath.RoundRandom(num2));
			BaseGen.symbolStack.Push("outdoorLighting", rp, null);
			if (faction.def.techLevel >= TechLevel.Industrial)
			{
				int num3 = Rand.Chance(0.75f) ? GenMath.RoundRandom((float)rp.rect.Area / 400f) : 0;
				for (int i = 0; i < num3; i++)
				{
					ResolveParams resolveParams2 = rp;
					resolveParams2.faction = faction;
					BaseGen.symbolStack.Push("firefoamPopper", resolveParams2, null);
				}
			}
			if (num > 0)
			{
				ResolveParams rp3 = rp;
				rp3.faction = faction;
				rp3.edgeDefenseWidth = new int?(num);
				rp3.edgeThingMustReachMapEdge = new bool?(rp.edgeThingMustReachMapEdge ?? true);
				BaseGen.symbolStack.Push("edgeDefense", rp3, null);
			}
			ResolveParams resolveParams4 = rp;
			resolveParams4.rect = rp.rect.ContractedBy(num);
			resolveParams4.faction = faction;
			BaseGen.symbolStack.Push("ensureCanReachMapEdge", resolveParams4, null);
			ResolveParams resolveParams5 = rp;
			resolveParams5.rect = rp.rect.ContractedBy(num);
			resolveParams5.faction = faction;
			resolveParams5.floorOnlyIfTerrainSupports = new bool?(rp.floorOnlyIfTerrainSupports ?? true);
			BaseGen.symbolStack.Push("basePart_outdoors", resolveParams5, null);
			ResolveParams rp2 = rp;
			rp2.floorDef = TerrainDefOf.Bridge;
			rp2.floorOnlyIfTerrainSupports = new bool?(rp.floorOnlyIfTerrainSupports ?? true);
			rp2.allowBridgeOnAnyImpassableTerrain = new bool?(rp.allowBridgeOnAnyImpassableTerrain ?? true);
			BaseGen.symbolStack.Push("floor", rp2, null);
		}*/
		}
	}
}
