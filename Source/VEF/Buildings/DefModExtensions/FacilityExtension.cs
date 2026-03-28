using Verse;

namespace VEF.Buildings;

public class FacilityExtension : DefModExtension
{
    public bool linkOnInteractionSpots = false;

    public ThingDef equivalentToFacility = null;

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