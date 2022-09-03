using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using static KCSG.SettlementGenUtils;

namespace KCSG
{
    public class GenUtils
    {
        /// <summary>
        /// Generate layoutDef in rect
        /// </summary>
        public static void GenerateLayout(StructureLayoutDef layout, CellRect rect, Map map)
        {
            ThingDef wallForRoom = null;
            if (GenOption.StuffableOptions != null && GenOption.StuffableOptions.randomizeWall)
                wallForRoom = RandomWallStuffByWeight(ThingDefOf.Wall);

            for (int index = 0; index < layout.layouts.Count; index++)
            {
                GenerateRoomFromLayout(layout, index, rect, map, wallForRoom);
            }
            GenerateRoofGrid(layout, rect, map);
        }

        /// <summary>
        /// Entry method to generate a structure from a StructureLayoutDef
        /// </summary>
        private static void GenerateRoomFromLayout(StructureLayoutDef layout, int index, CellRect rect, Map map, ThingDef wallForRoom = null)
        {
            Faction faction = map.ParentFaction;

            var cells = rect.Cells.ToList();
            int count = cells.Count;

            for (int i = 0; i < count; i++)
            {
                IntVec3 cell = cells[i];
                if (cell.InBounds(map))
                {
                    SymbolDef symbol = layout.symbolsLists[index][i];
                    if (symbol != null)
                    {
                        GenerateSymbol(symbol, layout, map, cell, faction, wallForRoom);
                    }
                }
            }
        }

        /// <summary>
        /// Generate symbol at cell
        /// </summary>
        public static void GenerateSymbol(SymbolDef symbol, StructureLayoutDef layout, Map map, IntVec3 cell, Faction faction, ThingDef wallForRoom)
        {
            if (symbol.isTerrain && symbol.terrainDef != null)
            {
                GenerateTerrainAt(map, cell, symbol.terrainDef);
            }
            else if (symbol.pawnKindDefNS != null && (GenOption.ext == null || GenOption.ext.SymbolResolvers == null))
            {
                GeneratePawnAt(map, cell, symbol);
            }
            else if (symbol.thingDef != null)
            {
                if (symbol.thingDef.category == ThingCategory.Item)
                {
                    GenerateItemAt(map, cell, symbol);
                }
                else if (symbol.thingDef.category == ThingCategory.Plant)
                {
                    var terrain = cell.GetTerrain(map);
                    if (!terrain.BuildableByPlayer)
                    {
                        if (symbol.thingDef.plant != null && terrain.fertility >= symbol.thingDef.plant.fertilityMin)
                        {
                            Plant plant = ThingMaker.MakeThing(symbol.thingDef) as Plant;
                            plant.Growth = symbol.plantGrowth;
                            GenSpawn.Spawn(plant, cell, map, WipeMode.VanishOrMoveAside);
                        }
                    }
                }
                else if (symbol.thingDef.category == ThingCategory.Pawn && GenOption.ext?.SymbolResolvers == null)
                {
                    GenSpawn.Spawn(symbol.thingDef, cell, map, WipeMode.VanishOrMoveAside);
                }
                else
                {
                    if (GenOption.mineables[cell] != null && symbol.thingDef.designationCategory == DesignationCategoryDefOf.Security)
                    {
                        return;
                    }
                    GenerateBuildingAt(map, cell, symbol, layout, faction, wallForRoom);

                    // Generating settlement, we want to keep tracks of doors
                    if (GenOption.ext != null && !GenOption.ext.UsingSingleLayout && symbol.thingDef.altitudeLayer == AltitudeLayer.DoorMoveable)
                    {
                        doors?.Add(cell);
                    }
                }
            }
        }

        /// <summary>
        /// Generate terrain at cell. Make bridge if needed, remove mineable if needed.
        /// </summary>
        private static void GenerateTerrainAt(Map map, IntVec3 cell, TerrainDef terrainDef)
        {
            if (!cell.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Heavy))
            {
                map.terrainGrid.SetTerrain(cell, TerrainDefOf.Bridge);
            }
            else
            {
                GenOption.DespawnMineableAt(cell);
                map.terrainGrid.SetTerrain(cell, terrainDef);
            }
        }

        /// <summary>
        /// Generate pawn(s) at pos
        /// </summary>
        private static void GeneratePawnAt(Map map, IntVec3 cell, SymbolDef symbol)
        {
            var factionManager = Find.FactionManager;
            var parentFaction = map.ParentFaction;
            var symbolFaction = symbol.faction != null ? factionManager.FirstFactionOfDef(symbol.faction) : null;
            var slaveFaction = parentFaction != null ? factionManager.AllFactionsListForReading.FindAll(f => parentFaction.HostileTo(f)).RandomElement() : factionManager.AllFactionsListForReading.Find(f => f != Faction.OfPlayer && f != parentFaction);
            var pawns = new List<Pawn>();

            var request = new PawnGenerationRequest(symbol.pawnKindDefNS, symbol.isSlave ? slaveFaction : (symbol.spawnPartOfFaction ? parentFaction : symbolFaction), mustBeCapableOfViolence: true);

            for (int i = 0; i < symbol.numberToSpawn; i++)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(request);
                if (pawn == null)
                {
                    Debug.Message("Null pawn in GeneratePawnAt");
                    continue;
                }

                if (symbol.isSlave && parentFaction != null)
                {
                    pawn.guest.SetGuestStatus(parentFaction, GuestStatus.Prisoner);
                }

                if (symbol.spawnDead)
                {
                    pawn.Kill(new DamageInfo(DamageDefOf.Cut, 9999));
                    Corpse corpse = pawn.Corpse;
                    corpse.timeOfDeath = Mathf.Max(Find.TickManager.TicksGame - 120000, 0);
                    if (symbol.spawnRotten)
                    {
                        GenOption.corpseToRot.Add(corpse);
                        if (symbol.spawnFilthAround)
                        {
                            for (int x = 0; x < 5; x++)
                            {
                                IntVec3 rNext = new IntVec3();
                                RCellFinder.TryFindRandomCellNearWith(cell, ni => ni.Walkable(map), map, out rNext, 1, 3);
                                GenSpawn.Spawn(ThingDefOf.Filth_CorpseBile, rNext, map);
                            }
                        }
                    }
                    GenSpawn.Spawn(corpse, cell, map);
                }
                else
                {
                    GenSpawn.Spawn(pawn, cell, map, WipeMode.VanishOrMoveAside);
                    pawns.Add(pawn);
                }
            }

            if (symbol.defendSpawnPoint)
            {
                Lord lord = LordMaker.MakeNewLord(parentFaction, new LordJob_DefendPoint(cell, 3f, addFleeToil: false), map, pawns);
            }
        }

        /// <summary>
        /// Generate item at pos
        /// </summary>
        private static void GenerateItemAt(Map map, IntVec3 cell, SymbolDef symbol)
        {
            Thing thing = ThingMaker.MakeThing(symbol.thingDef, symbol.stuffDef ?? (symbol.thingDef.stuffCategories?.Count > 0 ? GenStuff.RandomStuffFor(symbol.thingDef) : null));

            if (symbol.maxStackSize != -1)
            {
                thing.stackCount = Rand.RangeInclusive(1, symbol.maxStackSize);
            }
            else
            {
                thing.stackCount = Mathf.Clamp(Rand.RangeInclusive(1, symbol.thingDef.stackLimit), 1, 75);
            }

            thing.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityBaseGen(), ArtGenerationContext.Outsider);

            GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Direct);
            thing.SetForbidden(true, false);
        }

        /// <summary>
        /// Generate building at pos
        /// </summary>
        private static void GenerateBuildingAt(Map map, IntVec3 cell, SymbolDef symbol, StructureLayoutDef layout, Faction faction, ThingDef wallStuff = null)
        {
            // If it's a shuttle, generate it properly
            if (symbol.thingDef == ThingDefOf.Shuttle)
            {
                ResolveParams rp = new ResolveParams
                {
                    singleThingDef = ThingDefOf.Shuttle,
                    rect = CellRect.SingleCell(cell),
                    faction = faction,
                    postThingSpawn = x =>
                    {
                        TransportShip transportShip = TransportShipMaker.MakeTransportShip(TransportShipDefOf.Ship_Shuttle, null, x);
                        ShipJob_WaitTime shipJobWaitTime = (ShipJob_WaitTime)ShipJobMaker.MakeShipJob(ShipJobDefOf.WaitTime);
                        shipJobWaitTime.duration = new IntRange(300, 3600).RandomInRange;
                        shipJobWaitTime.showGizmos = false;
                        transportShip.AddJob(shipJobWaitTime);
                        transportShip.AddJob(ShipJobDefOf.FlyAway);
                        transportShip.Start();
                    }
                };
                BaseGen.symbolStack.Push("thing", rp);
                return;
            }
            // Make the thing, with the right stuff for it
            Thing thing;
            if (symbol.thingDef.defName.ToLower().Contains("wall"))
            {
                thing = ThingMaker.MakeThing(symbol.thingDef, symbol.thingDef.MadeFromStuff ? wallStuff ?? RandomWallStuffByWeight(symbol) : null);
            }
            else
            {
                thing = ThingMaker.MakeThing(symbol.thingDef, RandomFurnitureStuffByWeight(symbol));
            }
            // Sanity check
            if (thing == null)
                return;
            // If ideology is loaded, try to apply the right style
            if (ModsConfig.IdeologyActive && faction != null && faction.ideos != null && faction.ideos.PrimaryIdeo is Ideo p)
            {
                thing.SetStyleDef(p.GetStyleFor(thing.def));
            }
            // Try to refuel if applicable
            CompRefuelable refuelable = thing.TryGetComp<CompRefuelable>();
            refuelable?.Refuel(refuelable.Props.fuelCapacity);
            // Try to recharge if applicable
            CompPowerBattery battery = thing.TryGetComp<CompPowerBattery>();
            battery?.AddEnergy(battery.Props.storedEnergyMax);
            // Try to fill item container
            if (thing is Building_Crate crate)
            {
                List<Thing> innerThings = new List<Thing>();
                if (faction == Faction.OfPlayer && symbol.thingSetMakerDefForPlayer != null)
                {
                    innerThings = symbol.thingSetMakerDefForPlayer.root.Generate(new ThingSetMakerParams());
                }
                else if (symbol.thingSetMakerDef != null)
                {
                    innerThings = symbol.thingSetMakerDef.root.Generate(new ThingSetMakerParams());
                }

                for (int i = 0; i < innerThings.Count; i++)
                {
                    var innerThing = innerThings[i];
                    innerThing.stackCount = Math.Min((int)(innerThing.stackCount * symbol.crateStackMultiplier), innerThing.def.stackLimit);
                    if (!crate.TryAcceptThing(innerThing))
                    {
                        Debug.Message($"Cannot add {innerThing.def.defName} to {crate.def.defName}");
                        innerThing.Destroy();
                    }
                }
            }
            // Try to fill corpse container
            else if (thing is Building_CorpseCasket corpseCasket && Rand.Value <= symbol.chanceToContainPawn)
            {
                Pawn pawn = GeneratePawnForContainer(symbol, map);
                pawn.Kill(null);

                var corpse = pawn.Corpse;
                GenOption.corpseToRot.Add(corpse);

                corpseCasket.GetComp<CompAssignableToPawn_Grave>()?.TryAssignPawn(pawn);
                if (!corpseCasket.TryAcceptThing(corpse))
                {
                    Debug.Message($"Building_CorpseCasket: Cannot add {pawn.Corpse} to {corpseCasket.def.defName}");
                }
            }
            // Try to fill pawn container
            else if (thing is Building_CryptosleepCasket casket && Rand.Value <= symbol.chanceToContainPawn)
            {
                Pawn pawn = GeneratePawnForContainer(symbol, map);
                if (!casket.TryAcceptThing(pawn))
                {
                    Debug.Message($"Building_Casket: Cannot add {pawn} to {casket.def.defName}");
                }
            }
            // If terrain at pos is bridgeable
            if (!cell.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Heavy))
            {
                // If is natural rock, try to spawn rough stone terrain around
                if (thing.def.building != null && thing.def.building.isNaturalRock)
                {
                    TerrainDef t = DefDatabase<TerrainDef>.GetNamedSilentFail($"{thing.def.defName}_Rough");
                    if (t != null)
                    {
                        map.terrainGrid.SetTerrain(cell, t);
                        foreach (IntVec3 intVec3 in GenAdjFast.AdjacentCells8Way(cell))
                        {
                            if (intVec3.InBounds(map) && !intVec3.GetTerrain(map).BuildableByPlayer)
                            {
                                map.terrainGrid.SetTerrain(intVec3, t);
                            }
                        }
                    }
                }
                // else spawn bridge
                else
                {
                    map.terrainGrid.SetTerrain(cell, TerrainDefOf.Bridge);
                }
            }
            // Spawn the thing
            GenSpawn.Spawn(thing, cell, map, symbol.rotation, WipeMode.VanishOrMoveAside);
            // Set the faction if applicable
            if (faction != null && thing.def.CanHaveFaction)
            {
                thing.SetFactionDirect(faction);
            }
            // Set bed for slave
            if (thing is Building_Bed bed && layout.IsForSlaves)
            {
                bed.ForOwnerType = BedOwnerType.Slave;
            }
            // Try generate conduit under impassable things and doors
            if (layout.spawnConduits && !thing.def.mineable && (thing.def.passability == Traversability.Impassable || thing.def.IsDoor) && faction?.def.techLevel >= TechLevel.Industrial)
            {
                Thing c = ThingMaker.MakeThing(AllDefOf.KCSG_PowerConduit);
                if (faction != null)
                {
                    c.SetFactionDirect(faction);
                }
                GenSpawn.Spawn(c, cell, map, WipeMode.FullRefund);
            }
            // Try to fill shelves
            if (thing is Building_Storage storage
                && GenOption.sld != null
                && GenOption.sld.stockpileOptions.fillStorageBuildings
                && !GenOption.sld.stockpileOptions.fillWithDefs.NullOrEmpty())
            {
                var marketValue = GenOption.sld.stockpileOptions.RefMarketValue;
                foreach (var storageCell in storage.AllSlotCells())
                {
                    var otherThing = storageCell.GetFirstItem(map);
                    if (Rand.Value < GenOption.sld.stockpileOptions.fillChance && GenOption.sld.stockpileOptions.replaceOtherThings || otherThing == null)
                    {
                        if (GenOption.sld.stockpileOptions.replaceOtherThings && otherThing.Spawned)
                            otherThing.DeSpawn();

                        var thingDef = GenOption.sld.stockpileOptions.fillWithDefs.RandomElementByWeight(t => marketValue - t.BaseMarketValue);
                        var item = ThingMaker.MakeThing(thingDef, thingDef.stuffCategories?.Count > 0 ? GenStuff.RandomStuffFor(thingDef) : null);

                        if (item.MarketValue <= GenOption.sld.stockpileOptions.maxValueStackIncrease)
                            item.stackCount = Mathf.Clamp(Rand.RangeInclusive(1, thingDef.stackLimit), 1, 90);

                        item.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityBaseGen(), ArtGenerationContext.Outsider);

                        GenPlace.TryPlaceThing(item, storageCell, map, ThingPlaceMode.Direct);
                        item.SetForbidden(true, false);
                    }
                }
            }
            // Handle mortar and mortar pawns
            SpawnMortar(thing, faction, map);
        }

        /// <summary>
        /// Spawn mortar manning pawn with the right job
        /// </summary>
        private static void SpawnMortar(Thing thing, Faction faction, Map map)
        {
            if (thing?.def?.building?.buildingTags?.Count > 0)
            {
                if (thing.def.building.IsMortar && thing.def.building.buildingTags.Contains("Artillery_MannedMortar") && thing.def.HasComp(typeof(CompMannable)) && faction != null)
                {
                    // Spawn pawn
                    var request = new PawnGenerationRequest(faction.RandomPawnKind(), faction, PawnGenerationContext.NonPlayer, map.Tile, mustBeCapableOfViolence: true, inhabitant: true);
                    var pawn = PawnGenerator.GeneratePawn(request);
                    var job = JobMaker.MakeJob(JobDefOf.ManTurret, thing);
                    job.expiryInterval = 20000;
                    job.expireRequiresEnemiesNearby = true;

                    GenSpawn.Spawn(pawn, thing.InteractionCell, map);
                    pawn.jobs.TryTakeOrderedJob(job);
                    // Spawn shells
                    ThingDef shellDef = TurretGunUtility.TryFindRandomShellDef(thing.def, false, true, faction.def.techLevel, false, 250f);
                    if (shellDef != null)
                    {
                        ResolveParams rpShell = new ResolveParams
                        {
                            faction = faction,
                            singleThingDef = shellDef,
                            singleThingStackCount = Rand.RangeInclusive(8, Math.Min(12, shellDef.stackLimit))
                        };
                        BaseGen.symbolStack.Push("thing", rpShell);
                    }
                }
            }
        }

        /// <summary>
        /// Generate pawn to be put inside container (casked, tomb...)
        /// </summary>
        private static Pawn GeneratePawnForContainer(SymbolDef temp, Map map)
        {
            Faction faction = temp.spawnPartOfFaction ? map.ParentFaction : null;
            if (temp.containPawnKindForPlayerAnyOf.Count > 0 && faction == Faction.OfPlayer)
            {
                return PawnGenerator.GeneratePawn(new PawnGenerationRequest(temp.containPawnKindForPlayerAnyOf.RandomElement(), faction, forceGenerateNewPawn: true));
            }
            else if (temp.containPawnKindAnyOf.Count > 0)
            {
                return PawnGenerator.GeneratePawn(new PawnGenerationRequest(temp.containPawnKindAnyOf.RandomElement(), faction, forceGenerateNewPawn: true));
            }

            return PawnGenerator.GeneratePawn(new PawnGenerationRequest(faction != null ? faction.RandomPawnKind() : PawnKindDefOf.Villager, faction, forceGenerateNewPawn: true));
        }

        /// <summary>
        /// Generate roof from layout resolved roof grid
        /// </summary>
        private static void GenerateRoofGrid(StructureLayoutDef layout, CellRect rect, Map map)
        {
            if (layout.roofGrid != null && layout.roofGridResolved.Count > 0)
            {
                var cells = rect.Cells.ToList();
                int count = cells.Count;

                for (int i = 0; i < count; i++)
                {
                    IntVec3 cell = cells[i];
                    if (cell.InBounds(map))
                    {
                        var wantedRoof = layout.roofGridResolved[i];
                        if (wantedRoof == "0" && layout.forceGenerateRoof)
                        {
                            map.roofGrid.SetRoof(cell, null);
                            continue;
                        }

                        var currentRoof = cell.GetRoof(map);
                        if (wantedRoof == "1" && (layout.forceGenerateRoof || currentRoof == null))
                        {
                            map.roofGrid.SetRoof(cell, RoofDefOf.RoofConstructed);
                            ClearInterferesWithRoof(cell, map);
                            continue;
                        }

                        if (wantedRoof == "2" && (layout.forceGenerateRoof || currentRoof == null || currentRoof == RoofDefOf.RoofConstructed))
                        {
                            map.roofGrid.SetRoof(cell, RoofDefOf.RoofRockThin);
                            ClearInterferesWithRoof(cell, map);
                            continue;
                        }

                        if (wantedRoof == "3")
                        {
                            map.roofGrid.SetRoof(cell, RoofDefOf.RoofRockThick);
                            ClearInterferesWithRoof(cell, map);
                            continue;
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Despawn everything that block/interfer with roofs
        /// </summary>
        private static void ClearInterferesWithRoof(IntVec3 cell, Map map)
        {
            var t = cell.GetPlant(map);
            if (t != null && t.def.plant != null && t.def.plant.interferesWithRoof)
                t.DeSpawn();

            GenOption.DespawnMineableAt(cell);
        }

        /// <summary>
        /// Get road type already on map if applicable
        /// </summary>
        public static List<TerrainDef> SetRoadInfo(Map map)
        {
            if (map.TileInfo?.Roads?.Count > 0)
            {
                var preRoadTypes = new List<TerrainDef>();
                foreach (RimWorld.Planet.Tile.RoadLink roadLink in map.TileInfo.Roads)
                {
                    foreach (RoadDefGenStep rgs in roadLink.road.roadGenSteps)
                    {
                        if (rgs is RoadDefGenStep_Place rgsp && rgsp != null && rgsp.place is TerrainDef t && t != null && t != TerrainDefOf.Bridge)
                        {
                            preRoadTypes.Add(t);
                        }
                    }
                }
                return preRoadTypes;
            }
            return null;
        }

        /// <summary>
        /// Clean structure spawn rect. Full clean remove everything but player/map pawns.
        /// Normal clean remove filth, non-natural buildings and stone chunks
        /// </summary>
        public static void PreClean(Map map, CellRect rect, bool fullClean, List<string> roofGrid = null)
        {
            Debug.Message($"Pre-generation map clean. Fullclean {fullClean}");
            var mapFaction = map.ParentFaction;
            var player = Faction.OfPlayer;

            if (roofGrid != null)
            {
                var cells = rect.Cells.ToList();

                for (int i = 0; i < roofGrid.Count && i < cells.Count; i++)
                {
                    IntVec3 cell = cells[i];
                    if (cell.InBounds(map) && roofGrid[i] != ".")
                        CleanAt(cell, map, fullClean, mapFaction, player);
                }
            }
            else
            {
                foreach (IntVec3 c in rect)
                    CleanAt(c, map, fullClean, mapFaction, player);
            }

            map.roofGrid.RoofGridUpdate();
        }

        /// <summary>
        /// Clean at a cell. Terrain & things
        /// </summary>
        private static void CleanAt(IntVec3 c, Map map, bool fullClean, Faction mapFaction, Faction player)
        {
            // Clean things
            var things = c.GetThingList(map);

            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is Thing thing && thing.Spawned)
                {
                    if (thing is Pawn p && p.Faction != mapFaction && p.Faction != player)
                    {
                        thing.DeSpawn();
                    }
                    else if (fullClean && (thing.def.category != ThingCategory.Building || !thing.def.building.isNaturalRock))
                    {
                        thing.DeSpawn();
                    }
                    // Clear filth, buildings, stone chunks
                    else if (thing.def.category == ThingCategory.Filth ||
                             (thing.def.category == ThingCategory.Building && !thing.def.building.isNaturalRock) ||
                             (thing.def.thingCategories != null && thing.def.thingCategories.Contains(ThingCategoryDefOf.StoneChunks)))
                    {
                        thing.DeSpawn();
                    }
                }
            }

            // Clean terrain
            if (map.terrainGrid.UnderTerrainAt(c) is TerrainDef terrain && terrain != null)
            {
                map.terrainGrid.SetTerrain(c, terrain);
            }

            // Clean roof
            if (fullClean)
                map.roofGrid.SetRoof(c, null);
        }

        /// <summary>
        /// Choose a random layout that match requirement(s) from a list.
        /// </summary>
        public static StructureLayoutDef ChooseStructureLayoutFrom(List<StructureLayoutDef> list)
        {
            List<StructureLayoutDef> choices = new List<StructureLayoutDef>();
            for (int i = 0; i < list.Count; i++)
            {
                var layout = list[i];
                if (layout.RequiredModLoaded)
                {
                    choices.Add(layout);
                }
            }
            return choices.RandomElement();
        }

        /// <summary>
        /// Choose a random WeightedStruct from a list.
        /// </summary>
        public static WeightedStruct ChooseWeightedStructFrom(List<WeightedStruct> list, IncidentParms parms)
        {
            List<WeightedStruct> choices = new List<WeightedStruct>();
            for (int i = 0; i < list.Count; i++)
            {
                var weightedStruct = list[i];
                if (weightedStruct.structureLayoutDef.RequiredModLoaded)
                {
                    choices.Add(weightedStruct);
                }
            }
            return choices.RandomElementByWeight((w) => w.weight * parms.points);
        }

        /// <summary>
        /// Get random stuff for wall
        /// </summary>
        public static ThingDef RandomWallStuffByWeight(ThingDef thingDef)
        {
            var option = GenOption.StuffableOptions;

            if (option.generalWallStuff && GenOption.generalWallStuff != null)
                return GenOption.generalWallStuff;

            if (option.allowedWallStuff.Count > 0)
            {
                return RandomStuffFromFor(option.allowedWallStuff, thingDef);
            }

            if (option.disallowedWallStuff.Count > 0)
            {
                var from = StartupActions.stuffs.FindAll(t => !option.disallowedWallStuff.Contains(t));
                return RandomStuffFromFor(from, thingDef);
            }

            return RandomStuffFromFor(StartupActions.stuffs, thingDef);
        }

        /// <summary>
        /// Get random stuff for wall, from symbol
        /// </summary>
        public static ThingDef RandomWallStuffByWeight(SymbolDef symbol)
        {
            if (GenOption.StuffableOptions != null && GenOption.StuffableOptions.randomizeWall && symbol.thingDef.MadeFromStuff)
                return RandomWallStuffByWeight(symbol.thingDef);

            return symbol.stuffDef;
        }

        /// <summary>
        /// Get random stuff for furniture
        /// </summary>
        public static ThingDef RandomFurnitureStuffByWeight(SymbolDef symbol)
        {
            if (!symbol.thingDef.MadeFromStuff)
                return null;

            var option = GenOption.StuffableOptions;

            if (option != null && option.randomizeFurniture && !option.excludedFunitureDefs.Contains(symbol.thingDef))
            {
                if (option.allowedFurnitureStuff.Count > 0)
                {
                    return RandomStuffFromFor(option.allowedFurnitureStuff, symbol.thingDef);
                }

                if (option.disallowedFurnitureStuff.Count > 0)
                {
                    var from = StartupActions.stuffs.FindAll(t => !option.disallowedFurnitureStuff.Contains(t));
                    return RandomStuffFromFor(from, symbol.thingDef);
                }

                return RandomStuffFromFor(StartupActions.stuffs, symbol.thingDef);
            }

            return symbol.stuffDef ?? symbol.thingDef.defaultStuff ?? ThingDefOf.WoodLog;
        }

        /// <summary>
        /// Get stuff by commonality from list, matching thingDef requirement
        /// </summary>
        public static ThingDef RandomStuffFromFor(List<ThingDef> from, ThingDef thingDef) => from.FindAll(t => thingDef.stuffCategories.Find(c => t.stuffProps.categories.Contains(c)) != null).RandomElementByWeight(t => t.stuffProps.commonality);

        /// <summary>
        /// Check a list to know if anything is near a given point
        /// </summary>
        public static bool NearUsedSpot(List<IntVec3> usedSpots, IntVec3 c, float dist)
        {
            if (usedSpots.NullOrEmpty())
                return false;

            for (int index = 0; index < usedSpots.Count; ++index)
            {
                if ((usedSpots[index] - c).LengthHorizontalSquared <= dist * dist)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Set terrain at pos if it's not a buildable, set bridge if needed.
        /// For road gen.
        /// </summary>
        public static void SetTerrainAt(IntVec3 c, Map map, TerrainDef roadDef)
        {
            if (GenOption.mineables.ContainsKey(c))
            {
                GenOption.DespawnMineableAt(c);
            }
            else
            {
                c.GetFirstMineable(map)?.DeSpawn();
            }

            if (map.terrainGrid.TerrainAt(c) is TerrainDef terrainDef)
            {
                if (!terrainDef.BuildableByPlayer)
                {
                    if (terrainDef.affordances.Contains(TerrainAffordanceDefOf.Bridgeable))
                        map.terrainGrid.SetTerrain(c, TerrainDefOf.Bridge);
                    else
                        map.terrainGrid.SetTerrain(c, roadDef);
                }
            }
        }

        /// <summary>
        /// Widen a given path
        /// </summary>
        public static void WidenPath(List<IntVec3> path, Map map, TerrainDef terrain, int width)
        {
            if (width <= 0 || path.NullOrEmpty())
                return;

            IntVec3 last = path[0];
            int count = path.Count;
            for (int c = 0; c < count; c++)
            {
                IntVec3 curr = path[c];
                if (curr.InBounds(map))
                {
                    int widenBy = 0;
                    for (int i = 1; i <= width && widenBy < width; i++)
                    {
                        if (last.x != curr.x)
                        {
                            var c1 = new IntVec3(curr.x, 0, curr.z + i);
                            if (c1.InBounds(map))
                            {
                                grid[c1.z][c1.x] = CellType.Used;
                                SetTerrainAt(c1, map, terrain);
                                widenBy++;
                            }

                            var c2 = new IntVec3(curr.x, 0, curr.z - i);
                            if (widenBy < width && c2.InBounds(map))
                            {
                                grid[c2.z][c2.x] = CellType.Used;
                                SetTerrainAt(c2, map, terrain);
                                widenBy++;
                            }
                        }

                        if (last.z != curr.z)
                        {
                            var c1 = new IntVec3(curr.x + i, 0, curr.z);
                            if (c1.InBounds(map))
                            {
                                grid[c1.z][c1.x] = CellType.Used;
                                SetTerrainAt(c1, map, terrain);
                                widenBy++;
                            }

                            var c2 = new IntVec3(curr.x - i, 0, curr.z);
                            if (widenBy < width && c2.InBounds(map))
                            {
                                grid[c2.z][c2.x] = CellType.Used;
                                SetTerrainAt(c2, map, terrain);
                                widenBy++;
                            }
                        }
                    }
                    last = curr;
                }
            }
        }

        /// <summary>
        /// Spawn main road props
        /// </summary>
        public static void SpawnMainRoadProps(List<IntVec3> road)
        {
            if (road.NullOrEmpty())
                return;

            var map = BaseGen.globalSettings.map;
            for (int i = 0; i < road.Count; i++)
            {
                IntVec3 curr = road[i];
                IntVec3 last = i - 1 > 0 ? road[i - 1] : curr;

                if (Rand.Chance(GenOption.PropsOptions.mainRoadPropsChance)
                    && !NearUsedSpot(GenOption.usedSpots, curr, GenOption.PropsOptions.mainRoadMinDistance))
                {
                    var rot = last.x != curr.x ? Rot4.East : Rot4.North;
                    var thingDef = GenOption.PropsOptions.RandomMainRoadProps();

                    if (thingDef != null)
                    {
                        var thing = ThingMaker.MakeThing(thingDef, GenStuff.DefaultStuffFor(thingDef));
                        if (thingDef.rotatable)
                            GenSpawn.Spawn(thing, curr, map, rot);
                        else
                            GenSpawn.Spawn(thing, curr, map);

                        GenOption.usedSpots.Add(curr);
                    }
                }
            }

        }

        /// <summary>
        /// Spawn link road props
        /// </summary>
        public static void SpawnLinkRoadProps(List<IntVec3> road)
        {
            if (road.NullOrEmpty())
                return;

            if (GenOption.usedSpots == null)
                GenOption.usedSpots = new List<IntVec3>();

            var map = BaseGen.globalSettings.map;
            for (int i = 0; i < road.Count; i++)
            {
                IntVec3 cell = road[i];

                if (Rand.Chance(GenOption.PropsOptions.linkRoadPropsChance))
                {
                    var thingDef = GenOption.PropsOptions.RandomLinkRoadProps();
                    if (thingDef == null)
                        return;

                    var cells = GenAdj.CellsAdjacent8Way(new TargetInfo(cell, map));

                    var matchingCells = new List<IntVec3>();
                    foreach (var c in cells)
                    {
                        if (grid[c.z][c.x] == CellType.Used)
                            continue;
                        if (NearUsedSpot(GenOption.usedSpots, c, GenOption.PropsOptions.linkRoadMinDistance))
                            continue;

                        var rect = new CellRect(c.x, c.z, thingDef.size.x, thingDef.size.z);
                        foreach (var ce in rect)
                        {
                            if (grid[ce.z][ce.x] == CellType.Used || !ce.Walkable(map))
                                continue;
                        }

                        matchingCells.Add(c);
                    }

                    if (matchingCells.Count > 0)
                    {
                        var thing = ThingMaker.MakeThing(thingDef, GenStuff.DefaultStuffFor(thingDef));
                        if (thingDef.rotatable)
                            GenSpawn.Spawn(thing, matchingCells.RandomElement(), map, Rot4.Random);
                        else
                            GenSpawn.Spawn(thing, matchingCells.RandomElement(), map);

                        GenOption.usedSpots.Add(cell);
                    }
                }
            }
        }
    }
}