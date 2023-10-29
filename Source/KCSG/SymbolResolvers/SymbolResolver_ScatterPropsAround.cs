using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using Verse;
using static KCSG.SettlementGenUtils;

namespace KCSG
{
    internal class SymbolResolver_ScatterPropsAround : SymbolResolver
    {
        private List<IntVec3> usedSpots = new List<IntVec3>();

        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;
            var propsOpt = GenOption.PropsOptions;
            var faction = rp.faction;

            if (propsOpt.scatterProps && propsOpt.scatterPropsDefs.Count > 0)
            {
                usedSpots = new List<IntVec3>();
                var usables = propsOpt.scatterPropsDefs.ListFullCopy();

                if (ModsConfig.IdeologyActive && faction != null && faction.ideos != null && faction.ideos.PrimaryIdeo is Ideo p && p.IdeoApprovesOfSlavery())
                {
                    var tBuildings = DefDatabase<ThingDef>.AllDefs.Where(def => def.StatBaseDefined(StatDefOf.TerrorSource)
                                                                                && (typeof(Building_Casket).IsAssignableFrom(def.thingClass)
                                                                                    || p.cachedPossibleBuildings.Any(b => b.ThingDef == def)));
                    usables.AddRange(tBuildings);
                }

                var thingUsed = new List<ThingDef>();
                var propsStart = DateTime.Now;
                for (int i = 0; i < propsOpt.scatterMaxAmount; i++)
                {
                    if (thingUsed.Count == usables.Count)
                        thingUsed.Clear();

                    var prop = usables.FindAll(t => !thingUsed.Contains(t)).RandomElement();
                    thingUsed.Add(prop);

                    Rot4 rot = Rot4.North;

                    if (prop.rotatable && Rand.Bool)
                        rot = Rot4.East;

                    if (RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith(c =>
                    {
                        if (!rp.rect.Contains(c))
                            return false;

                        if (NearUsedSpot(usedSpots, c, propsOpt.scatterMinDistance))
                            return false;

                        CellRect rect;
                        if (rot == Rot4.North)
                            rect = new CellRect(c.x, c.z, prop.size.x, prop.size.z);
                        else
                            rect = new CellRect(c.x, c.z, prop.size.z, prop.size.x);

                        foreach (var ce in rect)
                        {
                            if (grid[ce.z][ce.x] == CellType.Used || !ce.Walkable(map))
                                return false;
                        }

                        return true;
                    }, map, out IntVec3 cell))
                    {
                        Thing thing = ThingMaker.MakeThing(prop, faction != null ? BaseGenUtility.CheapStuffFor(prop, faction) : GenStuff.DefaultStuffFor(prop));
                        thing.SetFaction(map.ParentFaction);

                        if (thing is Building_Casket casket)
                        {
                            var otherFac = Find.FactionManager.GetFactions(allowNonHumanlike: false).Where(f => !f.IsPlayer && f.HostileTo(rp.faction)).RandomElementWithFallback(rp.faction);
                            var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Slave, otherFac, allowFood: false));
                            pawn.Kill(null);
                            casket.TryAcceptThing(pawn.Corpse);
                        }

                        if (prop.rotatable)
                            GenSpawn.Spawn(thing, cell, map, rot);
                        else
                            GenSpawn.Spawn(thing, cell, map);

                        usedSpots.Add(cell);
                    }
                }
                Debug.Message($"Props spawning time: {(DateTime.Now - propsStart).TotalMilliseconds}ms.");
            }

            Debug.Message($"Total time (without pawn gen): {(DateTime.Now - startTime).TotalSeconds}s.");
        }
    }
}