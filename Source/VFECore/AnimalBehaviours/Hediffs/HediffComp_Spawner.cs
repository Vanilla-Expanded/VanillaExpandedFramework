

using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Linq;
using Verse.Sound;
using UnityEngine;

namespace AnimalBehaviours
{
    class HediffComp_Spawner : HediffComp
    {


        public HediffCompProperties_Spawner PropsSpawner
        {
            get
            {
                return (HediffCompProperties_Spawner)this.props;
            }
        }

        private int ticksUntilSpawn;


        public override void CompPostMake()
        {
            base.CompPostMake();
			ticksUntilSpawn = PropsSpawner.initialSpawnWait;
        }

        

		public override void CompPostTick(ref float severityAdjustment)
		{
			base.CompPostTick(ref severityAdjustment);
			TickInterval(1);
		}

		

		private void TickInterval(int interval)
		{
			if (!parent.pawn.Spawned)
			{
				return;
			}
			if(parent.pawn.Map != null) {
				ticksUntilSpawn -= interval;
				CheckShouldSpawn();
			}


				
			
		}

		private void CheckShouldSpawn()
		{
			if (ticksUntilSpawn <= 0)
			{
				ResetCountdown();
				TryDoSpawn();
			}
		}

		public bool TryDoSpawn()
		{
			if (!parent.pawn.Spawned)
			{
				return false;
			}
			if (PropsSpawner.spawnMaxAdjacent >= 0)
			{
				int num = 0;
				for (int i = 0; i < 9; i++)
				{
					IntVec3 c = parent.pawn.Position + GenAdj.AdjacentCellsAndInside[i];
					if (!c.InBounds(parent.pawn.Map))
					{
						continue;
					}
					List<Thing> thingList = c.GetThingList(parent.pawn.Map);
					for (int j = 0; j < thingList.Count; j++)
					{
						if (thingList[j].def == PropsSpawner.thingToSpawn)
						{
							num += thingList[j].stackCount;
							if (num >= PropsSpawner.spawnMaxAdjacent)
							{
								return false;
							}
						}
					}
				}
			}
			if (TryFindSpawnCell(parent.pawn, PropsSpawner.thingToSpawn, PropsSpawner.spawnCount, out var result))
			{
				Thing thing = ThingMaker.MakeThing(PropsSpawner.thingToSpawn);
				thing.stackCount = PropsSpawner.spawnCount;
				if (thing == null)
				{
					Log.Error("Could not spawn anything for " + parent);
				}
				if (PropsSpawner.inheritFaction && thing.Faction != parent.pawn.Faction)
				{
					thing.SetFaction(parent.pawn.Faction);
				}
				GenPlace.TryPlaceThing(thing, result, parent.pawn.Map, ThingPlaceMode.Direct, out var lastResultingThing);
				if (PropsSpawner.spawnForbidden)
				{
					lastResultingThing.SetForbidden(value: true);
				}
				if (PropsSpawner.showMessageIfOwned && parent.pawn.Faction == Faction.OfPlayer)
				{
					Messages.Message("MessageCompSpawnerSpawnedItem".Translate(PropsSpawner.thingToSpawn.LabelCap), thing, MessageTypeDefOf.PositiveEvent);
				}
				return true;
			}
			return false;
		}

		public static bool TryFindSpawnCell(Thing parent, ThingDef thingToSpawn, int spawnCount, out IntVec3 result)
		{
			foreach (IntVec3 item in GenAdj.CellsAdjacent8Way(parent).InRandomOrder())
			{
				if (!item.Walkable(parent.Map))
				{
					continue;
				}
				Building edifice = item.GetEdifice(parent.Map);
				if (edifice != null && thingToSpawn.IsEdifice())
				{
					continue;
				}
				Building_Door building_Door = edifice as Building_Door;
				if ((building_Door != null && !building_Door.FreePassage) || (parent.def.passability != Traversability.Impassable && !GenSight.LineOfSight(parent.Position, item, parent.Map)))
				{
					continue;
				}
				bool flag = false;
				List<Thing> thingList = item.GetThingList(parent.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Thing thing = thingList[i];
					if (thing.def.category == ThingCategory.Item && (thing.def != thingToSpawn || thing.stackCount > thingToSpawn.stackLimit - spawnCount))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					result = item;
					return true;
				}
			}
			result = IntVec3.Invalid;
			return false;
		}

		private void ResetCountdown()
		{
			ticksUntilSpawn = PropsSpawner.spawnIntervalRange.RandomInRange;
		}

		public override void CompExposeData()
		{
			base.CompExposeData();
			string text = (PropsSpawner.saveKeysPrefix.NullOrEmpty() ? null : (PropsSpawner.saveKeysPrefix + "_"));
			Scribe_Values.Look(ref ticksUntilSpawn, text + "ticksUntilSpawn", 0);
		}
		public override string CompLabelInBracketsExtra => GetLabel();
		public string GetLabel()
		{
			

			return ticksUntilSpawn.ToStringTicksToPeriod();
		}



		


	}
}
