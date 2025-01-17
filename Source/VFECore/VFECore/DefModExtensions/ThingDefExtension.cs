using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFECore
{
    public class ThingDefExtension : DefModExtension
    {
        // For weapons
        public bool usableWithShields = false;
        public WeaponDrawOffsets weaponCarryDrawOffsets = null;     // Offsets carried weapon regardless of stance
        public WeaponDrawOffsets weaponDraftedDrawOffsets = null;   // Only when drafted but not actively attacking/aiming

        // For shields and apparel
        public List<PawnKindDef> useFactionColourForPawnKinds;

        // For artillery
        public float siegeBlueprintPoints = SiegeBlueprintPlacer.ArtyCost;

        // For thing that can be discovered by deep scanner
        public Color deepColor = Color.white;
        public float transparencyMultiplier = 0.5f;
        public bool allowDeepDrill = true;
        // For buildings that need to render deep resources mouse attachments
        public bool deepResourcesOnGUI = false;
        public bool deepResourcesOnGUIRequireScanner = true;

        // For skyfallers that can fall into shield fields
        public int shieldDamageIntercepted = -1;

        public bool destroyCorpse;

        public ConstructionSkillRequirement constructionSkillRequirement;

        public float? meleeRangeOverride;
    }

    public class WeaponDrawOffsets
    {
        public Offset north = null;
        public Offset east = null;
        public Offset south = null;
        public Offset west = null;
    }

    public class Offset
    {
        public Vector3 drawOffset = new(0, 0, 0);
        public float angleOffset = 0;
    }

}