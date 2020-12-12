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
            if (building.currentThingList == null)
            {
                icon = ContentFinder<Texture2D>.Get("UI/IP_SetOutput", true);
                defaultLabel = "IP_ChooseOutput".Translate();
              
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

                }, MenuOptionPriority.Default, null, null, 29f, null, null));
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }






    }


}


