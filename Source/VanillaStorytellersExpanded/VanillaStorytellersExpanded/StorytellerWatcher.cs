using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace VanillaStorytellersExpanded
{
    public class RaidQueue : IExposable
    {
        public IncidentDef incidentDef;
        public IncidentParms parms;
        public int tickToFire;
        public RaidQueue()
        {

        }

        public RaidQueue(IncidentDef incidentDef, IncidentParms parms, int tickToFire)
        {
            this.incidentDef = incidentDef;
            this.parms = parms;
            this.tickToFire = tickToFire;
        }
        public void ExposeData()
        {
            Scribe_Defs.Look(ref incidentDef, "incidentDef");
            Scribe_Deep.Look(ref parms, "parms");
            Scribe_Values.Look(ref tickToFire, "tickToFire");
        }
    }
	public class StorytellerWatcher : GameComponent
	{
        public int lastRaidExpansionTicks;

        public StorytellerDef currentStoryteller;

        public Dictionary<FactionDef, IntRange> originalNaturalGoodwillValues;

        public List<RaidGroup> raidGroups;
        public List<RaidGroup> reinforcementGroups;
        public List<RaidQueue> raidQueues;
        public StorytellerWatcher()
        {
        }

        public StorytellerWatcher(Game game)
        {

        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                CheckStorytellerChanges();
            }
            if (this.raidQueues?.Count > 0)
            {
                for (int num = raidQueues.Count - 1; num >= 0; num--)
                {
                    if (Find.TickManager.TicksAbs >= raidQueues[num].tickToFire)
                    {
                        raidQueues[num].incidentDef.Worker.TryExecute(raidQueues[num].parms);
                        raidQueues.RemoveAt(num);
                    }
                }
            }
        }

        public void PreInit()
        {
            if (this.raidGroups == null) this.raidGroups = new List<RaidGroup>();
            if (this.reinforcementGroups == null) this.reinforcementGroups = new List<RaidGroup>();
            if (this.originalNaturalGoodwillValues == null) this.originalNaturalGoodwillValues = new Dictionary<FactionDef, IntRange>();
            if (this.raidQueues == null) this.raidQueues = new List<RaidQueue>();
        }
        public override void LoadedGame()
        {
            base.LoadedGame();
            this.PreInit();
            ChangeOrRestoreNaturalGoodwill();
        }
        public override void StartedNewGame()
        {
            base.StartedNewGame();
            this.PreInit();
            ChangeOrRestoreNaturalGoodwill();
        }

        public void CheckStorytellerChanges()
        {
            if (currentStoryteller != Find.Storyteller.def)
            {
                currentStoryteller = Find.Storyteller.def;
                ChangeOrRestoreNaturalGoodwill();
            }
        }

        public void ChangeOrRestoreNaturalGoodwill()
        {
            if (Find.Storyteller.def.HasModExtension<StorytellerDefExtension>())
            {
                var options = Find.Storyteller.def.GetModExtension<StorytellerDefExtension>();
                if (options.storytellerThreat != null)
                {
                    ChangeNaturalGoodwill(options.storytellerThreat);
                }
                else
                {
                    RestoreNaturalGoodwillForAllFactions();
                }
            }
            else
            {
                RestoreNaturalGoodwillForAllFactions();
            }
        }

        public void ChangeNaturalGoodwill(StorytellerThreat storytellerThreat)
        {
            RestoreNaturalGoodwillForAllFactions();
            foreach (var factionDef in DefDatabase<FactionDef>.AllDefs)
            {
                if (factionDef != Faction.OfPlayer.def && !factionDef.permanentEnemy)
                {
                    originalNaturalGoodwillValues[factionDef] = factionDef.naturalColonyGoodwill;
                    factionDef.naturalColonyGoodwill = storytellerThreat.naturallGoodwillForAllFactions;
                    //Log.Message("New " + factionDef + " - naturalColonyGoodwill: " + factionDef.naturalColonyGoodwill, true);
                }
            }
        }

        public void RestoreNaturalGoodwillForAllFactions()
        {
            if (originalNaturalGoodwillValues != null)
            {
                //Log.Message("Restoring NaturalGoodwill");
                foreach (var factionDef in DefDatabase<FactionDef>.AllDefs)
                {
                    if (originalNaturalGoodwillValues.ContainsKey(factionDef) && factionDef != Faction.OfPlayer.def && !factionDef.permanentEnemy)
                    {
                        factionDef.naturalColonyGoodwill = originalNaturalGoodwillValues[factionDef];
                        //Log.Message("Old " + factionDef + " - factionDef.naturalColonyGoodwill: " + factionDef.naturalColonyGoodwill, true);
                    }
                }
            }
        }

        public bool GroupHasLivingPawns(HashSet<Pawn> group)
        {
            foreach (var pawn in group)
            {
                if (pawn != null && pawn.Map != null && !pawn.Dead && !pawn.Downed && !pawn.Destroyed)
                {
                    return true;
                }
            }
            return false;
        }
        public bool FactionPresentInCurrentRaidGroups(Faction faction)
        {
            for (int num = this.raidGroups.Count - 1; num >= 0; num--)
            {
                if (this.raidGroups[num].faction == faction)
                {
                    if (GroupHasLivingPawns(this.raidGroups[num].pawns))
                    {
                        return true;
                    }
                    else
                    {
                        //Log.Message(faction + " has no living pawns, removing it");
                        this.raidGroups.RemoveAt(num);
                    }
                }
            }

            for (int num = this.reinforcementGroups.Count - 1; num >= 0; num--)
            {
                if (this.reinforcementGroups[num].faction == faction)
                {
                    if (GroupHasLivingPawns(this.reinforcementGroups[num].pawns))
                    {
                        return true;
                    }
                    else
                    {
                        //Log.Message(faction + " has no living pawns, removing it");
                        this.reinforcementGroups.RemoveAt(num);
                    }
                }
            }
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastRaidExpansionTicks, "lastRaidExpansionTicks", 0);
            Scribe_Defs.Look(ref currentStoryteller, "currentStoryteller");
            Scribe_Collections.Look<RaidGroup>(ref raidGroups, "raidGroups", LookMode.Deep);
            Scribe_Collections.Look<RaidGroup>(ref reinforcementGroups, "reinforcementGroups", LookMode.Deep);
        }
    }
}
