using RimWorld;
using UnityEngine;
using Verse;


namespace VanillaCookingExpanded
{


    public class VanillaCookingExpanded_Settings : ModSettings

    {


        public static bool allowConditions = true;



        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref allowConditions, "allowConditions", true, true);
       
        }

        public static void DoWindowContents(Rect inRect)
        {
            Listing_Standard ls = new Listing_Standard();


            ls.Begin(inRect);
            ls.Label("VCE_MustBeInstalled".Translate());
            ls.Gap(12f);
            ls.CheckboxLabeled("VCE_AllowConditions".Translate(), ref allowConditions, null);
            ls.Gap(12f);
           
            ls.End();
        }



    }










}
