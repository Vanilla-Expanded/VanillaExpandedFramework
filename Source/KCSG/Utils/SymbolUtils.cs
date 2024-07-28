using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace KCSG
{
    public static class SymbolUtils
    {
        /// <summary>
        /// Generate symbol at cell
        /// </summary>
        public static void Generate(this SymbolDef symbol, StructureLayoutDef layout, Map map, IntVec3 cell, Faction faction, ThingDef wallForRoom)
        {
            if (symbol.pawnKindDefNS != null)
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
                else if (symbol.thingDef.category == ThingCategory.Pawn && GenOption.customGenExt?.symbolResolvers == null)
                {
                    GenSpawn.Spawn(symbol.thingDef, cell, map, WipeMode.VanishOrMoveAside);
                }
                else
                {
                    if (GenOption.GetMineableAt(cell) != null && symbol.thingDef.designationCategory == AllDefOf.Security)
                    {
                        return;
                    }
                    GenerateBuildingAt(map, cell, symbol, layout, faction, wallForRoom);

                    // Generating settlement, we want to keep tracks of doors
                    if (GenOption.customGenExt != null && !GenOption.customGenExt.UsingSingleLayout && symbol.thingDef.altitudeLayer == AltitudeLayer.DoorMoveable)
                    {
                        SettlementGenUtils.doors?.Add(cell);
                    }
                }
            }
        }

        /// <summary>
        /// Generate pawn(s) at pos
        /// </summary>
        private static void GeneratePawnAt(Map map, IntVec3 cell, SymbolDef symbol)
        {
            var manager = Find.FactionManager;
            var mapFac = map.ParentFaction;

            var symbolFac = symbol.faction != null ? manager.FirstFactionOfDef(symbol.faction) : null;
            var slaveFac = mapFac == null ? RandomUtils.RandomNonColonyFaction() : mapFac.RandomNonColonyEnnemy();

            var request = new PawnGenerationRequest(symbol.pawnKindDefNS, symbol.isSlave ? slaveFac : (symbol.spawnPartOfFaction ? mapFac : symbolFac), mustBeCapableOfViolence: true);

            var pawns = new List<Pawn>();
            for (int i = 0; i < symbol.numberToSpawn; i++)
            {
                var pawn = PawnGenerator.GeneratePawn(request);
                if (pawn == null)
                {
                    Debug.Error("Null pawn in GeneratePawnAt");
                    continue;
                }

                if (symbol.isSlave && mapFac != null)
                {
                    pawn.guest.SetGuestStatus(mapFac, GuestStatus.Prisoner);
                }

                if (symbol.spawnDead)
                {
                    var corpse = PreparePawnCorpse(pawn, symbol.spawnRotten);
                    if (symbol.spawnFilthAround)
                    {
                        for (int x = 0; x < 5; x++)
                        {
                            IntVec3 rNext = new IntVec3();
                            RCellFinder.TryFindRandomCellNearWith(cell, ni => ni.Walkable(map), map, out rNext, 1, 3);
                            GenSpawn.Spawn(ThingDefOf.Filth_CorpseBile, rNext, map);
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
                Lord lord = LordMaker.MakeNewLord(mapFac, new LordJob_DefendPoint(cell, 3f, addFleeToil: false), map, pawns);
            }
        }

        /// <summary>
        /// Prepare corpse spawn
        /// </summary>
        private static Corpse PreparePawnCorpse(Pawn pawn, bool rot)
        {
            // Random damage to worn apparels
            if (pawn.apparel != null)
            {
                var apparels = pawn.apparel.WornApparel;
                for (int a = 0; a < apparels.Count; a++)
                {
                    var apparel = apparels[a];
                    apparel.HitPoints = Rand.Range(1, (int)(apparel.MaxHitPoints * 0.75));
                }
            }
            // Random damage to equipement
            if (pawn.equipment != null && pawn.equipment.Primary is ThingWithComps p)
            {
                p.HitPoints = Rand.Range(1, (int)(p.MaxHitPoints * 0.75));
            }
            // Remove all rottable things (food...)
            if (pawn.inventory != null)
            {
                var inv = pawn.inventory.GetDirectlyHeldThings();
                foreach (var item in inv)
                {
                    if (item.TryGetComp<CompRottable>() != null)
                    {
                        inv.Remove(item);
                    }
                }
            }

            pawn.Kill(null);
            var corpse = pawn.Corpse;
            if (rot)
            {
                corpse.timeOfDeath = Mathf.Max(Find.TickManager.TicksGame - 120000, 0);
                GenOption.corpsesToRot.Add(corpse);
            }

            return corpse;
        }

        /// <summary>
        /// Generate item at pos
        /// </summary>
        private static void GenerateItemAt(Map map, IntVec3 cell, SymbolDef symbol)
        {
            var thing = ThingMaker.MakeThing(symbol.thingDef, symbol.stuffDef ?? (symbol.thingDef.stuffCategories?.Count > 0 ? GenStuff.RandomStuffFor(symbol.thingDef) : null));
            thing.stackCount = symbol.maxStackSize != -1 ? Rand.RangeInclusive(1, symbol.maxStackSize) : Mathf.Clamp(Rand.RangeInclusive(1, symbol.thingDef.stackLimit), 1, 75);
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
                thing = ThingMaker.MakeThing(symbol.thingDef, symbol.thingDef.MadeFromStuff ? wallStuff ?? RandomUtils.RandomWallStuffWeighted(symbol) : null);
            }
            else
            {
                thing = ThingMaker.MakeThing(symbol.thingDef, RandomUtils.RandomFurnitureStuffWeighted(symbol));
            }
            // Sanity check
            if (thing == null)
                return;
            // If ideology is loaded, try to apply the right style
            if (ModsConfig.IdeologyActive && faction != null && faction.ideos != null && faction.ideos.PrimaryIdeo is Ideo p)
            {
                thing.SetStyleDef(p.GetStyleFor(thing.def));
            }
            // Try coloring it
            if (thing.def.building != null && thing.def.building.paintable && symbol.colorDef != null && thing is Building building)
            {
                building.ChangePaint(symbol.colorDef);
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
                var corpse = PreparePawnCorpse(pawn, true);

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
                //else
                //{
                //    map.terrainGrid.SetTerrain(cell, TerrainDefOf.Bridge);
                //}
            }
            // Spawn the thing
            GenSpawn.Spawn(thing, cell, map, symbol.rotation, WipeMode.VanishOrMoveAside);
            // Set the faction if applicable
            if (symbol.spawnPartOfFaction && faction != null && thing.def.CanHaveFaction)
            {
                thing.SetFaction(faction);
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
                    c.SetFaction(faction);
                }
                GenSpawn.Spawn(c, cell, map, WipeMode.FullRefund);
            }
            // Try to fill shelves
            if (thing is Building_Storage storage
                && GenOption.settlementLayout != null
                && GenOption.settlementLayout.stockpileOptions.fillStorageBuildings
                && !GenOption.settlementLayout.stockpileOptions.fillWithDefs.NullOrEmpty())
            {
                var marketValue = GenOption.settlementLayout.stockpileOptions.RefMarketValue;
                foreach (var storageCell in storage.AllSlotCells())
                {
                    var otherThing = storageCell.GetFirstItem(map);
                    if (Rand.Value < GenOption.settlementLayout.stockpileOptions.fillChance && GenOption.settlementLayout.stockpileOptions.replaceOtherThings || otherThing == null)
                    {
                        if (GenOption.settlementLayout.stockpileOptions.replaceOtherThings && otherThing.Spawned)
                            otherThing.DeSpawn();

                        var thingDef = GenOption.settlementLayout.stockpileOptions.fillWithDefs.RandomElementByWeight(t => marketValue - t.BaseMarketValue);
                        var item = ThingMaker.MakeThing(thingDef, thingDef.stuffCategories?.Count > 0 ? GenStuff.RandomStuffFor(thingDef) : null);

                        if (item.MarketValue <= GenOption.settlementLayout.stockpileOptions.maxValueStackIncrease)
                            item.stackCount = Mathf.Clamp(Rand.RangeInclusive(1, thingDef.stackLimit), 1, 90);

                        item.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityBaseGen(), ArtGenerationContext.Outsider);

                        GenPlace.TryPlaceThing(item, storageCell, map, ThingPlaceMode.Direct);
                        item.SetForbidden(true, false);
                    }
                }
            }
            // Handle styling
            if (symbol.styleCategoryDef != null && symbol.styleCategoryDef.GetStyleForThingDef(thing.def) is ThingStyleDef styleDef)
            {
                thing.SetStyleDef(styleDef);
            }
            // Handle mortar and mortar pawns
            SpawnMortar(thing, faction, map);
        }

        /// <summary>
        /// Generate pawn to be put inside container (casked, tomb...)
        /// </summary>
        private static Pawn GeneratePawnForContainer(SymbolDef temp, Map map)
        {
            var faction = temp.spawnPartOfFaction ? map.ParentFaction : null;
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
        /// Spawn mortar manning pawn with the right job
        /// </summary>
        private static void SpawnMortar(Thing thing, Faction faction, Map map)
        {
            // Prevent spawning new colonists
            if (faction == Faction.OfPlayer)
                return;

            if (thing?.def?.building?.buildingTags?.Count > 0)
            {
                if (thing.def.building.IsMortar && thing.def.building.buildingTags.Contains("Artillery_MannedMortar") && thing.def.HasComp(typeof(CompMannable)) && faction != null && faction.RandomPawnKind() is PawnKindDef pawnKind)
                {
                    // Spawn pawn
                    var request = new PawnGenerationRequest(pawnKind, faction, PawnGenerationContext.NonPlayer, map.Tile, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: true, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: true);
                    var singlePawnLord = LordMaker.MakeNewLord(faction, new LordJob_ManTurrets(), map);
                    var rpPawn = new ResolveParams
                    {
                        faction = faction,
                        singlePawnGenerationRequest = request,
                        rect = CellRect.SingleCell(thing.InteractionCell),
                        singlePawnLord = singlePawnLord
                    };
                    BaseGen.symbolStack.Push("pawn", rpPawn);

                    // Spawn shells
                    ThingDef shellDef = TurretGunUtility.TryFindRandomShellDef(thing.def, false, true, true, faction.def.techLevel, false, 250f);
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
    }
}