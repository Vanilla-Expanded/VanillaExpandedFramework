using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace VanillaStorytellersExpanded
{
    public class StorytellerWatcher : GameComponent
	{
        public int lastRaidExpansionTicks;
        public StorytellerDef currentStoryteller;
        public Dictionary<FactionDef, IntRange> originalNaturalGoodwillValues;
        public List<RaidGroup> raidGroups;
        public List<RaidGroup> reinforcementGroups;
        public List<RaidQueue> raidQueues;
        public Dictionary<int, QuestGiverManager> questGiverManagers;
        public StorytellerWatcher()
        {
        }

        public StorytellerWatcher(Game game)
        {

        }

        public QuestGiverManager AddQuestGiverManager(int questManagerID, QuestGiverDef def)
        {
            var questGiverManager = new QuestGiverManager(def);
            questGiverManagers[questManagerID] = questGiverManager;
            if (def.generateOnce)
            {
                questGiverManager.GenerateQuests();
            }
            return questGiverManager;
        }
        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                //for (int i = 0; i < 60000; i++)
                //{
                //    Find.StoryWatcher.StoryWatcherTick();
                //    Find.Storyteller.StorytellerTick();
                //    Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + 1);
                //}
                CheckStorytellerChanges();
                foreach (var questGiverManager in questGiverManagers.Values)
                {
                    questGiverManager.Tick();
                }
            }
            if (this.raidQueues?.Count > 0)
            {
                for (int num = raidQueues.Count - 1; num >= 0; num--)
                {
                    if (Find.TickManager.TicksAbs >= raidQueues[num].tickToFire)
                    {
                        try
                        {
                            raidQueues[num].incidentDef.Worker.TryExecute(raidQueues[num].parms);
                        }
                        catch
                        {
                            try
                            {
                                if (raidQueues[num].parms.target != null)
                                {
                                    var parms = StorytellerUtility.DefaultParmsNow(raidQueues[num].incidentDef.category, raidQueues[num].parms.target);
                                    parms.faction = raidQueues[num].parms.faction;
                                    raidQueues[num].incidentDef.Worker.TryExecute(parms);
                                }
                                else
                                {
                                    Log.Error("Raid queue has no target. This shouldn't happen. Removing raid queue.");
                                    raidQueues.RemoveAt(num);
                                }
                            }
                            catch
                            {
                                raidQueues.RemoveAt(num);
                            }
                        }
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
            if (this.questGiverManagers == null) this.questGiverManagers = new Dictionary<int, QuestGiverManager>();
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
                }
            }
        }

        public void RestoreNaturalGoodwillForAllFactions()
        {
            if (originalNaturalGoodwillValues != null)
            {
                foreach (var factionDef in DefDatabase<FactionDef>.AllDefs)
                {
                    if (originalNaturalGoodwillValues.ContainsKey(factionDef) && factionDef != Faction.OfPlayer.def && !factionDef.permanentEnemy)
                    {
                        factionDef.naturalColonyGoodwill = originalNaturalGoodwillValues[factionDef];
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
            Scribe_Collections.Look(ref questGiverManagers, "questGiverManagers", LookMode.Value, LookMode.Deep, ref intKeys, ref questGiverValues);
            Scribe_Collections.Look(ref raidQueues, "raidQueues", LookMode.Deep);
        }

        private List<int> intKeys = new List<int>();
        private List<QuestGiverManager> questGiverValues = new List<QuestGiverManager>();
    }
}
