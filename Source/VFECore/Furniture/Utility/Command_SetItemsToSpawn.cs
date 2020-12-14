using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse.AI;
using Verse;
using System.Linq;



namespace VanillaFurnitureExpanded
{
    [StaticConstructorOnStartup]
    public class Command_SetItemsToSpawn : Command
    {


        public CompConfigurableSpawner building;



        public Command_SetItemsToSpawn()
        {
            foreach (object obj in Find.Selector.SelectedObjects)
            {
                Building currentbuilding = obj as Building;
                building = currentbuilding.TryGetComp<CompConfigurableSpawner>();
                if (building != null)
                {
                    if (building.currentThingList != null)
                    {
                        icon = ContentFinder<Texture2D>.Get(building.currentThingList.GizmoIcon, true);
                        defaultLabel = building.currentThingList.GizmoLabel.Translate();
                        defaultDesc = building.currentThingList.GizmoDescription.Translate();

                    }
                    else
                    {
                        icon = ContentFinder<Texture2D>.Get("UI/IP_SetOutput", true);
                        defaultLabel = "IP_ChooseOutput".Translate();
                        defaultDesc = "IP_ChooseOutput".Translate();
                    }

                }
            }
            

        }



        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            List<FloatMenuOption> list = new List<FloatMenuOption>();
 
            foreach (ConfigurableSpawnerDef thingList in DefDatabase<ConfigurableSpawnerDef>.AllDefs.Where(element => (element.building == building.parent.def.defName)))
            {
                list.Add(new FloatMenuOption(thingList.listName.Translate(), delegate
                {
                    building.currentThingList = thingList;
                    building.ResetCountdown();

                }, MenuOptionPriority.Default, null, null, 29f, null, null));
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }






    }


}


