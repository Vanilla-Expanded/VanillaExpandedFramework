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





        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref flagCorpseDecayingEffect, "flagCorpseDecayingEffect", true, true);
            Scribe_Values.Look(ref flagDigWhenHungry, "flagDigWhenHungry", true, true);
            Scribe_Values.Look(ref flagAnimalParticles, "flagAnimalParticles", true, true);



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
            ls.CheckboxLabeled("VCE_AnimalParticlesOption".Translate(), ref flagAnimalParticles, null);
            ls.Gap(12f);

            ls.End();
        }



    }










}
