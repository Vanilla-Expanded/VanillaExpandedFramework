using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VEF.Buildings;

public class FacilityExtension : DefModExtension
{
    // If the facility should link to the building on the interaction spots of the building
    public bool linkOnInteractionSpots = false;

    // A Def that this facility is equivalent to, meaning that they're treated as the same facility for the purpose of link amounts.
    public ThingDef equivalentToFacility = null;
    // A list of Facilities that we copy links from
    public List<ThingDef> copyLinksFrom = null;

    // If true, prevents any AffectedByFacilitiesExtension with copyFacilitiesFrom from linking this facility.
	public bool disableAffectedByFacilitiesExtensionLinking = false;
    // A list of buildings that this facility won't automatically link to
    public List<ThingDef> preventAutoLinkingWith = null;

    public override void ResolveReferences(Def parentDef)
    {
        base.ResolveReferences(parentDef);

        if (equivalentToFacility != null)
            VanillaExpandedFramework_CompAffectedByFacilities_CanPotentiallyLinkTo_Patch.isActive = true;

        if (copyLinksFrom != null && parentDef is ThingDef def)
        {
            var parentFacility = def.GetCompProperties<CompProperties_Facility>();
            if (parentFacility != null)
            {
                LongEventHandler.ExecuteWhenFinished(() =>
                {
                    parentFacility.linkableBuildings ??= [];

                    foreach (var otherFacility in copyLinksFrom)
                    {
                        var links = otherFacility.GetCompProperties<CompProperties_Facility>()?.linkableBuildings;
                        if (links == null)
                            continue;

                        foreach (var link in links)
                        {
                            // Don't link with specified buildings
                            if (preventAutoLinkingWith.NotNullAndContains(link))
                                continue;
                            // Don't double link
                            if (parentFacility.linkableBuildings.Contains(link))
                                continue;
                            // Make sure that we're trying to link with a building affected by facilities
                            var comp = link.GetCompProperties<CompProperties_AffectedByFacilities>();
                            if (comp == null)
                                continue;
                            // If the building is marked as "don't link into" (globally or with this facility), skip it
                            if (link.GetModExtension<AffectedByFacilitiesExtension>()?.RefusesAutoLinkFrom(def) == true)
                                continue;

                            // Add linking to both
                            comp.linkableFacilities ??= [];
                            comp.linkableFacilities.AddDistinct(def);
                            parentFacility.linkableBuildings.Add(link);
                        }
                    }
                });
            }
        }
    }

    // True if a building copying links via AffectedByFacilitiesExtension.copyLinksFrom should not link with this facility
    public bool RefusesAutoLinkFrom(ThingDef building) => disableAffectedByFacilitiesExtensionLinking || preventAutoLinkingWith.NotNullAndContains(building);

    public static bool AreFacilitiesEquivalent(ThingDef currentlyLinkedFacility, ThingDef newFacility)
    {
        // If the 2 facilities are identical, return true
        if (currentlyLinkedFacility == newFacility)
            return true;

        // If the new facility has the extension with a non-null equivalent facility,
        // and that facility matches the currently linked one, return true.
        var newFacilityExtension = newFacility.GetModExtension<FacilityExtension>();
        if (newFacilityExtension?.equivalentToFacility != null && newFacilityExtension.equivalentToFacility == currentlyLinkedFacility)
            return true;

        // Check if the currently linked facility has the extension with a non-null equivalent facility.
        var currentlyLinkedFacilityExtension = currentlyLinkedFacility.GetModExtension<FacilityExtension>();
        if (currentlyLinkedFacilityExtension?.equivalentToFacility != null)
        {
            // If the equivalent facility matches the new one, return true
            if (currentlyLinkedFacilityExtension.equivalentToFacility == newFacility)
                return true;
            // If the equivalent facility of both extension matches, return true.
            if (currentlyLinkedFacilityExtension.equivalentToFacility == newFacilityExtension?.equivalentToFacility)
                return true;
        }

        return false;
    }
}