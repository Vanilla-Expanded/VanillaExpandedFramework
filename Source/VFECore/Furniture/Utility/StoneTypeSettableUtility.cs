using RimWorld;
using Verse;

using UnityEngine;


namespace VanillaFurnitureExpanded
{
    public static class StoneTypeSettableUtility
    {
        public static Command_SetStoneType SetStoneToMineCommand(CompRockSpawner passingbuilding)
        {
            return new Command_SetStoneType()
            {

                hotKey = KeyBindingDefOf.Misc1,

                building = passingbuilding

            };
        }


    }
}