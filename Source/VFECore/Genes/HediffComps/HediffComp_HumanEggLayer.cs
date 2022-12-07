using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI.Group;

namespace VanillaGenesExpanded
{
    public class HediffComp_HumanEggLayer : HediffComp
    {

		private float eggProgress;

		private int fertilizationCount;

		private Pawn fertilizedBy;

		public int pregnancyRemovalCounter = -1;

		public List<GeneDef> motherGenes = new List<GeneDef>();
		public List<GeneDef> fatherGenes = new List<GeneDef>();


		private bool Active
		{
			get
			{
				Pawn pawn = parent.pawn as Pawn;
				if (Props.eggLayFemaleOnly && pawn != null && pawn.gender != Gender.Female)
				{
					return false;
				}
				if (!pawn.ageTracker.CurLifeStage.reproductive)
				{
					return false;
				}
				if (pawn.GetStatValue(StatDefOf.Fertility) <= 0f)
				{
					return false;
				}			
				if (pawn.SterileGenes())
				{
					return false;
				}
				return true;
			}
		}

		public bool CanLayNow
		{
			get
			{
				if (!Active)
				{
					return false;
				}
				return eggProgress >= 1f;
			}
		}

		public bool FullyFertilized => fertilizationCount >= 1;

		private bool ProgressStoppedBecauseUnfertilized
		{
			get
			{
				if (Props.eggProgressUnfertilizedMax < 1f && fertilizationCount == 0)
				{
					return eggProgress >= Props.eggProgressUnfertilizedMax;
				}
				return false;
			}
		}

		public HediffCompProperties_HumanEggLayer Props => (HediffCompProperties_HumanEggLayer)props;

		public override void CompExposeData()
		{
			base.CompExposeData();
			Scribe_Values.Look(ref eggProgress, "eggProgress", 0f);
			Scribe_Values.Look(ref fertilizationCount, "fertilizationCount", 0);
			Scribe_Values.Look<int>(ref pregnancyRemovalCounter, "pregnancyRemovalCounter", -1);			
			Scribe_References.Look(ref fertilizedBy, "fertilizedBy");
			Scribe_Collections.Look(ref this.motherGenes, nameof(this.motherGenes),LookMode.Def);
			Scribe_Collections.Look(ref this.fatherGenes, nameof(this.fatherGenes), LookMode.Def);
		}

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (pregnancyRemovalCounter > -1)
            {
				pregnancyRemovalCounter++;
				if(pregnancyRemovalCounter > 100) {
					Pawn.health.RemoveHediff(Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PregnantHuman));
					pregnancyRemovalCounter = -1;
				}

			}


			if (Active)
			{


				float num = 1f / (Props.eggLayIntervalDays * 60000f);
				Pawn pawn = parent.pawn as Pawn;



				if (pawn != null)
				{
                   
					num *= PawnUtility.BodyResourceGrowthSpeed(pawn);
				}
                if (Pawn.Map != null) { eggProgress += num;}
				
				
				if (eggProgress >= 1f)
				{
					eggProgress = 1f;
					if (Pawn.Map != null) { ProduceEgg(); }
					
				}
				
				if (ProgressStoppedBecauseUnfertilized)
				{
					eggProgress = Props.eggProgressUnfertilizedMax;
				}
			}
		}

		public void Fertilize(Pawn male)
		{
			fertilizationCount = 1;
			fertilizedBy = male;
			Find.LetterStack.ReceiveLetter("VGE_EggPregnancyLabel".Translate(Pawn.NameShortColored), "VGE_EggPregnancy".Translate(Pawn.NameShortColored), LetterDefOf.PositiveEvent, (TargetInfo)Pawn);
			

		}

		public void DisableNormalPregnancy()
		{
			pregnancyRemovalCounter = 0;

		}

		public ThingDef NextEggType()
		{
			if (fertilizationCount > 0)
			{
				return Props.eggFertilizedDef;
			}
			return Props.eggUnfertilizedDef;
		}

		public virtual Thing ProduceEgg()
		{
			if (!Active)
			{
				Log.Error("LayEgg while not Active: " + parent);
			}
			eggProgress = 0f;
			
			Thing thing;
			if (fertilizationCount > 0)
			{
				thing = ThingMaker.MakeThing(Props.eggFertilizedDef);
				fertilizationCount = 0;
			}
			else
			{
				thing = ThingMaker.MakeThing(Props.eggUnfertilizedDef);
			}
			
			CompHumanHatcher comphumanHatcher = thing.TryGetComp<CompHumanHatcher>();
			if (comphumanHatcher != null)
			{
				comphumanHatcher.hatcheeFaction = parent.pawn.Faction;
				Pawn pawn = parent.pawn as Pawn;
				if (pawn != null)
				{
					comphumanHatcher.hatcheeParent = pawn;
					comphumanHatcher.motherGenes = this.motherGenes;
				}
				if (fertilizedBy != null)
				{
					comphumanHatcher.otherParent = fertilizedBy;
					comphumanHatcher.fatherGenes = this.fatherGenes;
				}
                if (Props.maleDominant)
                {
					comphumanHatcher.maleDominant = Props.maleDominant;
				}
				if (Props.femaleDominant)
				{
					comphumanHatcher.femaleDominant = Props.femaleDominant;
				}


			}
			GenPlace.TryPlaceThing(thing, Pawn.Position, Pawn.Map, ThingPlaceMode.Near, delegate (Thing t, int i)
			{
				if (Pawn.Faction != Faction.OfPlayer)
				{
					t.SetForbidden(value: true);
				}
			});
			Find.LetterStack.ReceiveLetter("VGE_EggLaidLabel".Translate(Pawn.NameShortColored), "VGE_EggLaid".Translate(Pawn.NameShortColored), LetterDefOf.PositiveEvent, (TargetInfo)Pawn);

			return thing;
		}

		public override string CompLabelInBracketsExtra => GetLabel();




		public string GetLabel()
		{

			if (!Active)
			{
				return null;
			}
			string text = "EggProgress".Translate() + ": " + eggProgress.ToStringPercent();
			if (Props.eggLayFemaleOnly && Pawn.gender == Gender.Male)
			{
				text += "\n" + "VGE_Male_Egg".Translate();
			}else

			if (fertilizationCount > 0)
			{
				text += "\n" + "Fertilized".Translate();
			}
			else if (ProgressStoppedBecauseUnfertilized)
			{
				text += "\n" + "ProgressStoppedUntilFertilized".Translate();
			}
			return text;


		}

	



	}
}
