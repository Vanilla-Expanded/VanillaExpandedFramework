using RimWorld;
using Verse;

using UnityEngine;


namespace VanillaFurnitureExpanded
{
    public static class ConfigurableSpawnerSettableUtility
    {
        public static Command_SetItemsToSpawn SetItemsToSpawnCommand(CompConfigurableSpawner passingbuilding)
        {
            return new Command_SetItemsToSpawn()
            {

                hotKey = KeyBindingDefOf.Misc1,

                building = passingbuilding

            };
        }


    }
}