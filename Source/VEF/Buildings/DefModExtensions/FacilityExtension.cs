using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VEF.Buildings;

public class FacilityExtension : DefModExtension
{
    public bool linkOnInteractionSpots = false;

    public ThingDef equivalentToFacility = null;
    public List<ThingDef> copyLinksFrom = null;

    public override void ResolveReferences(Def parentDef)
    {
        base.ResolveReferences(parentDef);

        if (copyLinksFrom != null && parentDef is ThingDef def)
        {
            var parentFacility = def.GetCompProperties<CompProperties_Facility>();
            if (parentFacility != null)
            {
                LongEventHandler.ExecuteWhenFinished(() =>
                {
                    foreach (var otherFacility in copyLinksFrom)
                    {
                        var links = otherFacility.GetCompProperties<CompProperties_Facility>()?.linkableBuildings;
                        if (links == null)
                            continue;

                        foreach (var link in links)
                        {
                            if (parentFacility.linkableBuildings.Contains(link))
                                continue;
                            var comp = link.GetCompProperties<CompProperties_AffectedByFacilities>();
                            if (comp == null)
                                continue;

                            comp.linkableFacilities.Add(def);
                            parentFacility.linkableBuildings.Add(link);
                        }
                    }
                });
            }
        }
    }

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