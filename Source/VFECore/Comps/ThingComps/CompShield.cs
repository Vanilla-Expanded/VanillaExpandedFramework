using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFECore
{

    public class CompShield : ThingComp
    {

        public bool equippedOffHand;

        public CompProperties_Shield Props => (CompProperties_Shield)props;

        public Pawn EquippingPawn
        {
            get
            {
                if (ParentHolder is Pawn_EquipmentTracker equipment)
                    return equipment.pawn;
                return null;
            }
        }

        public bool UsableNow
        {
            get
            {
                if (EquippingPawn != null)
                {
                    // Pawn's primary is this shield
                    var primary = EquippingPawn.equipment.Primary;
                    if (primary == parent)
                        return true;

                    // Too few hands
                    if (!EquippingPawn.CanUseShields())
                        return false;

                    // Dual wielding - has offhand
                    if (ModCompatibilityCheck.DualWield && EquippingPawn.equipment.Primary != null && NonPublicMethods.DualWield.Ext_Pawn_EquipmentTracker_TryGetOffHandEquipment(EquippingPawn.equipment, out ThingWithComps offHand))
                        return false;

                    // Get pawn's primary weapon and check if it is flagged to be usable with shields, as well as the pawn having at least 1 hand
                    if (primary != null)
                        return primary == parent || primary.def.UsableWithShields();
                }

                // No pawn or primary, therefore can be used
                return true;
            }
        }

        public bool CoversBodyPart(BodyPartRecord partRec)
        {
            if (Props.coveredBodyPartGroups == null)
                return false;

            // Go through each covered body part group in Props and each body part group within partRec; return if there are any matches
            return Props.coveredBodyPartGroups.Any(p => partRec.groups.Any(p2 => p == p2));
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref equippedOffHand, "equippedOffHand");
            base.PostExposeData();
        }

    }

}
