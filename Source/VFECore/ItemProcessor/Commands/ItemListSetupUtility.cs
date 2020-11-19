using RimWorld;
using Verse;

using UnityEngine;


namespace ItemProcessor
{
    public static class ItemListSetupUtility
    {

        public static Command_SetOutputList SetOutputListCommand(Building_ItemProcessor passingbuilding, Map passingMap, string desc)
        {
            return new Command_SetOutputList()
            {
                //defaultDesc = desc.Translate(),
                hotKey = KeyBindingDefOf.Misc1,
                map = passingMap,
                building = passingbuilding

            };
        }

        public static Command_SetFirstItemList SetFirstItemListCommand(Building_ItemProcessor passingbuilding, Map passingMap, string desc)
        {
            return new Command_SetFirstItemList()
            {
                defaultDesc = desc.Translate(),
                hotKey = KeyBindingDefOf.Misc1,
                map = passingMap,
                building = passingbuilding

            };
        }

        public static Command_SetSecondItemList SetSecondItemListCommand(Building_ItemProcessor passingbuilding, Map passingMap, string desc)
        {
            return new Command_SetSecondItemList()
            {
                defaultDesc = desc.Translate(),
                hotKey = KeyBindingDefOf.Misc1,
                map = passingMap,
                building = passingbuilding

            };
        }

        public static Command_SetThirdItemList SetThirdItemListCommand(Building_ItemProcessor passingbuilding, Map passingMap, string desc)
        {
            return new Command_SetThirdItemList()
            {
                defaultDesc = desc.Translate(),
                hotKey = KeyBindingDefOf.Misc1,
                map = passingMap,
                building = passingbuilding

            };
        }

        public static Command_SetFourthItemList SetFourthItemListCommand(Building_ItemProcessor passingbuilding, Map passingMap, string desc)
        {
            return new Command_SetFourthItemList()
            {
                defaultDesc = desc.Translate(),
                hotKey = KeyBindingDefOf.Misc1,
                map = passingMap,
                building = passingbuilding

            };
        }

        public static Command_SetQualityList SetQualityListCommand(Building_ItemProcessor passingbuilding, Map passingMap)
        {
            return new Command_SetQualityList()
            {
                defaultDesc = "IP_SetAutoQuality".Translate(),
                hotKey = KeyBindingDefOf.Misc1,
                map = passingMap,
                building = passingbuilding

            };
        }
    }
}
