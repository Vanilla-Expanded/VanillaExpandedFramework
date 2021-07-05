using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFECore
{
	public class ScenPart_ArriveAtEdge : ScenPart
	{
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<PlayerPawnsArriveMethod>(ref this.method, "method", PlayerPawnsArriveMethod.Standing, false);
			Scribe_Values.Look<IntVec3>(ref this.location, "location");
		}

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			if (Widgets.ButtonText(listing.GetScenPartRect(this, ScenPart.RowHeight), this.method.ToStringHuman(), true, true, true))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (object obj in Enum.GetValues(typeof(PlayerPawnsArriveMethod)))
				{
					PlayerPawnsArriveMethod localM2 = (PlayerPawnsArriveMethod)obj;
					PlayerPawnsArriveMethod localM = localM2;
					list.Add(new FloatMenuOption(localM.ToStringHuman(), delegate ()
					{
						this.method = localM;
					}, MenuOptionPriority.Default, null, null, 0f, null, null));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
		}

		public override string Summary(Scenario scen)
		{
			if (this.method == PlayerPawnsArriveMethod.DropPods)
			{
				return "ScenPart_ArriveInDropPods".Translate();
			}
			return null;
		}

		public override void Randomize()
		{
			this.method = PlayerPawnsArriveMethod.Standing;
		}

		public override void GenerateIntoMap(Map map)
		{
			if (Find.GameInitData == null)
			{
				return;
			}
			RCellFinder.TryFindRandomPawnEntryCell(out this.location, map, 1f, false, null);
			List<List<Thing>> list = new List<List<Thing>>();
			foreach (Pawn item in Find.GameInitData.startingAndOptionalPawns)
			{
				list.Add(new List<Thing>
				{
					item
				});
			}
			List<Thing> list2 = new List<Thing>();
			foreach (ScenPart scenPart in Find.Scenario.AllParts)
			{
				list2.AddRange(scenPart.PlayerStartingThings());
			}
			int num = 0;
			foreach (Thing thing in list2)
			{
				if (thing.def.CanHaveFaction)
				{
					thing.SetFactionDirect(Faction.OfPlayer);
				}
				list[num].Add(thing);
				num++;
				if (num >= list.Count)
				{
					num = 0;
				}
			}
			DropPodUtility.DropThingGroupsNear(this.location, map, list, 110, Find.GameInitData.QuickStarted || this.method != PlayerPawnsArriveMethod.DropPods, true, true, true, false);
		}

		public override void PostMapGenerate(Map map)
		{
			if (Find.GameInitData == null)
			{
				return;
			}
		}

		private PlayerPawnsArriveMethod method;
		private IntVec3 location;
	}
}
