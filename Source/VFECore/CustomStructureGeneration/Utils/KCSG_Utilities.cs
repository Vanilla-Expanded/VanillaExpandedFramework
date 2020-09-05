using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace KCSG
{
    public class KCSG_Utilities
    {
        #region Rect Utils
        public static List<CellRect> GetRects(CellRect fullSize, SettlementLayoutDef lDef, Map map, out CellRect afterpath)
		{
			List<CellRect> rects = new List<CellRect>();
			int nWidth = fullSize.Width / lDef.gridSize.x;
			int nHeight = fullSize.Height / lDef.gridSize.z;
#if DEBUG
			Log.Message("Number of grid/building: widht: " + nWidth.ToString() + " , height: " + nHeight.ToString());
#endif
			// Calculate new fullSize depending on path
			if (!lDef.path) afterpath = fullSize; // No path return normal size
			else
			{
				int newWidth = fullSize.Width + ((nWidth - 1) * lDef.pathWidth);
				int newHeight = fullSize.Height + ((nHeight - 1) * lDef.pathWidth);
#if DEBUG
				Log.Message("New dimension: widht: " + newWidth.ToString() + " , height: " + newHeight.ToString());
				Log.Message("Old dimension: widht: " + fullSize.Width.ToString() + " , height: " + fullSize.Height.ToString());
#endif
				afterpath = new CellRect(fullSize.CenterCell.x - newWidth / 2, fullSize.CenterCell.z - newHeight / 2, newWidth, newHeight);
			}

			// Grid rects creation
			IntVec3 firstCenter = new IntVec3(afterpath.First().x + (lDef.gridSize.x / 2), afterpath.First().y, afterpath.First().z + (lDef.gridSize.z / 2));
			CellRect rect1 = new CellRect(firstCenter.x - lDef.gridSize.x / 2, firstCenter.z - lDef.gridSize.z / 2, lDef.gridSize.x, lDef.gridSize.z);
			rects.Add(rect1);

			IntVec3 temp = firstCenter;
			for (int i = 0; i < nHeight; i++)
			{
				for (int i2 = 0; i2 < nWidth; i2++)
				{
					temp.x += lDef.gridSize.x;
					if (lDef.path) temp.x += lDef.pathWidth;
					if (afterpath.Contains(temp))
					{
						CellRect rect = new CellRect(temp.x - lDef.gridSize.x / 2, temp.z - lDef.gridSize.z / 2, lDef.gridSize.x, lDef.gridSize.z);
						rects.Add(rect);
					}
				}
				temp.x = firstCenter.x - lDef.gridSize.x - lDef.pathWidth;
				if (lDef.path) temp.z += lDef.pathWidth;
				temp.z += lDef.gridSize.z;
			}
			return rects;
		}

		public static void MinMaxXZ(List<IntVec3> list, out int zMin, out int zMax, out int xMin, out int xMax)
		{
			zMin = list[0].z;
			zMax = 0;
			xMin = list[0].x;
			xMax = 0;
			foreach (IntVec3 c in list)
			{
				if (c.z < zMin) zMin = c.z;
				if (c.z > zMax) zMax = c.z;
				if (c.x < xMin) xMin = c.x;
				if (c.x > xMax) xMax = c.x;
			}
#if DEBUG
			Log.Message("xMin" + xMin.ToString());
			Log.Message("xMax" + xMax.ToString());
			Log.Message("zMin" + zMin.ToString());
			Log.Message("zMax" + zMax.ToString());
#endif
		}

		public static void HeightWidthFromLayout(StructureLayoutDef structureLayoutDef, out int height, out int width)
        {
			height = structureLayoutDef.layouts[0].Count;
			width = structureLayoutDef.layouts[0][0].Split(',').ToList().Count;
		}

        #endregion

        #region Gen Utils

        public static void GenerateRoomFromLayout(List<string> layoutList, CellRect roomRect, Map map, StructureLayoutDef rld)
		{
			List<string> allSymbList = new List<string>();

			foreach (string str in layoutList)
			{
				List<string> symbSplitFromLine = str.Split(',').ToList();
				symbSplitFromLine.ForEach((s) => allSymbList.Add(s));
			}

			List<string> roofGrid = new List<string>();
			if (rld.roofGrid != null)
            {
				foreach (string str in rld.roofGrid)
				{
					List<string> tempSplitFromLine = str.Split(',').ToList();
					tempSplitFromLine.ForEach((s) => roofGrid.Add(s));
				}
			}			

			int l = 0;
			foreach (IntVec3 cell in roomRect)
			{
				if (l < allSymbList.Count && allSymbList[l] != ".")
				{
					SymbolDef temp;
					MapComponent_KSG.pairsSymbolLabel.TryGetValue(allSymbList[l], out temp);
					Thing thing;
					if (temp != null)
					{
						if (temp.isTerrain && temp.terrainDef != null)
						{
							map.terrainGrid.SetTerrain(cell, temp.terrainDef);
							cell.GetThingList(map).ToList().FindAll(t1 => t1.def.category == ThingCategory.Building && !t1.def.BuildableByPlayer).ForEach((t) => t.DeSpawn());
						}
						else if (temp.isPawn && temp.pawnKindDefNS != null)
                        {
							if (temp.lordJob != null)
                            {
								Lord lord = KCSG_Utilities.CreateNewLord(temp.lordJob, map, cell);
								for (int i = 0; i < temp.numberToSpawn; i++)
								{
									Pawn pawn;
									if (temp.spawnPartOfFaction) pawn = PawnGenerator.GeneratePawn(temp.pawnKindDefNS, map.ParentFaction);
									else pawn = PawnGenerator.GeneratePawn(temp.pawnKindDefNS);
									if (temp.isSlave) pawn.guest.SetGuestStatus(map.ParentFaction, true);

									GenSpawn.Spawn(pawn, cell, map);
									lord.AddPawn(pawn);
								}
							}
							else
                            {
								for (int i = 0; i < temp.numberToSpawn; i++)
								{
									Pawn pawn;
									if (temp.spawnPartOfFaction) pawn = PawnGenerator.GeneratePawn(temp.pawnKindDefNS, map.ParentFaction);
									else pawn = PawnGenerator.GeneratePawn(temp.pawnKindDefNS);

									if (temp.isSlave) pawn.guest.SetGuestStatus(map.ParentFaction, true);
									GenSpawn.Spawn(pawn, cell, map);
								}
							}
						}
						else if (temp.isItem && temp.thingDef != null)
                        {
							if (temp.thingDef.stuffCategories != null) thing = ThingMaker.MakeThing(temp.thingDef, GenStuff.RandomStuffFor(temp.thingDef));
							else thing = ThingMaker.MakeThing(temp.thingDef);

							if (temp.thingDef.stackLimit > 1) thing.stackCount = temp.stackCount.RandomInRange;
							if (thing.TryGetComp<CompQuality>() != null) thing.TryGetComp<CompQuality>().SetQuality(QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Outsider);

							GenSpawn.Spawn(thing, cell, map);
							if (thing.TryGetComp<CompForbiddable>() != null) thing.SetForbidden(true);
						}
						else if (temp.thingDef != null)
						{
							thing = ThingMaker.MakeThing(temp.thingDef, temp.stuffDef);
							// If the thing is refulable, fill it
							if (thing.TryGetComp<CompRefuelable>() != null) thing.TryGetComp<CompRefuelable>().Refuel((int)thing.TryGetComp<CompRefuelable>().Props.fuelCapacity / 2);
							// If it's a grave, fill it
							if (thing is Building_Casket inheritFromCasket)
							{
								Pawn pawn;
								if (temp.containPawnKind != null) pawn = PawnGenerator.GeneratePawn(temp.containPawnKindDef, map.ParentFaction);
								else pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Villager, map.ParentFaction);
								inheritFromCasket.TryAcceptThing(pawn);
							}

							if (cell.GetFirstMineable(map) != null && (thing.def.defName == "Barricade" || thing.def.defName == "Sandbags")) { }
							else if (thing.def.rotatable && thing.def.category == ThingCategory.Building)
                            {
								if (cell.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Bridgeable)) map.terrainGrid.SetTerrain(cell, TerrainDefOf.Bridge);
								GenSpawn.Spawn(thing, cell, map, new Rot4(temp.rotation.AsInt));
								thing.SetFactionDirect(map.ParentFaction);
							}
							else if (thing.def.category == ThingCategory.Plant && cell.GetThingList(map).FindAll(th => th.def.passability == Traversability.Impassable).Count == 0) // && cell.GetFirstThing<Thing>(map).def.passability != Traversability.Impassable) // If it's a plant
							{
								if (cell.GetTerrain(map).fertility <= 0) map.terrainGrid.SetTerrain(cell, TerrainDefOf.Soil);
								Plant plant = thing as Plant;
								plant.Growth = temp.plantGrowth; // apply the growth
								GenSpawn.Spawn(plant, cell, map, WipeMode.Vanish);
							}
							else if (thing.def.category == ThingCategory.Building) 
							{
								// cell.GetThingList(map).ToList().ForEach(
								if (cell.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Bridgeable)) map.terrainGrid.SetTerrain(cell, TerrainDefOf.Bridge);
								GenSpawn.Spawn(thing, cell, map, WipeMode.Vanish);
								thing.SetFactionDirect(map.ParentFaction);
							}

							if ((thing.def.passability == Traversability.Impassable || thing.def.altitudeLayer == AltitudeLayer.DoorMoveable) && map.ParentFaction != null && map.ParentFaction.def.techLevel >= TechLevel.Industrial) // Add power cable under all impassable
                            {
								if (LoadedModManager.RunningMods.ToList().FindAll(m => m.Name == "Subsurface Conduit").Count > 0) GenSpawn.Spawn(DefDatabase<ThingDef>.AllDefsListForReading.FindAll(d => d.defName == "MUR_SubsurfaceConduit").First(), cell, map, WipeMode.Vanish);
								else GenSpawn.Spawn(ThingDefOf.PowerConduit, cell, map, WipeMode.Vanish);
							}
						}
					}
					else
                    {
						Log.Error("No symbolDef found for symbol " + allSymbList[l]);
                    }					
				}

				if (rld.roofGrid != null && l < roofGrid.Count && roofGrid[l] == "1")
				{
					map.roofGrid.SetRoof(cell, RoofDefOf.RoofConstructed);
				}
				else if (rld.roofOver)
                {
					map.roofGrid.SetRoof(cell, RoofDefOf.RoofConstructed);
					if (cell.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Bridgeable)) map.terrainGrid.SetTerrain(cell, TerrainDefOf.Bridge);
				}
				l++;
			}
		}

		public static void GenerateRoofFromLayout(CellRect roomRect, Map map, StructureLayoutDef rld)
		{
			List<IntVec3> cellExort = roomRect.Cells.ToList();
			cellExort.Sort((x, y) => x.z.CompareTo(y.z));
			IntVec3 first = roomRect.First();

			for (int i = 0; i < roomRect.Height; i++)
			{
				List<string> tempList = rld.terrainGrid[i].Split(',').ToList();
				for (int i2 = 0; i2 < roomRect.Width; i2++)
				{
					if (tempList[i2] == "1")
                    {
						map.roofGrid.SetRoof(first, RoofDefOf.RoofConstructed);
					}
					first.x++;
				}
				first.x -= roomRect.Width;
				first.z++;
			}
		}

		public static void GenerateTerrainFromLayout(CellRect roomRect, Map map, StructureLayoutDef rld)
		{
			List<IntVec3> cellExort = roomRect.Cells.ToList();
			cellExort.Sort((x, y) => x.z.CompareTo(y.z));
			IntVec3 first = roomRect.First();

			for (int i = 0; i < roomRect.Height; i++)
            {
				List<string> tempList = rld.terrainGrid[i].Split(',').ToList();
                for (int i2 = 0; i2 < roomRect.Width; i2++)
                {
					if (i2 < tempList.Count)
                    {
						SymbolDef temp;
						MapComponent_KSG.pairsSymbolLabel.TryGetValue(tempList[i2], out temp);
						if (temp != null)
						{
							if (map.terrainGrid.TerrainAt(first).affordances.Contains(TerrainAffordanceDefOf.Bridgeable)) map.terrainGrid.SetTerrain(first, TerrainDefOf.Bridge);
							else if (temp.terrainDef != null) map.terrainGrid.SetTerrain(first, temp.terrainDef);

							first.GetThingList(map).ToList().FindAll(t1 => (t1.def.category == ThingCategory.Building && (!t1.def.BuildableByPlayer || t1.Faction == null)) || t1.def.mineable).ForEach((t) => t.DeSpawn());
						}
					}
					first.x++;
				}
				first.x -= roomRect.Width;
				first.z++;
			}
		}

		public static void FillStockpileRoom(StructureLayoutDef rld, CellRect roomRect, Map map)
        {
            foreach (IntVec3 i in roomRect.CenterCell.GetRoom(map).Cells)
            {
				int x = (Rand.Bool ? 1 : 2);
				if (x == 1 && i.GetThingList(map).Count == 0)
                {
					ThingDef randTD = rld.allowedThingsInStockpile.RandomElement();
					ThingDef randStuff = null;
					if (randTD.stuffCategories != null)
                    {
						randStuff = DefDatabase<ThingDef>.AllDefsListForReading.FindAll((t) => t.IsStuff).RandomElement();
                    }
					Thing thing = ThingMaker.MakeThing(randTD, randStuff);
					if (thing.def.stackLimit > 1) thing.stackCount = Rand.RangeInclusive(5, thing.def.stackLimit);
					thing.SetForbidden(true);
					GenSpawn.Spawn(thing, i, map);
                }

			}
        }

		public static Lord CreateNewLord(Type lordJobType, Map map, IntVec3 cell)
		{
			return LordMaker.MakeNewLord(map.ParentFaction, Activator.CreateInstance(lordJobType, new object[]
			{
				new SpawnedPawnParams
				{
					defSpot = cell,
				}
			}) as LordJob, map, null);
		}

		// Vanilla function, remade to be able to use subsurface conduit when mod loaded
		public static void EnsureBatteriesConnectedAndMakeSense(Map map, List<Thing> tmpThings, Dictionary<PowerNet, bool> tmpPowerNetPredicateResults, List<IntVec3> tmpCells, ThingDef conduit)
		{
			tmpThings.Clear();
			tmpThings.AddRange(map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial));
			for (int i = 0; i < tmpThings.Count; i++)
			{
				CompPowerBattery compPowerBattery = tmpThings[i].TryGetComp<CompPowerBattery>();
				if (compPowerBattery != null)
				{
					PowerNet powerNet = compPowerBattery.PowerNet;
					if (powerNet == null)
					{
						map.powerNetManager.UpdatePowerNetsAndConnections_First();
						PowerNet powerNet2;
						IntVec3 dest;
						if (KCSG_Utilities.TryFindClosestReachableNet(compPowerBattery.parent.Position, (PowerNet x) => KCSG_Utilities.HasAnyPowerGenerator(x), map, out powerNet2, out dest, tmpPowerNetPredicateResults))
						{
							map.floodFiller.ReconstructLastFloodFillPath(dest, tmpCells);
							if (powerNet2 != null)
							{
								KCSG_Utilities.SpawnTransmitters(tmpCells, map, compPowerBattery.parent.Faction, conduit);
							}
						}
					}
				}
			}
		}

		public static void EnsurePowerUsersConnected(Map map, List<Thing> tmpThings, Dictionary<PowerNet, bool> tmpPowerNetPredicateResults, List<IntVec3> tmpCells, ThingDef conduit)
		{
			tmpThings.Clear();
			tmpThings.AddRange(map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial));
			bool hasAtleast1TurretInt = tmpThings.Any((Thing t) => t is Building_Turret);
			for (int i = 0; i < tmpThings.Count; i++)
			{
				if (KCSG_Utilities.IsPowerUser(tmpThings[i]))
				{
					CompPowerTrader powerComp = tmpThings[i].TryGetComp<CompPowerTrader>();
					PowerNet powerNet = powerComp.PowerNet;
					if (powerNet != null && powerNet.hasPowerSource)
					{
						KCSG_Utilities.TryTurnOnImmediately(powerComp, map);
					}
					else
					{
						map.powerNetManager.UpdatePowerNetsAndConnections_First();
						PowerNet powerNet2;
						IntVec3 dest;
						if (KCSG_Utilities.TryFindClosestReachableNet(powerComp.parent.Position, (PowerNet x) => x.CurrentEnergyGainRate() - powerComp.Props.basePowerConsumption * CompPower.WattsToWattDaysPerTick > 1E-07f, map, out powerNet2, out dest, tmpPowerNetPredicateResults))
						{
							map.floodFiller.ReconstructLastFloodFillPath(dest, tmpCells);
							KCSG_Utilities.SpawnTransmitters(tmpCells, map, tmpThings[i].Faction, conduit);
							KCSG_Utilities.TryTurnOnImmediately(powerComp, map);
						}
						else if (KCSG_Utilities.TryFindClosestReachableNet(powerComp.parent.Position, (PowerNet x) => x.CurrentStoredEnergy() > 1E-07f, map, out powerNet2, out dest, tmpPowerNetPredicateResults))
						{
							map.floodFiller.ReconstructLastFloodFillPath(dest, tmpCells);
							KCSG_Utilities.SpawnTransmitters(tmpCells, map, tmpThings[i].Faction, conduit);
						}
					}
				}
			}
		}

		public static void EnsureGeneratorsConnectedAndMakeSense(Map map, List<Thing> tmpThings, Dictionary<PowerNet, bool> tmpPowerNetPredicateResults, List<IntVec3> tmpCells, ThingDef conduit)
		{
			tmpThings.Clear();
			tmpThings.AddRange(map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial));
			for (int i = 0; i < tmpThings.Count; i++)
			{
				if (KCSG_Utilities.IsPowerGenerator(tmpThings[i]))
				{
					PowerNet powerNet = tmpThings[i].TryGetComp<CompPower>().PowerNet;
					if (powerNet == null || !KCSG_Utilities.HasAnyPowerUser(powerNet))
					{
						map.powerNetManager.UpdatePowerNetsAndConnections_First();
						PowerNet powerNet2;
						IntVec3 dest;
						if (KCSG_Utilities.TryFindClosestReachableNet(tmpThings[i].Position, (PowerNet x) => KCSG_Utilities.HasAnyPowerUser(x), map, out powerNet2, out dest, tmpPowerNetPredicateResults))
						{
							map.floodFiller.ReconstructLastFloodFillPath(dest, tmpCells);
							KCSG_Utilities.SpawnTransmitters(tmpCells, map, tmpThings[i].Faction, conduit);
						}
					}
				}
			}
		}

		public static bool HasAnyPowerUser(PowerNet net)
		{
			List<CompPowerTrader> powerComps = net.powerComps;
			for (int i = 0; i < powerComps.Count; i++)
			{
				if (KCSG_Utilities.IsPowerUser(powerComps[i].parent))
				{
					return true;
				}
			}
			return false;
		}

		public static void TryTurnOnImmediately(CompPowerTrader powerComp, Map map)
		{
			if (powerComp.PowerOn)
			{
				return;
			}
			map.powerNetManager.UpdatePowerNetsAndConnections_First();
			if (powerComp.PowerNet != null && powerComp.PowerNet.CurrentEnergyGainRate() > 1E-07f)
			{
				powerComp.PowerOn = true;
			}
		}

		public static bool IsPowerUser(Thing thing)
		{
			CompPowerTrader compPowerTrader = thing.TryGetComp<CompPowerTrader>();
			return compPowerTrader != null && (compPowerTrader.PowerOutput < 0f || (!compPowerTrader.PowerOn && compPowerTrader.Props.basePowerConsumption > 0f));
		}

		public static void SpawnTransmitters(List<IntVec3> cells, Map map, Faction faction, ThingDef conduit)
		{
			for (int i = 0; i < cells.Count; i++)
			{
				if (cells[i].GetTransmitter(map) == null)
				{
					GenSpawn.Spawn(conduit, cells[i], map, WipeMode.Vanish).SetFaction(faction, null);
				}
			}
		}

		public static bool EverPossibleToTransmitPowerAt(IntVec3 c, Map map)
		{
			return c.GetTransmitter(map) != null || GenConstruct.CanBuildOnTerrain(ThingDefOf.PowerConduit, c, map, Rot4.North, null, null);
		}

		public static bool IsPowerGenerator(Thing thing)
		{
			if (thing.TryGetComp<CompPowerPlant>() != null)
			{
				return true;
			}
			CompPowerTrader compPowerTrader = thing.TryGetComp<CompPowerTrader>();
			return compPowerTrader != null && (compPowerTrader.PowerOutput > 0f || (!compPowerTrader.PowerOn && compPowerTrader.Props.basePowerConsumption < 0f));
		}

		public static bool HasAnyPowerGenerator(PowerNet net)
		{
			List<CompPowerTrader> powerComps = net.powerComps;
			for (int i = 0; i < powerComps.Count; i++)
			{
				if (KCSG_Utilities.IsPowerGenerator(powerComps[i].parent))
				{
					return true;
				}
			}
			return false;
		}

		public static bool TryFindClosestReachableNet(IntVec3 root, Predicate<PowerNet> predicate, Map map, out PowerNet foundNet, out IntVec3 closestTransmitter, Dictionary<PowerNet, bool> tmpPowerNetPredicateResults)
		{
			tmpPowerNetPredicateResults.Clear();
			PowerNet foundNetLocal = null;
			IntVec3 closestTransmitterLocal = IntVec3.Invalid;
			map.floodFiller.FloodFill(root, (IntVec3 x) => KCSG_Utilities.EverPossibleToTransmitPowerAt(x, map), delegate (IntVec3 x)
			{
				Building transmitter = x.GetTransmitter(map);
				PowerNet powerNet = (transmitter != null) ? transmitter.GetComp<CompPower>().PowerNet : null;
				if (powerNet == null)
				{
					return false;
				}
				bool flag;
				if (!tmpPowerNetPredicateResults.TryGetValue(powerNet, out flag))
				{
					flag = predicate(powerNet);
					tmpPowerNetPredicateResults.Add(powerNet, flag);
				}
				if (flag)
				{
					foundNetLocal = powerNet;
					closestTransmitterLocal = x;
					return true;
				}
				return false;
			}, int.MaxValue, true, null);
			tmpPowerNetPredicateResults.Clear();
			if (foundNetLocal != null)
			{
				foundNet = foundNetLocal;
				closestTransmitter = closestTransmitterLocal;
				return true;
			}
			foundNet = null;
			closestTransmitter = IntVec3.Invalid;
			return false;
		}

        #endregion
    }
}
