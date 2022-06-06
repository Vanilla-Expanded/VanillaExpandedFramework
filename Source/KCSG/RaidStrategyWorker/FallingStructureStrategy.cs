using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace KCSG
{
    internal class FallingStructureStrategy : RaidStrategyWorker_Siege
    {
        public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            CGO.fallingStructure = def.GetModExtension<FallingStructure>();

            if (CGO.fallingStructure.needToHaveSettlements && !Find.World.worldObjects.Settlements.FindAll(s => s.Faction == parms.faction).Any())
                return false;

            if (CGO.fallingStructure.canBeUsedBy.Contains(parms.faction.def))
            {
                CGO.fallingStructureChoosen = LayoutUtils.ChooseWeightedStruct(CGO.fallingStructure.WeightedStructs, parms).structureLayoutDef;

                if (CGO.fallingStructureChoosen != null)
                {
                    return parms.points >= MinimumPoints(parms.faction, groupKind) && FindRect((Map)parms.target,
                                                                                               CGO.fallingStructureChoosen.height,
                                                                                               CGO.fallingStructureChoosen.width) != IntVec3.Invalid;
                }
                else return false;
            }
            else return false;
        }

        public IntVec3 FindRect(Map map, int height, int width)
        {
            int maxSize = Math.Max(width, height);
            for (int tries = 0; tries < 100; tries++)
            {
                CellRect rect = CellRect.CenteredOn(CellFinder.RandomNotEdgeCell(maxSize, map), width, height);
                if (rect.Cells.ToList().Any(i => !i.Walkable(map) || !i.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Medium)))
                    continue;
                else
                    return rect.CenterCell;
            }
            return CellFinder.RandomNotEdgeCell(maxSize, map);
        }

        public override List<Pawn> SpawnThreats(IncidentParms parms)
        {
            CellRect cellRect = CellRect.CenteredOn(parms.spawnCenter, CGO.fallingStructureChoosen.width, CGO.fallingStructureChoosen.height);

            List<string> allSymbList = new List<string>();
            Map map = (Map)parms.target;

            foreach (string str in CGO.fallingStructureChoosen.layouts[0])
            {
                List<string> symbSplitFromLine = str.Split(',').ToList();
                symbSplitFromLine.ForEach((s) => allSymbList.Add(s));
            }

            List<TTIR> fallers = new List<TTIR>();
            Dictionary<ActiveDropPodInfo, IntVec3> pods = new Dictionary<ActiveDropPodInfo, IntVec3>();

            int l = 0;
            foreach (IntVec3 cell in cellRect.Cells)
            {
                if (l < allSymbList.Count && allSymbList[l] != ".")
                {
                    SymbolDef temp = DefDatabase<SymbolDef>.GetNamed(allSymbList[l], false);
                    Thing thing;

                    if (temp.thingDef != null && !CGO.fallingStructure.thingsToSpawnInDropPod.Contains(temp.thingDef))
                    {
                        TTIR ttir = new TTIR();

                        thing = ThingMaker.MakeThing(temp.thingDef, temp.stuffDef);
                        thing.SetFactionDirect(parms.faction);
                        if (!CGO.fallingStructure.spawnDormantWhenPossible && thing.TryGetComp<CompCanBeDormant>() is CompCanBeDormant ccbd && ccbd != null)
                        {
                            ccbd.wakeUpOnTick = Find.TickManager.TicksGame + 150;
                        }

                        if (thing.def.rotatable && thing.def.category == ThingCategory.Building)
                        {
                            ttir.rot = new Rot4(temp.rotation.AsInt);
                        }

                        ThingDef faller = new ThingDef
                        {
                            thingClass = CGO.fallingStructure.skyfaller,
                            category = ThingCategory.Ethereal,
                            useHitPoints = false,

                            drawOffscreen = true,
                            tickerType = TickerType.Normal,
                            altitudeLayer = AltitudeLayer.Skyfaller,
                            drawerType = DrawerType.RealtimeOnly,

                            defName = temp.thingDef.defName,
                            label = temp.thingDef.label + " (incoming)",
                            size = new IntVec2(thing.def.size.x, thing.def.size.z)
                        };

                        faller.skyfaller = new SkyfallerProperties()
                        {
                            shadowSize = new UnityEngine.Vector2(thing.def.size.x + 1, thing.def.size.z + 1),
                            ticksToImpactRange = new IntRange(150, 150),
                            movementType = SkyfallerMovementType.Decelerate
                        };

                        ttir.faller = faller;
                        ttir.toSpawn = thing;
                        ttir.cell = cell;

                        fallers.Add(ttir);
                    }
                    else if (temp.thingDef != null)
                    {
                        thing = ThingMaker.MakeThing(temp.thingDef, temp.stuffDef);
                        thing.SetFactionDirect(parms.faction);

                        ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
                        activeDropPodInfo.innerContainer.TryAdd(thing);
                        activeDropPodInfo.openDelay = 40;
                        activeDropPodInfo.leaveSlag = false;
                        pods.Add(activeDropPodInfo, cell);
                    }
                }
                l++;
            }
            // ARRIVAL
            fallers.ForEach(ttir => SpawnSkyfaller(ttir.faller, ttir.toSpawn, ttir.cell, map, ttir.rot));
            for (int i = 0; i < pods.Count; i++)
            {
                DropPodUtility.MakeDropPodAt(pods.ElementAt(i).Value, map, pods.ElementAt(i).Key);
            }

            IncidentParms parms1 = parms;
            RCellFinder.TryFindRandomCellNearWith(parms.spawnCenter, i => i.Walkable(map), map, out parms1.spawnCenter, 33, 40);

            base.SpawnThreats(parms1);
            CGO.ClearFalling();
            return null;
        }

        protected override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
        {
            IntVec3 originCell = parms.spawnCenter.IsValid ? parms.spawnCenter : pawns[0].PositionHeld;
            if (parms.faction.HostileTo(Faction.OfPlayer))
            {
                return new LordJob_AssaultColony(parms.faction, true, true, false, false, true);
            }
            RCellFinder.TryFindRandomSpotJustOutsideColony(originCell, map, out IntVec3 fallbackLocation);
            return new LordJob_AssistColony(parms.faction, fallbackLocation);
        }

        private KCSG_Skyfaller SpawnSkyfaller(ThingDef skyfaller, Thing innerThing, IntVec3 pos, Map map, Rot4 rot)
        {
            KCSG_Skyfaller faller = (KCSG_Skyfaller)SkyfallerMaker.MakeSkyfaller(skyfaller);
            if (innerThing != null)
            {
                if (!faller.innerContainer.TryAdd(innerThing, true))
                {
                    Log.Error($"Could not add {innerThing.ToStringSafe()} to a KCSG_Skyfaller.");
                    innerThing.Destroy(DestroyMode.Vanish);
                }
            }
            faller.rot = rot;

            return (KCSG_Skyfaller)GenSpawn.Spawn(faller, pos, map, rot, WipeMode.Vanish);
        }
    }
}