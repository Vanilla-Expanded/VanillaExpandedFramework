using RimWorld;
using UnityEngine;
using Verse;
using System.Linq;
using System.Collections.Generic;



namespace AnimalBehaviours
{



    public class VanillaAnimalsExpanded_Mod : Mod
    {


        public static VanillaAnimalsExpanded_Settings settings;

        public VanillaAnimalsExpanded_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<VanillaAnimalsExpanded_Settings>();
        }
     
        public override string SettingsCategory()
        {
            bool anyToggles = (from k in DefDatabase<GenericToggleableAnimalDef>.AllDefsListForReading select k).Any();

            if (anyToggles)
            {
                return "Animal Toggles";
            }
            else return "";


        }



        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);

            List<GenericToggleableAnimalDef> toggleablespawnlist = (from k in DefDatabase<GenericToggleableAnimalDef>.AllDefsListForReading select k).ToList();

            foreach (GenericToggleableAnimalDef toggleablespawndef in toggleablespawnlist) {
                if (settings.pawnSpawnStates == null) settings.pawnSpawnStates = new Dictionary<string, bool>();
                foreach (string defName in toggleablespawndef.toggleablePawns)
                {
                    if (!settings.pawnSpawnStates.ContainsKey(defName) && DefDatabase<ThingDef>.GetNamedSilentFail(defName) != null)
                    {
                        settings.pawnSpawnStates[defName] = false;
                    }
                }

            }
            
            settings.DoWindowContents(inRect);


        }
    }


}
