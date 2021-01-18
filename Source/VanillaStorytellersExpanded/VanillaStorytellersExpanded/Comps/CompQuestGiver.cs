using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace VanillaStorytellersExpanded
{
	public class CompProperties_QuestGiver : CompProperties
	{
		public string floatOptionLabel;
		public JobDef jobToGive;
		public int questManagerID;
		public QuestGiverDef questGiver;
		public CompProperties_QuestGiver()
		{
			compClass = typeof(CompQuestGiver);
		}
	}
	public class CompQuestGiver : ThingComp
	{
		public CompProperties_QuestGiver Props => (CompProperties_QuestGiver)props;

		private StorytellerWatcher storytellerWatcher;
		public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
			storytellerWatcher = Current.Game.GetComponent<StorytellerWatcher>();
		}
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
			yield return QuestGiverFloatMenuOption(selPawn);
		}
        public FloatMenuOption QuestGiverFloatMenuOption(Pawn user)
		{
			string label = Props.floatOptionLabel;
			Action action = delegate
			{

				Job job = JobMaker.MakeJob(Props.jobToGive, this.parent);
				user.jobs.TryTakeOrderedJob(job);
			};
			return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action, MenuOptionPriority.VeryLow), user, this.parent);
		}

		public void Use()
        {
			QuestGiverManager questGiverManager = null;
			if (!storytellerWatcher.questGiverManagers.TryGetValue(Props.questManagerID, out questGiverManager))
			{
				questGiverManager = storytellerWatcher.AddQuestGiverManager(Props.questManagerID, Props.questGiver);
				questGiverManager.Init();
			}
			questGiverManager.CallWindow();
		}
	}
}
