using RimWorld;
using UnityEngine;
using Verse;


namespace AnimalBehaviours
{



    public class AnimalBehaviours_Mod : Mod
    {


        public AnimalBehaviours_Mod(ModContentPack content) : base(content)
        {
            GetSettings<AnimalBehaviours_Settings>();
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
