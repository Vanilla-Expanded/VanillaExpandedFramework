using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace KCSG
{
    internal class FallingStructureStrategy : RaidStrategyWorker_ImmediateAttack
    {
        /// <summary>
        /// Check if map and faction are valid for falling structure
        /// </summary>
        public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            GenOption.fExt = def.GetModExtension<FallingStructure>();

            if (!GenOption.fExt.canBeUsedBy.Contains(parms.faction.def))
                return false;

            if (GenOption.fExt.needToHaveSettlements && !Find.World.worldObjects.Settlements.FindAll(s => s.Faction == parms.faction).Any())
                return false;

            GenOption.fDef = GenUtils.ChooseWeightedStructFrom(GenOption.fExt.structures, parms);

            if (GenOption.fDef == null)
                return false;

            return base.CanUseWith(parms, groupKind) && FindRect((Map)parms.target, GenOption.fDef.height, GenOption.fDef.width) != IntVec3.Invalid;
        }

        /// <summary>
        /// Find a valid spawn rect on the map
        /// </summary>
        public IntVec3 FindRect(Map map, int height, int width)
        {
            int edgeDist = Math.Max(width, height);
            for (int t = 0; t < 100; t++)
            {
                CellRect rect = CellRect.CenteredOn(CellFinder.RandomNotEdgeCell(edgeDist, map), width, height);

                bool valid = true;
                foreach (var i in rect.Cells)
                {
                    if (!i.Walkable(map) || !i.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Medium))
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                    return rect.CenterCell;
            }

            return IntVec3.Invalid;
        }

        /// <summary>
        /// Spawn pawns and structure
        /// </summary>
        public override List<Pawn> SpawnThreats(IncidentParms parms)
        {
            var cellRect = CellRect.CenteredOn(parms.spawnCenter, GenOption.fDef.width, GenOption.fDef.height);

            var allSymbList = new List<string>();
            Map map = (Map)parms.target;

            for (int i = 0; i < GenOption.fDef.layouts[0].Count; i++)
            {
                var str = GenOption.fDef.layouts[0][i];
                var split = str.Split(',');
                for (int s = 0; s < split.Count(); s++)
                {
                    allSymbList.Add(split[s]);
                }
            }

            var fallers = new Dictionary<KCSG_Skyfaller, IntVec3>();
            var pods = new Dictionary<ActiveDropPodInfo, IntVec3>();

            int l = 0;
            int count = allSymbList.Count;
            foreach (IntVec3 cell in cellRect.Cells)
            {
                if (l < count && allSymbList[l] != ".")
                {
                    var temp = DefDatabase<SymbolDef>.GetNamed(allSymbList[l], false);
                    if (temp.thingDef == null)
                        continue;

                    if (GenOption.fExt.thingsToSpawnInDropPod.Contains(temp.thingDef))
                    {
                        var thing = ThingMaker.MakeThing(temp.thingDef, temp.stuffDef ?? GenStuff.RandomStuffFor(temp.thingDef));
                        thing.SetFactionDirect(parms.faction);

                        var info = new ActiveDropPodInfo
                        {
                            openDelay = 40,
                            leaveSlag = false
                        };
                        info.innerContainer.TryAdd(thing);
                        pods.Add(info, cell);
                    }
                    else
                    {
                        var thing = ThingMaker.MakeThing(temp.thingDef, temp.stuffDef ?? GenStuff.RandomStuffFor(temp.thingDef));
                        thing.SetFactionDirect(parms.faction);

                        if (!GenOption.fExt.spawnDormantWhenPossible && thing.TryGetComp<CompCanBeDormant>() is CompCanBeDormant ccbd && ccbd != null)
                        {
                            ccbd.wakeUpOnTick = Find.TickManager.TicksGame + 150;
                        }

                        ThingDef def = new ThingDef
                        {
                            thingClass = GenOption.fExt.skyfaller,
                            category = ThingCategory.Ethereal,
                            useHitPoints = false,

                            drawOffscreen = true,
                            tickerType = TickerType.Normal,
                            altitudeLayer = AltitudeLayer.Skyfaller,
                            drawerType = DrawerType.RealtimeOnly,

                            defName = temp.thingDef.defName,
                            label = temp.thingDef.label + " (incoming)",
                            size = new IntVec2(thing.def.size.x, thing.def.size.z),
                            skyfaller = new SkyfallerProperties()
                            {
                                shadowSize = new UnityEngine.Vector2(thing.def.size.x + 1, thing.def.size.z + 1),
                                ticksToImpactRange = new IntRange(150, 150),
                                movementType = SkyfallerMovementType.Decelerate
                            }
                        };

                        var faller = (KCSG_Skyfaller)SkyfallerMaker.MakeSkyfaller(def);
                        if (thing != null)
                        {
                            if (!faller.innerContainer.TryAdd(thing, true))
                            {
                                Log.Error($"Could not add {thing.ToStringSafe()} to a KCSG_Skyfaller.");
                                thing.Destroy(DestroyMode.Vanish);
                            }
                        }
                        faller.rot = temp.rotation != null ? temp.rotation : Rot4.North;
                        fallers.Add(faller, cell);
                    }
                }
                l++;
            }

            // Arrival
            for (int i = 0; i < fallers.Count; i++)
            {
                var f = fallers.ElementAt(i);
                GenSpawn.Spawn(f.Key, f.Value, map, f.Key.rot, WipeMode.Vanish);
            }

            for (int i = 0; i < pods.Count; i++)
            {
                var kvp = pods.ElementAt(i);
                DropPodUtility.MakeDropPodAt(kvp.Value, map, kvp.Key);
            }

            IncidentParms nParms = parms;
            RCellFinder.TryFindRandomCellNearWith(parms.spawnCenter, i => i.Walkable(map), map, out nParms.spawnCenter, Math.Min(GenOption.fDef.width, GenOption.fDef.height));

            return base.SpawnThreats(nParms);
        }
    }
}