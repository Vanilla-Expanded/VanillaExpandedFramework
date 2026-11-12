using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using VEF.Buildings;
using Verse;
using VEF.Pawns;

namespace VEF.Things
{
    public class ThingDefExtension : DefModExtension
    {
        // For weapons
        public bool? usableWithShields = false;
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

        // Different graphics/styles if crafted by the player. As opposed to random styles, this only ever applies to player-crafted items.
        // List of all possible styles.
        public List<ThingStyleChance> playerCraftedStyles;
        // Determines if the style for player crafted items can override other styles (like random style or ideo styles)
        public bool playerCraftedStylesOverrideOtherStyles = false;
        // Chance for the player crafted style to be applied.
        public float playerCraftedStyleChance = 1f;

        // For CompAssignableToPawn
        // Always hide the overlay showing the assigned pawn
        public bool hideAssignedPawnOverlay = false;
        // Hide the overlay if there's no assigned pawn
        public bool hideUnownedAssignedPawnOverlay = false;

        public override void ResolveReferences(Def parentDef)
        {
            base.ResolveReferences(parentDef);

            if (hideAssignedPawnOverlay)
                VanillaExpandedFramework_CompAssignableToPawn_PlayerCanSeeAssignments__Patch.DefsWithNoAssignmentOverlay.Add(parentDef);
            else if (hideUnownedAssignedPawnOverlay)
                VanillaExpandedFramework_CompAssignableToPawn_PlayerCanSeeAssignments__Patch.DefsWithNoUnownedAssignmentOverlay.Add(parentDef);
        }
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
        public Vector3 drawOffset;
        public Vector3? drawOffsetWhileMoving;
        public float angleOffset;
        public float? angleOffsetWhileMoving;
    }

}