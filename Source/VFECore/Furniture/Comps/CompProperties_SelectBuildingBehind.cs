using RimWorld;
using Verse;

namespace VanillaFurnitureExpanded
{
    public class CompProperties_SelectBuildingBehind : CompProperties
    {

        //A very simple comp class that adds a Gizmo command button that deselects this item, and selects a buildingToSelect
        //in the same tile.

        //This is used in holograms to add a "Select Base" button, but can be used with any other two Buildings that want to
        //share the same space

        public CompProperties_SelectBuildingBehind()
        {
            this.compClass = typeof(CompSelectBuildingBehind);
        }

        public string buildingToSelect;
        public string commandButtonImage = "";
        public string commandButtonText = "";
        public string commandButtonDesc = "";
    }
}