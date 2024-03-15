using Verse;
using UnityEngine;

using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;

namespace VanillaGenesExpanded
{
    public class CompHumanHatcher : ThingComp
    {
		private float gestateProgress;

		public Pawn hatcheeParent;

		public Pawn otherParent;

		public Faction hatcheeFaction;

		public CompProperties_HumanHatcher Props => (CompProperties_HumanHatcher)props;

		private CompTemperatureRuinable FreezerComp => parent.GetComp<CompTemperatureRuinable>();

		public List<GeneDef> motherGenes = new List<GeneDef>();
		public List<GeneDef> fatherGenes = new List<GeneDef>();

		public bool maleDominant = false;
		public bool femaleDominant = false;

		public bool TemperatureDamaged
		{
			get
			{
				if (FreezerComp != null)
				{
					return FreezerComp.Ruined;
				}
				return false;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref gestateProgress, "gestateProgress", 0f);
			Scribe_Values.Look(ref maleDominant, "maleDominant", false);
			Scribe_Values.Look(ref femaleDominant, "femaleDominant", false);
			Scribe_References.Look(ref hatcheeParent, "hatcheeParent");
			Scribe_References.Look(ref otherParent, "otherParent");
			Scribe_References.Look(ref hatcheeFaction, "hatcheeFaction");
			Scribe_Collections.Look(ref this.motherGenes, nameof(this.motherGenes), LookMode.Def);
			Scribe_Collections.Look(ref this.fatherGenes, nameof(this.fatherGenes), LookMode.Def);
		}

		public override void CompTick()
		{
			if (!TemperatureDamaged)
			{
				float num = 1f / (Props.hatcherDaystoHatch * 60000f);
				gestateProgress += num;
				if (gestateProgress > 1f)
				{
					Hatch();
				}
			}
		}

		public void Hatch()
		{
			try
			{
				PawnGenerationRequest request = new PawnGenerationRequest(hatcheeParent.kindDef, hatcheeFaction, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, allowDead: false, allowDowned: true, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Newborn);
				for (int i = 0; i < parent.stackCount; i++)
				{
					Pawn pawn = PawnGenerator.GeneratePawn(request);
					if (PawnUtility.TrySpawnHatchedOrBornPawn(pawn, parent))
					{
						if (pawn != null)
						{
							if (hatcheeParent != null)
							{
								if (pawn.playerSettings != null && hatcheeParent.playerSettings != null && hatcheeParent.Faction == hatcheeFaction)
								{
									pawn.playerSettings.AreaRestrictionInPawnCurrentMap = hatcheeParent.playerSettings.AreaRestrictionInPawnCurrentMap;
								}
								if (pawn.RaceProps.IsFlesh)
								{
									pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, hatcheeParent);
								}
							}
							if (otherParent != null && (hatcheeParent == null || hatcheeParent.gender != otherParent.gender) && pawn.RaceProps.IsFlesh)
							{
								pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, otherParent);
							}
						}
						if (parent.Spawned)
						{
							FilthMaker.TryMakeFilth(parent.Position, parent.Map, ThingDefOf.Filth_AmnioticFluid);
						}

						Find.LetterStack.ReceiveLetter("VGE_EggHatchedLabel".Translate(pawn.NameShortColored), "VGE_EggHatched".Translate(pawn.NameShortColored), LetterDefOf.PositiveEvent, (TargetInfo)pawn);


						if (maleDominant)
                        {
                            if (fatherGenes?.Count > 0) { 
								foreach(GeneDef gene in fatherGenes)
                                {
									pawn.genes.AddGene(gene,false);
								}
																						
							}							
                        }else if (femaleDominant)
						{
							if (motherGenes?.Count > 0)
							{
								foreach (GeneDef gene in motherGenes)
								{
									pawn.genes.AddGene(gene, false);
								}

							}
						}
                        else
                        {
							System.Random rand = new System.Random();
							List<GeneDef> genesToAdd = new List<GeneDef>();
							foreach (GeneDef gene in motherGenes)
                            {
                                if (fatherGenes.Contains(gene))
                                {
									genesToAdd.Add(gene);
                                }else
                                {									
                                    if (rand.NextDouble() > 0.5f)
                                    {
										genesToAdd.Add(gene);
									}
                                }
                            }
							foreach (GeneDef gene in fatherGenes)
							{
								if (!motherGenes.Contains(gene))								
								{
									if (rand.NextDouble() > 0.5f)
									{
                                        if (!genesToAdd.Contains(gene)) { genesToAdd.Add(gene); }										
									}
								}
							}
							foreach (GeneDef gene in genesToAdd)
							{
								pawn.genes.AddGene(gene, false);
							}

						}



					}
					else
					{
						Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
					}
				}
			}
			finally
			{
				parent.Destroy();
			}
		}

		public override bool AllowStackWith(Thing other)
		{
			
			return false;
			
		}

		public override void PreAbsorbStack(Thing otherStack, int count)
		{
			float t = (float)count / (float)(parent.stackCount + count);
			float b = ((ThingWithComps)otherStack).GetComp<CompHumanHatcher>().gestateProgress;
			gestateProgress = Mathf.Lerp(gestateProgress, b, t);
		}

		public override void PostSplitOff(Thing piece)
		{
			CompHumanHatcher comp = ((ThingWithComps)piece).GetComp<CompHumanHatcher>();
			comp.gestateProgress = gestateProgress;
			comp.hatcheeParent = hatcheeParent;
			comp.otherParent = otherParent;
			comp.hatcheeFaction = hatcheeFaction;
		}

		public override void PrePreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
		{
			base.PrePreTraded(action, playerNegotiator, trader);
			switch (action)
			{
				case TradeAction.PlayerBuys:
					hatcheeFaction = Faction.OfPlayer;
					break;
				case TradeAction.PlayerSells:
					hatcheeFaction = trader.Faction;
					break;
			}
		}

		public override void PostPostGeneratedForTrader(TraderKindDef trader, int forTile, Faction forFaction)
		{
			base.PostPostGeneratedForTrader(trader, forTile, forFaction);
			hatcheeFaction = forFaction;
		}

		public override string CompInspectStringExtra()
		{
			if (!TemperatureDamaged)
			{
				return "EggProgress".Translate() + ": " + gestateProgress.ToStringPercent() + "\n" + "HatchesIn".Translate() + ": " + "PeriodDays".Translate((Props.hatcherDaystoHatch * (1f - gestateProgress)).ToString("F1"));
			}
			return null;
		}

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
				yield return gizmo;
            }
			if (DebugSettings.ShowDevGizmos)
			{

				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "DEV: Finish hatching";
				command_Action.action = delegate
				{
					gestateProgress = 1;
				};
				yield return command_Action;
			}
		}
    }
}
