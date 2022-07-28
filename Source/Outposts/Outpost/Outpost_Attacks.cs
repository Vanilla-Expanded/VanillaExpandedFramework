using System.Linq;
using System.Collections.Generic;
using System.Text;
using KCSG;
using RimWorld;
using RimWorld.Planet;
using Verse.AI.Group;
using Verse;

namespace Outposts
{
    public partial class Outpost
    {

        public override MapGeneratorDef MapGeneratorDef
        {
            get
            {
                if (def.GetModExtension<CustomGenOption>() is { } cGen && (cGen.chooseFromlayouts.Count > 0 || cGen.chooseFromSettlements.Count > 0))
                    return DefDatabase<MapGeneratorDef>.GetNamed("KCSG_WorldObject_Gen");
                return MapGeneratorDefOf.Base_Faction;
            }
        }
        
        public override void PostMapGenerate()
        {
            base.PostMapGenerate();

            foreach (var pawn in Map.mapPawns.AllPawns.Where(p => p.RaceProps.Humanlike || p.HostileTo(Faction)).ToList()) pawn.Destroy();

            foreach (var occupant in occupants)
            {
                GenPlace.TryPlaceThing(occupant, Map.Center, Map, ThingPlaceMode.Near);
                if (occupant.Position.Fogged(Map))
                {
                    FloodFillerFog.FloodUnfog(occupant.Position, Map);
                }
            }

        }
        public virtual void MapClearAndReset()
        {
            occupants.Clear();
            var pawns = Map.mapPawns.AllPawns.ListFullCopy();
            foreach (var pawn in pawns)
            {
                if (pawn.Faction is { IsPlayer: true } || pawn.HostFaction is { IsPlayer: true })
                {
                    pawn.DeSpawn();
                    occupants.Add(pawn);
                }
            }
            RecachePawnTraits();
            raidFaction = null;
            raidPoints = 0;
        }
        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            if (!Map.mapPawns.FreeColonists.Any())
            {
                occupants.Clear();
                Find.LetterStack.ReceiveLetter("Outposts.Letters.Lost.Label".Translate(), "Outposts.Letters.Lost.Text".Translate(Name),
                    LetterDefOf.NegativeEvent);
                alsoRemoveWorldObject = true;
                return true;
            }

            var pawns = Map.mapPawns.AllPawns.ListFullCopy();
            if (!pawns.Any(p => p.Faction == raidFaction && !p.Downed )) 
            {
                //This is what's required for gauntlet figured i'd put it after the initial if so we're not checking bunch of extra things per tick
                if (Map.listerThings.ThingsInGroup(ThingRequestGroup.ThingHolder).Any(x => x is Skyfaller)) 
                {
                    foreach (var thing in Map.listerThings.ThingsInGroup(ThingRequestGroup.ThingHolder).Where(x => x is Skyfaller))
                    {
                        var skyfaller = thing as Skyfaller;
                        if (skyfaller.Faction == raidFaction || skyfaller.innerContainer.Any(x => x.Faction == raidFaction))
                        {
                            alsoRemoveWorldObject = false;
                            return false;
                        }
                    }
                }
                if (Map.listerBuildings.allBuildingsNonColonist.Any(x => x.Faction == raidFaction)) //I think this would make it so you have to destroy hives as well for something like AA blackhvie raid
                {
                    alsoRemoveWorldObject = false;
                    return false;
                }
                occupants.Clear();
                foreach (var pawn in pawns)
                {
                    if (pawn.Faction is { IsPlayer: true } || pawn.HostFaction is { IsPlayer: true })
                    {
                        pawn.DeSpawn();
                        occupants.Add(pawn);
                    }
                }
                AddLoot(raidFaction,raidPoints,Map,out var loot);
                Find.LetterStack.ReceiveLetter("Outposts.Letters.BattleWon.Label".Translate(), "Outposts.Letters.BattleWon.Text".Translate(Name, loot),
                LetterDefOf.PositiveEvent,
                new LookTargets(Gen.YieldSingle(this)));

                RecachePawnTraits();
                raidFaction = null;
                raidPoints = 0;
                alsoRemoveWorldObject = false;
                return true;
            }

            alsoRemoveWorldObject = false;
            return false;
        }
        
        public virtual void AddLoot(Faction raidFaction,float raidPoints,Map map, out string letter)//made these passed for benefit of ambush
        {
            //looking at this if a colonist gets downed and dropped their weapon would they not lose their weapon?
            //Also can get raider's weapon
            letter = null;
            StringBuilder sb = new StringBuilder();
            float mv = 0f;
            foreach (Thing thing in map.listerThings.ThingsInGroup(ThingRequestGroup.Weapon).ToList())
            {
                if (!containedItems.Contains(thing) && !thing.Position.Fogged(map)) //In case ancient dangers are possible in these maps
                {
                    mv += thing.MarketValue;
                    thing.DeSpawn();
                    containedItems.Add(thing);
                }
            }
            //Rescue colonist corpses. Cant let those funeral opportunities go to waste
            foreach (Corpse corpse in map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse).ToList())
            {
                if (corpse.InnerPawn?.Faction?.IsPlayer ?? false)
                {
                    sb.AppendLine("Outposts.Letters.BattleWon.Rescued".Translate(corpse.InnerPawn.NameFullColored));
                    corpse.DeSpawn();
                    containedItems.Add(corpse);
                    continue;
                }
                //NonUnoPinata means no weapons trying to safely add compat because i use it XD
                if (!corpse.InnerPawn?.equipment?.AllEquipmentListForReading.NullOrEmpty() ?? false)
                {
                    foreach (var thing in corpse.InnerPawn.equipment.AllEquipmentListForReading.ToList())
                    {
                        if (thing.def.IsWeapon)
                        {
                            corpse.InnerPawn.equipment.TryDropEquipment(thing, out var equipment, corpse.Position, false);
                            mv += equipment.MarketValue;
                            equipment.DeSpawn();
                            containedItems.Add(equipment);
                        }
                    }
                }
            }
            foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned.Where(x => x.Faction == raidFaction && x.Downed).ToList())
            {
                if (Rand.Chance(0.33f) && !pawn.Dead && pawn.RaceProps.Humanlike)
                {
                    sb.AppendLine("Outposts.Letters.BattleWon.Captured".Translate(pawn.NameFullColored));
                    pawn.guest.CapturedBy(Faction);
                    pawn.DeSpawn();
                    AddPawn(pawn);
                }
            }
            //I wanted loot
            if (raidFaction.def.raidLootMaker != null)
            {
                float raidLootPoints = raidPoints * Find.Storyteller.difficulty.EffectiveRaidLootPointsFactor;
                float num = raidFaction.def.raidLootValueFromPointsCurve.Evaluate(raidLootPoints);
                ThingSetMakerParams parms2 = default(ThingSetMakerParams);
                parms2.totalMarketValueRange = new FloatRange(num, num);
                parms2.makingFaction = raidFaction;
                List<Thing> loot = raidFaction.def.raidLootMaker.root.Generate(parms2);                
                foreach (Thing thing in loot)
                {
                    mv += thing.MarketValue;
                    AddItem(thing);
                }                
            }
            if (mv > 0)
            {
                sb.AppendLine("Outposts.Letters.BattleWon.Secured".Translate(mv.ToStringMoney()));
            }
            if (sb.Length > 0)
            {
                letter = sb.ToString(); 
            }
        }
    }
}