using RimWorld;
using UnityEngine;
using Verse;


namespace VEF.AnimalBehaviours
{



    public class AnimalBehaviours_Mod : Mod
    {


        public AnimalBehaviours_Mod(ModContentPack content) : base(content)
        {
            var settings = GetSettings<AnimalBehaviours_Settings>();
            BackwardsCompatibilityFixer.FixSettingsNameOrNamespace(this, settings, "AnimalBehaviours");
        }
        public override string SettingsCategory()
        {
           
                return "Animal Behaviours";
           


        }



        public override void DoSettingsWindowContents(Rect inRect)
        {
            AnimalBehaviours_Settings.DoWindowContents(inRect);
        }
    }


}
