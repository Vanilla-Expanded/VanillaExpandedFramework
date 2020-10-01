using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace VanillaStorytellersExpanded
{
	public class StorytellerWatcher : GameComponent
	{
        public int lastRaidExpansionTicks;

        public StorytellerDef currentStoryteller;

        public Faction currentRaidingFaction;

        public Dictionary<FactionDef, IntRange> originalNaturalGoodwillValues;

        public Dictionary<Lord, bool> enemiesAreOutOfTheMap;

        public List<RaidGroup> raidGroups;
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
        }

        public void PreInit()
        {
            if (this.raidGroups == null) this.raidGroups = new List<RaidGroup>();
            if (this.enemiesAreOutOfTheMap == null) this.enemiesAreOutOfTheMap = new Dictionary<Lord, bool>();
            if (this.originalNaturalGoodwillValues == null) this.originalNaturalGoodwillValues = new Dictionary<FactionDef, IntRange>();
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
            Log.Message("Changing NaturalGoodwill");
            RestoreNaturalGoodwillForAllFactions();
            foreach (var factionDef in DefDatabase<FactionDef>.AllDefs)
            {
                if (factionDef != Faction.OfPlayer.def && !factionDef.permanentEnemy)
                {
                    originalNaturalGoodwillValues[factionDef] = factionDef.naturalColonyGoodwill;
                    factionDef.naturalColonyGoodwill = storytellerThreat.naturallGoodwillForAllFactions;
                    Log.Message("New " + factionDef + " - naturalColonyGoodwill: " + factionDef.naturalColonyGoodwill, true);
                }
            }
        }

        public void RestoreNaturalGoodwillForAllFactions()
        {
            if (originalNaturalGoodwillValues != null)
            {
                Log.Message("Restoring NaturalGoodwill");
                foreach (var factionDef in DefDatabase<FactionDef>.AllDefs)
                {
                    if (originalNaturalGoodwillValues.ContainsKey(factionDef) && factionDef != Faction.OfPlayer.def && !factionDef.permanentEnemy)
                    {
                        factionDef.naturalColonyGoodwill = originalNaturalGoodwillValues[factionDef];
                        Log.Message("Old " + factionDef + " - factionDef.naturalColonyGoodwill: " + factionDef.naturalColonyGoodwill, true);
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastRaidExpansionTicks, "lastRaidExpansionTicks", 0);
            Scribe_Defs.Look(ref currentStoryteller, "currentStoryteller");
            Scribe_References.Look<Faction>(ref currentRaidingFaction, "currentRaidingFaction");
            Scribe_Collections.Look<Lord, bool>(ref enemiesAreOutOfTheMap, "enemiesAreOutOfTheMap", LookMode.Reference, LookMode.Value, ref lordKeys, ref lordValues);
            Scribe_Collections.Look<RaidGroup>(ref raidGroups, "raidGroups", LookMode.Deep);
        }

        private List<Lord> lordKeys;
        private List<bool> lordValues;
    }
}
