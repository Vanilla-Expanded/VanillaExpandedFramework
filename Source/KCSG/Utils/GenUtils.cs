using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace KCSG
{
    public class GenUtils
    {
        public static void GenerateRoomFromLayout(List<string> layoutList, CellRect roomRect, Map map, StructureLayoutDef rld)
        {
            if (rld.roofGrid != null)
            {
                GenerateRoofGrid(rld.roofGrid, roomRect, map);
            }

            Dictionary<string, SymbolDef> pairsSymbolLabel = KCSG_Utilities.FillpairsSymbolLabel();
            List<string> allSymbList = new List<string>();
            foreach (string str in layoutList)
            {
                allSymbList.AddRange(str.Split(','));
            }

            int l = 0;
            foreach (IntVec3 cell in roomRect)
            {
                if (l < allSymbList.Count && allSymbList[l] != ".")
                {
                    pairsSymbolLabel.TryGetValue(allSymbList[l], out SymbolDef temp);
                    Thing thing;
                    if (temp != null)
                    {
                        if (temp.isTerrain && temp.terrainDef != null)
                        {
                            cell.GetFirstMineable(map)?.DeSpawn();
                            if (!cell.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Heavy))
                            {
                                map.terrainGrid.SetTerrain(cell, TerrainDefOf.Bridge);
                            }
                            else
                            {
                                map.terrainGrid.SetTerrain(cell, temp.terrainDef);
                            }
                        }
                        else if (temp.isPawn && temp.pawnKindDefNS != null)
                        {
                            if (temp.lordJob != null)
                            {
                                Lord lord = CreateNewLord(temp.lordJob, map, cell);
                                for (int i = 0; i < temp.numberToSpawn; i++)
                                {
                                    Pawn pawn = temp.spawnPartOfFaction ? PawnGenerator.GeneratePawn(temp.pawnKindDefNS, map.ParentFaction) : PawnGenerator.GeneratePawn(temp.pawnKindDefNS);
                                    if (temp.isSlave) pawn.guest.SetGuestStatus(map.ParentFaction, true);

                                    GenSpawn.Spawn(pawn, cell, map, WipeMode.FullRefund);
                                    lord.AddPawn(pawn);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < temp.numberToSpawn; i++)
                                {
                                    Pawn pawn = temp.spawnPartOfFaction ? PawnGenerator.GeneratePawn(temp.pawnKindDefNS, map.ParentFaction) : PawnGenerator.GeneratePawn(temp.pawnKindDefNS);

                                    if (temp.isSlave) pawn.guest.SetGuestStatus(map.ParentFaction, true);
                                    GenSpawn.Spawn(pawn, cell, map, WipeMode.FullRefund);
                                }
                            }
                        }
                        else if (temp.isItem && temp.thingDef != null && cell.Walkable(map))
                        {
                            thing = temp.thingDef.stuffCategories != null ? ThingMaker.MakeThing(temp.thingDef, GenStuff.RandomStuffFor(temp.thingDef)) : ThingMaker.MakeThing(temp.thingDef);

                            thing.stackCount = Mathf.Clamp(temp.stackCount.RandomInRange, 1, 75);
                            if (thing.TryGetComp<CompQuality>() != null) thing.TryGetComp<CompQuality>().SetQuality(QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Outsider);

                            GenSpawn.Spawn(thing, cell, map, WipeMode.FullRefund);
                            if (thing.TryGetComp<CompForbiddable>() != null) thing.SetForbidden(true);
                        }
                        else if (temp.thingDef != null)
                        {
                            thing = ThingMaker.MakeThing(temp.thingDef, temp.stuffDef);

                            if (thing.TryGetComp<CompRefuelable>() != null) thing.TryGetComp<CompRefuelable>().Refuel((int)thing.TryGetComp<CompRefuelable>().Props.fuelCapacity / 2);
                            if (thing.TryGetComp<CompPowerBattery>() != null) thing.TryGetComp<CompPowerBattery>().AddEnergy(thing.TryGetComp<CompPowerBattery>().Props.storedEnergyMax);
                            if (thing is Building_Casket inheritFromCasket)
                            {
                                Pawn pawn = temp.containPawnKind != null ? PawnGenerator.GeneratePawn(temp.containPawnKindDef, map.ParentFaction) : PawnGenerator.GeneratePawn(PawnKindDefOf.Villager, map.ParentFaction);
                                inheritFromCasket.TryAcceptThing(pawn);
                            }

                            if (cell.GetFirstMineable(map) != null && thing.def.designationCategory == DesignationCategoryDefOf.Security)
                            {
                                l++;
                                continue;
                            }
                            else if (thing.def.category == ThingCategory.Plant && cell.GetTerrain(map).fertility > 0.5 && cell.Walkable(map)) // If it's a plant
                            {
                                Plant plant = thing as Plant;
                                plant.Growth = temp.plantGrowth; // apply the growth
                                GenSpawn.Spawn(plant, cell, map, WipeMode.FullRefund);
                            }
                            else if (thing.def.category == ThingCategory.Building)
                            {
                                if (!cell.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Heavy) && !thing.def.building.isNaturalRock) 
                                    map.terrainGrid.SetTerrain(cell, TerrainDefOf.Bridge);

                                if (thing.def.rotatable)
                                    GenSpawn.Spawn(thing, cell, map, new Rot4(temp.rotation.AsInt), WipeMode.FullRefund);
                                else
                                    GenSpawn.Spawn(thing, cell, map, WipeMode.FullRefund);
                                
                                thing.SetFactionDirect(map.ParentFaction);
                            }

                            if (thing.def.passability == Traversability.Impassable && map.ParentFaction?.def.techLevel >= TechLevel.Industrial) // Add power cable under all impassable
                            {
                                GenSpawn.Spawn(ThingDefOf.PowerConduit, cell, map, WipeMode.FullRefund);
                            }
                        }
                    }
                }
                l++;
            }
        }

        public static void GenerateRoofGrid(List<string> roofGrid, CellRect roomRect, Map map)
        {
            List<string> rg = new List<string>();
            foreach (string str in roofGrid)
            {
                rg.AddRange(str.Split(','));
            }

            for (int i = 0; i < rg.Count; i++)
            {
                if (rg[i] == "1")
                {
                    map.roofGrid.SetRoof(roomRect.Cells.ElementAt(i), RoofDefOf.RoofConstructed);
                    if (!roomRect.Cells.ElementAt(i).GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Heavy))
                    {
                        map.terrainGrid.SetTerrain(roomRect.Cells.ElementAt(i), TerrainDefOf.Bridge);
                    }
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
                        if (TryFindClosestReachableNet(compPowerBattery.parent.Position, (PowerNet x) => HasAnyPowerGenerator(x), map, out PowerNet powerNet2, out IntVec3 dest, tmpPowerNetPredicateResults))
                        {
                            map.floodFiller.ReconstructLastFloodFillPath(dest, tmpCells);
                            if (powerNet2 != null)
                            {
                                SpawnTransmitters(tmpCells, map, compPowerBattery.parent.Faction, conduit);
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
                if (IsPowerUser(tmpThings[i]))
                {
                    CompPowerTrader powerComp = tmpThings[i].TryGetComp<CompPowerTrader>();
                    PowerNet powerNet = powerComp.PowerNet;
                    if (powerNet != null && powerNet.hasPowerSource)
                    {
                        TryTurnOnImmediately(powerComp, map);
                    }
                    else
                    {
                        map.powerNetManager.UpdatePowerNetsAndConnections_First();
                        if (TryFindClosestReachableNet(powerComp.parent.Position, (PowerNet x) => x.CurrentEnergyGainRate() - powerComp.Props.basePowerConsumption * CompPower.WattsToWattDaysPerTick > 1E-07f, map, out PowerNet powerNet2, out IntVec3 dest, tmpPowerNetPredicateResults))
                        {
                            map.floodFiller.ReconstructLastFloodFillPath(dest, tmpCells);
                            SpawnTransmitters(tmpCells, map, tmpThings[i].Faction, conduit);
                            TryTurnOnImmediately(powerComp, map);
                        }
                        else if (TryFindClosestReachableNet(powerComp.parent.Position, (PowerNet x) => x.CurrentStoredEnergy() > 1E-07f, map, out powerNet2, out dest, tmpPowerNetPredicateResults))
                        {
                            map.floodFiller.ReconstructLastFloodFillPath(dest, tmpCells);
                            SpawnTransmitters(tmpCells, map, tmpThings[i].Faction, conduit);
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
                if (IsPowerGenerator(tmpThings[i]))
                {
                    PowerNet powerNet = tmpThings[i].TryGetComp<CompPower>().PowerNet;
                    if (powerNet == null || !HasAnyPowerUser(powerNet))
                    {
                        map.powerNetManager.UpdatePowerNetsAndConnections_First();
                        if (TryFindClosestReachableNet(tmpThings[i].Position, (PowerNet x) => HasAnyPowerUser(x), map, out PowerNet powerNet2, out IntVec3 dest, tmpPowerNetPredicateResults))
                        {
                            map.floodFiller.ReconstructLastFloodFillPath(dest, tmpCells);
                            SpawnTransmitters(tmpCells, map, tmpThings[i].Faction, conduit);
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
                if (IsPowerUser(powerComps[i].parent))
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
                    GenSpawn.Spawn(conduit, cells[i], map, WipeMode.FullRefund).SetFaction(faction, null);
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
                if (IsPowerGenerator(powerComps[i].parent))
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
            map.floodFiller.FloodFill(root, (IntVec3 x) => EverPossibleToTransmitPowerAt(x, map), delegate (IntVec3 x)
            {
                Building transmitter = x.GetTransmitter(map);
                PowerNet powerNet = transmitter?.GetComp<CompPower>().PowerNet;
                if (powerNet == null)
                {
                    return false;
                }
                if (!tmpPowerNetPredicateResults.TryGetValue(powerNet, out bool flag))
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
    }
}