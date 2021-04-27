using RimWorld;
using UnityEngine;
using Verse;


namespace VanillaCookingExpanded
{



    public class VanillaCookingExpanded_Mod : Mod
    {


        public VanillaCookingExpanded_Mod(ModContentPack content) : base(content)
        {
            GetSettings<VanillaCookingExpanded_Settings>();
        }
        public override string SettingsCategory() {
            if (ModLister.HasActiveModWithName("Vanilla Cooking Expanded")){
                return "Vanilla Cooking Expanded";
            } else return "";


        }
        
       

        public override void DoSettingsWindowContents(Rect inRect)
        {
            VanillaCookingExpanded_Settings.DoWindowContents(inRect);
        }
    }


}
