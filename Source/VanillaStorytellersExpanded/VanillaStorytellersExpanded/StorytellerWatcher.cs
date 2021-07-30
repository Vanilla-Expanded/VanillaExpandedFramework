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
            if (this.raidQueues?.Any() ?? false)
            {
                for (int num = raidQueues.Count - 1; num >= 0; num--)
                {
                    var raidQueue = raidQueues[num];
                    if (Find.TickManager.TicksAbs >= raidQueue.tickToFire)
                    {
                        try
                        {
                            raidQueue.incidentDef.Worker.TryExecute(raidQueue.parms);
                        }
                        catch
                        {
                            try
                            {
                                if (raidQueue.parms.target is null)
                                {
                                    raidQueue.parms.target = Find.RandomPlayerHomeMap;
                                }
                                var parms = StorytellerUtility.DefaultParmsNow(raidQueue.incidentDef.category, raidQueue.parms.target);
                                parms.faction = raidQueue.parms.faction;
                                raidQueue.incidentDef.Worker.TryExecute(parms);
                            }
                            catch
                            {

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
            if (this.raidQueues == null) this.raidQueues = new List<RaidQueue>();
            if (this.questGiverManagers == null) this.questGiverManagers = new Dictionary<int, QuestGiverManager>();
        }
        public override void LoadedGame()
        {
            base.LoadedGame();
            this.PreInit();
        }
        public override void StartedNewGame()
        {
            base.StartedNewGame();
            this.PreInit();
        }

        public void CheckStorytellerChanges()
        {
            if (currentStoryteller != Find.Storyteller.def)
            {
                currentStoryteller = Find.Storyteller.def;
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
