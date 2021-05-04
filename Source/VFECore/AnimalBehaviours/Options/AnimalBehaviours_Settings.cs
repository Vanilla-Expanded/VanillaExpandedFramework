using RimWorld;
using UnityEngine;
using Verse;


namespace AnimalBehaviours
{


    public class AnimalBehaviours_Settings : ModSettings

    {


        public static bool flagCorpseDecayingEffect = true;
        public static bool flagDigWhenHungry = true;
        public static bool flagAnimalParticles = true;
        public static bool flagAsexualReproduction = true;
        public static bool flagBlinkMechanics = true;
        public static bool flagBuildPeriodically = true;
        public static bool flagDigPeriodically = true;
        public static bool flagChargeBatteries = true;
        public static bool flagExplodingAnimalEggs = true;
        public static bool flagHovering = true;
        public static bool flagGraphicChanging = true;
        public static bool flagEffecters = true;
        public static bool flagRegeneration = true;
        public static bool flagResurrection = true;
        public static bool flagUntameable = true;



        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref flagCorpseDecayingEffect, "flagCorpseDecayingEffect", true, true);
            Scribe_Values.Look(ref flagDigWhenHungry, "flagDigWhenHungry", true, true);
            Scribe_Values.Look(ref flagAnimalParticles, "flagAnimalParticles", true, true);
            Scribe_Values.Look(ref flagAsexualReproduction, "flagAsexualReproduction", true, true);
            Scribe_Values.Look(ref flagBlinkMechanics, "flagBlinkMechanics", true, true);
            Scribe_Values.Look(ref flagBuildPeriodically, "flagBuildPeriodically", true, true);
            Scribe_Values.Look(ref flagDigPeriodically, "flagDigPeriodically", true, true);
            Scribe_Values.Look(ref flagChargeBatteries, "flagChargeBatteries", true, true);
            Scribe_Values.Look(ref flagExplodingAnimalEggs, "flagExplodingAnimalEggs", true, true);
            Scribe_Values.Look(ref flagHovering, "flagHovering", true, true);
            Scribe_Values.Look(ref flagGraphicChanging, "flagGraphicChanging", true, true);
            Scribe_Values.Look(ref flagEffecters, "flagEffecters", true, true);
            Scribe_Values.Look(ref flagRegeneration, "flagRegeneration", true, true);
            Scribe_Values.Look(ref flagResurrection, "flagResurrection", true, true);
            Scribe_Values.Look(ref flagUntameable, "flagUntameable", true, true);



        }

        public static void DoWindowContents(Rect inRect)
        {
            Listing_Standard ls = new Listing_Standard();


            ls.Begin(inRect);
            ls.Label("VCE_AffectsAllAnimalMods".Translate());
            ls.Gap(12f);
            ls.CheckboxLabeled("VCE_CorpseDecayingEffectOption".Translate(), ref flagCorpseDecayingEffect, null);
            ls.Gap(12f);
            ls.CheckboxLabeled("VCE_DigWhenHungryOption".Translate(), ref flagDigWhenHungry, null);
            ls.Gap(12f);
            ls.CheckboxLabeled("VCE_DigPeriodicallyOption".Translate(), ref flagDigPeriodically, null);
            ls.Gap(12f);
            ls.CheckboxLabeled("VCE_AnimalParticlesOption".Translate(), ref flagAnimalParticles, null);
            ls.Gap(12f);
            ls.CheckboxLabeled("VCE_AsexualReproductionOption".Translate(), ref flagAsexualReproduction, null);
            ls.Gap(12f);
            ls.CheckboxLabeled("VCE_BlinkMechanicsOption".Translate(), ref flagBlinkMechanics, null);
            ls.Gap(12f);
            ls.CheckboxLabeled("VCE_BuildPeriodicallyOption".Translate(), ref flagBuildPeriodically, null);
            ls.Gap(12f);
            ls.CheckboxLabeled("VCE_ChargeBatteriesOption".Translate(), ref flagChargeBatteries, null);
            ls.Gap(12f);
            ls.CheckboxLabeled("VCE_ExplodingEggsOption".Translate(), ref flagExplodingAnimalEggs, null);
            ls.Gap(12f);
            ls.CheckboxLabeled("VCE_HoveringOption".Translate(), ref flagHovering, null);
            ls.Gap(12f);
            ls.CheckboxLabeled("VCE_GraphicChangingOption".Translate(), ref flagGraphicChanging, null);
            ls.Gap(12f);
            ls.CheckboxLabeled("VCE_EffecterOption".Translate(), ref flagEffecters, null);
            ls.Gap(12f);
            ls.CheckboxLabeled("VCE_RegenerationOption".Translate(), ref flagRegeneration, null);
            ls.Gap(12f);
            ls.CheckboxLabeled("VCE_ResurrectionOption".Translate(), ref flagResurrection, null);
            ls.Gap(12f);
            ls.CheckboxLabeled("VCE_UntameableOption".Translate(), ref flagUntameable, null);
            ls.Gap(12f);


            ls.End();
        }



    }










}
