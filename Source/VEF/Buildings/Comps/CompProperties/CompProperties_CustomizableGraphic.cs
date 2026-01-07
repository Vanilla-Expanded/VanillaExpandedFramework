using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VEF.Buildings;

public class CompProperties_CustomizableGraphic : CompProperties
{
    // By default, use the same graphic as CompRandomBuildingGraphic - may as well reuse it
    public string iconPath = "UI/VEF_ChooseGraphic";
    private Texture2D icon;

    // Overrides for the default label/description
    public string gizmoLabel = null;
    public string gizmoDescription = null;

    // Default graphic index for default and styled graphic
    public int defaultIndex = -1;
    public Dictionary<ThingStyleDef, int> defaultStyleIndex;

    // Data (name, sorting order) for default and styled graphic
    public List<CustomizableGraphicOptionData> defaultGraphicData;
    public Dictionary<ThingStyleDef, List<CustomizableGraphicOptionData>> styledGraphicData;

    public Texture2D Icon => icon ??= ContentFinder<Texture2D>.Get(iconPath) ?? BaseContent.BadTex;

    public CompProperties_CustomizableGraphic() => compClass = typeof(CompCustomizableGraphic);

    // Data for each graphic entry, may add more data in the future if needed
    public class CustomizableGraphicOptionData
    {
        public string name;
        public int sortingPriority;

        // Which graphic variant this will be rotated to in clockwise/counterclockwise direction.
        // Currently, only used for Gravships, but may be used for other features in the future. Use any negative value if it can't be rotated.
        // Counterclockwise will be handled by rotating clockwise - a single counterclockwise rotation will result in 3 clockwise rotations.
        public int clockwiseRotationIndex = -1;
    }
}