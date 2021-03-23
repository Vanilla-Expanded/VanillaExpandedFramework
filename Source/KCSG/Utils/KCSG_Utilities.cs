using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Verse;
using Verse.AI.Group;

namespace KCSG
{
    public class KCSG_Utilities
    {
        #region Rect Utils

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
/*#if DEBUG
			Log.Message("xMin" + xMin.ToString());
			Log.Message("xMax" + xMax.ToString());
			Log.Message("zMin" + zMin.ToString());
			Log.Message("zMax" + zMax.ToString());
#endif*/
        }

        public static void HeightWidthFromLayout(StructureLayoutDef structureLayoutDef, out int height, out int width)
        {
            if (structureLayoutDef == null || structureLayoutDef.layouts.Count == 0)
            {
                Log.Warning("structureLayoutDef was null. Throwing 10 10 size");
                height = 10;
                width = 10;
                return;
            }
            height = structureLayoutDef.layouts[0].Count;
            width = structureLayoutDef.layouts[0][0].Split(',').ToList().Count;
        }

        public static void EdgeFromList(List<IntVec3> cellExport, out int height, out int width)
        {
            height = 0;
            width = 0;
            IntVec3 first = cellExport.First();
            foreach (IntVec3 c in cellExport)
            {
                if (first.z == c.z) width++;
            }
            foreach (IntVec3 c in cellExport)
            {
                if (first.x == c.x) height++;
            }
/*#if DEBUG
            Log.Message("Export height: " + height.ToString() + " width: " + width.ToString());
#endif*/
        }

        public static void EdgeFromArea(List<IntVec3> cellExport, out int height, out int width)
        {
            height = 0;
            width = 0;
            IntVec3 first = cellExport.First();
            foreach (IntVec3 f in cellExport)
            {
                int tempW = 0, tempH = 0;
                foreach (IntVec3 c in cellExport)
                {
                    if (f.z == c.z) tempW++;
                }
                foreach (IntVec3 c in cellExport)
                {
                    if (f.x == c.x) tempH++;
                }
                if (tempW > width) width = tempW;
                if (tempH > height) height = tempH;
            }
#if DEBUG
            Log.Message("Export area height: " + height.ToString() + " width: " + width.ToString());
#endif
        }

        public static List<IntVec3> AreaToSquare(Area a, int height, int widht)
        {
            List<IntVec3> list = a.ActiveCells.ToList();
            int zMin, zMax, xMin, xMax;
            KCSG_Utilities.MinMaxXZ(list, out zMin, out zMax, out xMin, out xMax);

            List<IntVec3> listOut = new List<IntVec3>();

            for (int zI = zMin; zI <= zMax; zI++)
            {
                for (int xI = xMin; xI <= xMax; xI++)
                {
                    listOut.Add(new IntVec3(xI, 0, zI));
                }
            }
            listOut.Sort((x, y) => x.z.CompareTo(y.z));
            return listOut;
        }

        public static int GetMaxThingOnOneCell(List<IntVec3> cellExport, Map map, Dictionary<IntVec3, List<Thing>> pairsCellThingList)
        {
            int max = 1;
            foreach (var item in cellExport)
            {
                List<Thing> things = pairsCellThingList.TryGetValue(item);
                things.RemoveAll(t => t is Pawn || t.def.building == null || t.def.defName == "PowerConduit");
                if (things.Count > max) max = things.Count;
            }
            return max;
        }

        #endregion Rect Utils

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

            // Log.Message("-- Layout list creation - PASS");

            Dictionary<string, SymbolDef> pairsSymbolLabel = KCSG_Utilities.FillpairsSymbolLabel();

            int l = 0;
            foreach (IntVec3 cell in roomRect)
            {
                if (l < allSymbList.Count && allSymbList[l] != ".")
                {
                    SymbolDef temp;
                    pairsSymbolLabel.TryGetValue(allSymbList[l], out temp);
                    Thing thing;
                    if (temp != null)
                    {
                        if (temp.isTerrain && temp.terrainDef != null)
                        {
                            map.terrainGrid.SetTerrain(cell, temp.terrainDef);
                            cell.GetThingList(map).FindAll(t1 => t1.def.category == ThingCategory.Building && !t1.def.BuildableByPlayer).ForEach((t) => t.DeSpawn());
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

                            thing.stackCount = temp.stackCount.RandomInRange;
                            if (thing.TryGetComp<CompQuality>() != null) thing.TryGetComp<CompQuality>().SetQuality(QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Outsider);

                            if (cell.Walkable(map)) GenSpawn.Spawn(thing, cell, map);
                            if (thing.TryGetComp<CompForbiddable>() != null) thing.SetForbidden(true);
                        }
                        else if (temp.thingDef != null)
                        {
                            thing = ThingMaker.MakeThing(temp.thingDef, temp.stuffDef);
                            // If the thing is refulable, fill it
                            if (thing.TryGetComp<CompRefuelable>() != null) thing.TryGetComp<CompRefuelable>().Refuel((int)thing.TryGetComp<CompRefuelable>().Props.fuelCapacity / 2);
                            if (thing.TryGetComp<CompPowerBattery>() != null) thing.TryGetComp<CompPowerBattery>().AddEnergy(thing.TryGetComp<CompPowerBattery>().Props.storedEnergyMax);
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
                            else if (thing.def.category == ThingCategory.Plant && cell.GetThingList(map).FindAll(th => th.def.passability == Traversability.Impassable).Count == 0) // If it's a plant
                            {
                                if (rld.roofGrid != null && l < roofGrid.Count && (roofGrid[l] == "0" || roofGrid[l] == "."))
                                {
                                    if (cell.GetTerrain(map).fertility <= 0) map.terrainGrid.SetTerrain(cell, TerrainDefOf.Soil);
                                    Plant plant = thing as Plant;
                                    plant.Growth = temp.plantGrowth; // apply the growth
                                    GenSpawn.Spawn(plant, cell, map, WipeMode.Vanish);
                                }
                                else if (rld.roofGrid == null)
                                {
                                    if (cell.GetTerrain(map).fertility <= 0) map.terrainGrid.SetTerrain(cell, TerrainDefOf.Soil);
                                    Plant plant = thing as Plant;
                                    plant.Growth = temp.plantGrowth; // apply the growth
                                    GenSpawn.Spawn(plant, cell, map, WipeMode.Vanish);
                                }
                            }
                            else if (thing.def.category == ThingCategory.Building)
                            {
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
                        else
                        {
                            // Log.Message("--- Cell " + l.ToString() + " with SymbolDef " + allSymbList[l] + "(resolved to " + temp.defName + ") has nothing to place");
                        }
                    }
                }

                if (rld.roofGrid != null && l < roofGrid.Count && roofGrid[l] == "1")
                {
                    map.roofGrid.SetRoof(cell, RoofDefOf.RoofConstructed);
                }
                l++;
            }
            // Log.Message("-- Cells passage done - PASS");
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

        #region Power Function

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

        #endregion Power Function

        public static IntVec3 FindRect(Map map, int h, int w, bool nearCenter = false)
        {
            CellRect rect;
            int fromCenter = 1;
            while (true)
            {
                if (nearCenter)
                {
                    rect = CellRect.CenteredOn(CellFinder.RandomClosewalkCellNear(map.Center, map, fromCenter), w, h);
                    rect.ClipInsideMap(map);
                }
                else rect = CellRect.CenteredOn(CellFinder.RandomNotEdgeCell(h, map), w, h);

                if (rect.Cells.ToList().Any(i => !i.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Medium))) fromCenter += 2;
                else return rect.CenterCell;
            }
        }

        #endregion Gen Utils

        #region Symbol Creation

        public static void CreateSymbolFromThing(Thing thingT, string defnamePrefix, List<String> alreadyCreated, List<XElement> symbols)
        {
            XElement symbolDef = new XElement("KCSG.SymbolDef", null);
            // Generate defName
            string defNameString = defnamePrefix + "_" + thingT.def.defName;
            if (thingT.Stuff != null) defNameString += "_" + thingT.Stuff.defName;
            if (thingT.def.rotatable && thingT.def.category != ThingCategory.Plant) defNameString += "_" + thingT.Rotation.ToStringHuman();

            XElement defName = new XElement("defName", defNameString);
            symbolDef.Add(defName);
            // Add thing
            XElement thing = new XElement("thing", thingT.def.defName);
            symbolDef.Add(thing);
            // Add stuff
            if (thingT.Stuff != null)
            {
                XElement stuff = new XElement("stuff", thingT.Stuff.defName);
                symbolDef.Add(stuff);
            }
            // Add rotation
            if (thingT.def.rotatable && thingT.def.category != ThingCategory.Plant)
            {
                XElement rotation = new XElement("rotation", thingT.Rotation.ToStringHuman());
                symbolDef.Add(rotation);
            }
            // Plant growth
            if (thingT is Plant plant)
            {
                XElement plantGrowth = new XElement("plantGrowth", plant.Growth.ToString());
                symbolDef.Add(plantGrowth);
            }

            string symbolString = defnamePrefix + "_" + thingT.def.defName;
            if (thingT.Stuff != null) symbolString += "_" + thingT.Stuff.defName;
            if (thingT.def.rotatable && thingT.def.category != ThingCategory.Plant) symbolString += "_" + thingT.Rotation.ToStringHuman();
            XElement symbol = new XElement("symbol", symbolString);
            symbolDef.Add(symbol);

            // Log.Message("CreateSymbolFromThing: " + symbol.Value + "in symbols: " + alreadyCreated.Contains(symbolDef.Value));

            if (!alreadyCreated.Contains(symbolDef.Value)) symbols.Add(symbolDef); alreadyCreated.Add(symbolDef.Value);
        }

        public static void CreateSymbolFromTerrain(TerrainDef terrainD, string defnamePrefix, List<String> alreadyCreated, List<XElement> symbols)
        {
            XElement symbolDef = new XElement("KCSG.SymbolDef", null);
            // Generate defName
            XElement defName = new XElement("defName", defnamePrefix + "_" + terrainD.defName);
            symbolDef.Add(defName);
            // Add isTerrain
            XElement isTerrain = new XElement("isTerrain", "true");
            symbolDef.Add(isTerrain);
            // Add terrain
            XElement terrain = new XElement("terrain", terrainD.defName);
            symbolDef.Add(terrain);
            // Add symbol
            XElement symbol = new XElement("symbol", defnamePrefix + "_" + terrainD.defName);
            symbolDef.Add(symbol);

            if (!alreadyCreated.Contains(symbol.Value)) symbols.Add(symbolDef); alreadyCreated.Add(symbol.Value);
        }

        public static void CreateSymbolFromPawn(Pawn pawn, string defnamePrefix, List<String> alreadyCreated, List<XElement> symbols)
        {
            XElement symbolDef = new XElement("KCSG.SymbolDef", null);
            // Generate defName
            string defNameString = defnamePrefix + "_" + pawn.kindDef.defName;

            XElement defName = new XElement("defName", defNameString);
            symbolDef.Add(defName);
            // Add isPawn
            XElement isPawn = new XElement("isPawn", "true");
            symbolDef.Add(isPawn);
            // Add pawnKindDef
            XElement pawnKindDef = new XElement("pawnKindDef", pawn.kindDef.defName);
            symbolDef.Add(pawnKindDef);

            string symbolString = defnamePrefix + "_" + pawn.kindDef.defName;
            XElement symbol = new XElement("symbol", symbolString);
            symbolDef.Add(symbol);

            if (!alreadyCreated.Contains(symbol.Value)) symbols.Add(symbolDef); alreadyCreated.Add(symbol.Value);
        }

        public static void CreateItemSymbolFromThing(Thing thingT, string defnamePrefix, List<String> alreadyCreated, List<XElement> symbols)
        {
            XElement symbolDef = new XElement("KCSG.SymbolDef", null);
            // Generate defName
            string defNameString = defnamePrefix + "_Item_" + thingT.def.defName;
            XElement defName = new XElement("defName", defNameString);
            symbolDef.Add(defName);
            // Add thing
            XElement thing = new XElement("thing", thingT.def.defName);
            symbolDef.Add(thing);
            // Add identifier
            XElement isItem = new XElement("isItem", "true");
            symbolDef.Add(isItem);
            // Add stackSize
            IntRange intRange = new IntRange(1, thingT.def.stackLimit);
            XElement stackCount = new XElement("stackCount", intRange.ToString());
            symbolDef.Add(stackCount);
            // Add symbol
            string symbolString = defnamePrefix + "_Item_" + thingT.def.defName;
            XElement symbol = new XElement("symbol", symbolString);
            symbolDef.Add(symbol);

            if (!alreadyCreated.Contains(symbol.Value)) symbols.Add(symbolDef); alreadyCreated.Add(symbol.Value);
        }

        public static List<XElement> CreateSymbolIfNeeded(List<IntVec3> cellExport, Map map, string defnamePrefix, Dictionary<IntVec3, List<Thing>> pairsCellThingList, Area area = null)
        {
            List<string> justCreated = new List<string>();
            List<XElement> symbols = new List<XElement>();

            foreach (IntVec3 c in cellExport)
            {
                if (area != null && !area.ActiveCells.Contains(c)) { }
                else
                {
                    TerrainDef terrainDef = c.GetTerrain(map);
                    if (terrainDef != null && !KCSG_Utilities.AlreadyExist(null, terrainDef) && terrainDef.BuildableByPlayer) KCSG_Utilities.CreateSymbolFromTerrain(terrainDef, defnamePrefix, justCreated, symbols);

                    List<Thing> things = pairsCellThingList.TryGetValue(c);
                    foreach (Thing t in things)
                    {
                        if (t != null && !KCSG_Utilities.AlreadyExist(t, null))
                        {
                            if (t.def.category == ThingCategory.Item) KCSG_Utilities.CreateItemSymbolFromThing(t, defnamePrefix, justCreated, symbols);
                            if (t.def.category == ThingCategory.Pawn) KCSG_Utilities.CreateSymbolFromPawn(t as Pawn, defnamePrefix, justCreated, symbols);
                            if (t.def.category == ThingCategory.Building || t.def.category == ThingCategory.Plant)
                            {
                                KCSG_Utilities.CreateSymbolFromThing(t, defnamePrefix, justCreated, symbols);
                                // Log.Message("CreateSymbolIfNeeded: " + t.def.defName + " symbols count: " + symbols.Count);
                            }
                        }
                    }
                }
            }

            return symbols;
        }

        #endregion Symbol Creation

        #region Symbol Utilities

        public static bool AlreadyExist(Thing thing, TerrainDef terrain)
        {
            foreach (SymbolDef s in DefDatabase<SymbolDef>.AllDefsListForReading)
            {
                if (thing?.def.altitudeLayer == AltitudeLayer.Filth) return true;
                else if (s.isItem && s.thingDef.defName == thing?.def.defName) return true;
                else if (thing is Pawn p && s.pawnKindDefNS == p.kindDef) return true;
                else if (thing is Plant && s.thingDef == thing.def) return true;
                else if (thing != null)
                {
                    if (s.thingDef == thing.def && s.stuffDef == thing.Stuff)
                    {
                        if (!thing.def.rotatable) return true;
                        else if (thing.def.rotatable && s.rotation == thing.Rotation) return true;
                    }
                }
                else if (s.isTerrain && s.terrainDef == terrain) return true;
            }
            return false;
        }

        #endregion Symbol Utilities

        #region Layout Creation

        public static XElement CreateStructureDef(List<IntVec3> cellExport, Map map, string defNamePrefix, Dictionary<string, SymbolDef> pairsSymbolLabel, Dictionary<IntVec3, List<Thing>> pairsCellThingList, Area area = null)
        {
            cellExport.Sort((x, y) => x.z.CompareTo(y.z));
            XElement StructureLayoutDef = new XElement("KCSG.StructureLayoutDef", null);
            // Generate defName
            Room room = cellExport[cellExport.Count / 2].GetRoom(map);
            string defNameString = "PlaceHolder";
            XElement defName = new XElement("defName", defNameString);
            StructureLayoutDef.Add(defName);
            // Stockpile?
            bool isStockpileBool = false;
            if (map.zoneManager.ZoneAt(cellExport.FindAll(c => c.Walkable(map)).First()) is Zone_Stockpile)
            {
                isStockpileBool = true;
                XElement isStockpile = new XElement("isStockpile", isStockpileBool.ToString());
                StructureLayoutDef.Add(isStockpile);
            }
            // allowedThingsInStockpile
            if (isStockpileBool)
            {
                XElement allowedThingsInStockpile = new XElement("allowedThingsInStockpile", null);
                foreach (Thing item in map.zoneManager.ZoneAt(cellExport.FindAll(c => c.Walkable(map)).First()).AllContainedThings)
                {
                    XElement li = new XElement("li", item.def.defName);
                    if (item is Pawn || item is Filth || item.def.building != null) { }
                    else if (allowedThingsInStockpile.Elements().ToList().FindAll(x => x.Value == li.Value).Count() == 0) allowedThingsInStockpile.Add(li);
                }
                StructureLayoutDef.Add(allowedThingsInStockpile);
            }
            XElement layouts = new XElement("layouts", null);
            // Add pawns layout
            bool add = false;
            XElement pawnsL = KCSG_Utilities.Createpawnlayout(cellExport, defNamePrefix, area, out add, map, pairsSymbolLabel, pairsCellThingList);
            if (add) layouts.Add(pawnsL);
            // Add items layout
            bool add2 = false;
            XElement itemsL = KCSG_Utilities.CreateItemlayout(cellExport, defNamePrefix, area, out add2, map, pairsSymbolLabel, pairsCellThingList);
            if (add2) layouts.Add(itemsL);
            // Add terrain layout
            if (area != null) layouts.Add(KCSG_Utilities.CreateTerrainlayout(cellExport, defNamePrefix, area, map, pairsSymbolLabel));
            else layouts.Add(KCSG_Utilities.CreateTerrainlayout(cellExport, defNamePrefix, null, map, pairsSymbolLabel));
            // Add things layouts
            int numOfLayout = KCSG_Utilities.GetMaxThingOnOneCell(cellExport, map, pairsCellThingList);
            for (int i = 0; i < numOfLayout; i++)
            {
                layouts.Add(KCSG_Utilities.CreateThinglayout(cellExport, defNamePrefix, i, area, map, pairsSymbolLabel, pairsCellThingList));
            }

            StructureLayoutDef.Add(layouts);

            // Add roofGrid
            if (area != null) StructureLayoutDef.Add(KCSG_Utilities.CreateRoofGrid(cellExport, map, area));
            else StructureLayoutDef.Add(KCSG_Utilities.CreateRoofGrid(cellExport, map));

            return StructureLayoutDef;
        }

        public static XElement CreateThinglayout(List<IntVec3> cellExport, string defNamePrefix, int index, Area area, Map map, Dictionary<string, SymbolDef> pairsSymbolLabel, Dictionary<IntVec3, List<Thing>> pairsCellThingList)
        {
            XElement liMain = new XElement("li", null);
            int height, width;
            KCSG_Utilities.EdgeFromList(cellExport, out height, out width);
            List<Thing> aAdded = new List<Thing>();

            IntVec3 first = cellExport.First();
            for (int i = 0; i < height; i++)
            {
                XElement li = new XElement("li", null);
                string temp = "";
                for (int i2 = 0; i2 < width; i2++)
                {
                    List<Thing> things = pairsCellThingList.TryGetValue(first);
                    things.RemoveAll(t => t.def.category == ThingCategory.Pawn || t.def.category == ThingCategory.Item || t.def.category == ThingCategory.Filth || t.def.defName == "PowerConduit");
                    Thing thing;
                    if (things.Count < index + 1)
                    {
                        if (i2 + 1 == width) temp += ".";
                        else temp += ".,";
                    }
                    else if (area != null && !area.ActiveCells.Contains(first))
                    {
                        if (i2 + 1 == width) temp += ".";
                        else temp += ".,";
                    }
                    else
                    {
                        thing = things[index];
                        if (!aAdded.Contains(thing) && thing.Position == first)
                        {
                            SymbolDef symbolDef;
                            if (thing.Stuff != null && thing.def.rotatable) symbolDef = pairsSymbolLabel.Values.ToList().Find(s => s.thingDef == thing.def && s.stuffDef == thing.Stuff && s.rotation == thing.Rotation);
                            else if (thing.Stuff != null && !thing.def.rotatable) symbolDef = pairsSymbolLabel.Values.ToList().Find(s => s.thingDef == thing.def && s.stuffDef == thing.Stuff);
                            else symbolDef = pairsSymbolLabel.Values.ToList().Find(s => s.thingDef == thing.def && s.rotation == thing.Rotation);

                            if (symbolDef == null)
                            {
                                string symbolString = defNamePrefix + "_" + thing.def.defName;
                                if (thing.Stuff != null) symbolString += "_" + thing.Stuff.defName;
                                if (thing.def.rotatable && thing.def.category != ThingCategory.Plant) symbolString += "_" + thing.Rotation.ToStringHuman();

                                if (i2 + 1 == width) temp += symbolString;
                                else temp += symbolString + ",";
                            }
                            else
                            {
                                if (i2 + 1 == width) temp += symbolDef.symbol;
                                else temp += symbolDef.symbol + ",";
                            }
                            aAdded.Add(thing);
                        }
                        else
                        {
                            if (i2 + 1 == width) temp += ".";
                            else temp += ".,";
                        }
                    }
                    first.x++;
                }
                first.x -= width;
                li.Add(temp);
                liMain.Add(li);
                first.z++;
            }
            return liMain;
        }

        public static XElement CreateTerrainlayout(List<IntVec3> cellExport, string defNamePrefix, Area area, Map map, Dictionary<string, SymbolDef> pairsSymbolLabel)
        {
            XElement liMain = new XElement("li", null);
            int height, width;
            KCSG_Utilities.EdgeFromList(cellExport, out height, out width);

            IntVec3 first = cellExport.First();
            for (int i = 0; i < height; i++)
            {
                XElement li = new XElement("li", null);
                string temp = "";
                for (int i2 = 0; i2 < width; i2++)
                {
                    if (area != null && !area.ActiveCells.Contains(first))
                    {
                        if (i2 + 1 == width) temp += ".";
                        else temp += ".,";
                    }
                    else if (!map.terrainGrid.TerrainAt(first).BuildableByPlayer)
                    {
                        if (i2 + 1 == width) temp += ".";
                        else temp += ".,";
                    }
                    else
                    {
                        // Find corresponding symbol
                        TerrainDef terrainD = map.terrainGrid.TerrainAt(first);
                        SymbolDef symbolDef = pairsSymbolLabel.Values.ToList().Find(s => s.isTerrain && s.terrainDef.defName == terrainD.defName);
                        if (symbolDef == null)
                        {
                            if (i2 + 1 == width) temp += defNamePrefix + "_" + terrainD.defName;
                            else temp += terrainD.defName + ",";
                        }
                        else
                        {
                            if (i2 + 1 == width) temp += symbolDef.symbol;
                            else temp += symbolDef.symbol + ",";
                        }
                    }
                    first.x++;
                }
                first.x -= width;
                li.Add(temp);
                liMain.Add(li);
                first.z++;
            }
            return liMain;
        }

        public static XElement CreateRoofGrid(List<IntVec3> cellExport, Map map, Area area = null)
        {
            XElement roofGrid = new XElement("roofGrid", null);
            int height, width;
            KCSG_Utilities.EdgeFromList(cellExport, out height, out width);

            IntVec3 first = cellExport.First();
            for (int i = 0; i < height; i++)
            {
                XElement li = new XElement("li", null);
                string temp = "";
                for (int i2 = 0; i2 < width; i2++)
                {
                    if (area != null && !area.ActiveCells.Contains(first))
                    {
                        if (i2 + 1 == width) temp += ".";
                        else temp += ".,";
                    }
                    else
                    {
                        if (first.Roofed(map))
                        {
                            if (i2 + 1 == width) temp += "1";
                            else temp += "1,";
                        }
                        else
                        {
                            if (i2 + 1 == width) temp += ".";
                            else temp += ".,";
                        }
                    }
                    first.x++;
                }
                first.x -= width;
                li.Add(temp);
                roofGrid.Add(li);
                first.z++;
            }
            return roofGrid;
        }

        public static XElement Createpawnlayout(List<IntVec3> cellExport, string defNamePrefix, Area area, out bool add, Map map, Dictionary<string, SymbolDef> pairsSymbolLabel, Dictionary<IntVec3, List<Thing>> pairsCellThingList)
        {
            XElement liMain = new XElement("li", null);
            int height, width;
            add = false;
            KCSG_Utilities.EdgeFromList(cellExport, out height, out width);

            IntVec3 first = cellExport.First();
            for (int i = 0; i < height; i++)
            {
                XElement li = new XElement("li", null);
                string temp = "";
                for (int i2 = 0; i2 < width; i2++)
                {
                    if (area != null && !area.ActiveCells.Contains(first))
                    {
                        if (i2 + 1 == width) temp += ".";
                        else temp += ".,";
                    }
                    else
                    {
                        List<Thing> things = pairsCellThingList.TryGetValue(first).FindAll(t => t is Pawn);
                        if (things.Count == 0)
                        {
                            if (i2 + 1 == width) temp += ".";
                            else temp += ".,";
                        }
                        else
                        {
                            add = true;
                            foreach (Pawn pawn in things)
                            {
                                SymbolDef symbolDef;
                                symbolDef = pairsSymbolLabel.Values.ToList().Find(s => s.pawnKindDefNS == pawn.kindDef);
                                if (symbolDef == null)
                                {
                                    string symbolString = defNamePrefix + "_" + pawn.kindDef.defName;

                                    if (i2 + 1 == width) temp += symbolString;
                                    else temp += symbolString + ",";
                                }
                                else
                                {
                                    if (i2 + 1 == width) temp += symbolDef.symbol;
                                    else temp += symbolDef.symbol + ",";
                                }
                            }
                        }
                    }
                    first.x++;
                }
                first.x -= width;
                li.Add(temp);
                liMain.Add(li);
                first.z++;
            }
            return liMain;
        }

        public static XElement CreateItemlayout(List<IntVec3> cellExport, string defNamePrefix, Area area, out bool add, Map map, Dictionary<string, SymbolDef> pairsSymbolLabel, Dictionary<IntVec3, List<Thing>> pairsCellThingList)
        {
            XElement liMain = new XElement("li", null);
            int height, width;
            add = false;
            KCSG_Utilities.EdgeFromList(cellExport, out height, out width);

            IntVec3 first = cellExport.First();
            for (int i = 0; i < height; i++)
            {
                XElement li = new XElement("li", null);
                string temp = "";
                for (int i2 = 0; i2 < width; i2++)
                {
                    if (area != null && !area.ActiveCells.Contains(first))
                    {
                        if (i2 + 1 == width) temp += ".";
                        else temp += ".,";
                    }
                    else
                    {
                        List<Thing> things = pairsCellThingList.TryGetValue(first).FindAll(t => t.def.category == ThingCategory.Item && t.def.category != ThingCategory.Filth);
                        if (things.Count == 0)
                        {
                            if (i2 + 1 == width) temp += ".";
                            else temp += ".,";
                        }
                        else
                        {
                            add = true;
                            foreach (Thing item in things)
                            {
                                SymbolDef symbolDef;
                                symbolDef = pairsSymbolLabel.Values.ToList().Find(s => s.thingDef == item.def && s.isItem);
                                if (symbolDef == null)
                                {
                                    string symbolString = defNamePrefix + "_Item_" + item.def.defName;

                                    if (i2 + 1 == width) temp += symbolString;
                                    else temp += symbolString + ",";
                                }
                                else
                                {
                                    if (i2 + 1 == width) temp += symbolDef.symbol;
                                    else temp += symbolDef.symbol + ",";
                                }
                            }
                        }
                    }
                    first.x++;
                }
                first.x -= width;
                li.Add(temp);
                liMain.Add(li);
                first.z++;
            }
            return liMain;
        }

        #endregion Layout Creation

        #region Other Utils

        public static void FillCellThingsList(List<IntVec3> cellExport, Map map, Dictionary<IntVec3, List<Thing>> pairsCellThingList)
        {
            pairsCellThingList.Clear();
            foreach (IntVec3 intVec in cellExport)
            {
                pairsCellThingList.Add(intVec, intVec.GetThingList(map).FindAll(t => t.def.defName != "").ToList());
            }
        }

        public static Dictionary<string, SymbolDef> FillpairsSymbolLabel()
        {
            Dictionary<string, SymbolDef> pairsSymbolLabel = new Dictionary<string, SymbolDef>();
            List<SymbolDef> symbolDefs = DefDatabase<SymbolDef>.AllDefsListForReading;
            foreach (SymbolDef s in symbolDefs)
            {
                pairsSymbolLabel.Add(s.symbol, s);
            }
            return pairsSymbolLabel;
        }

        #endregion Other Utils
    }
}