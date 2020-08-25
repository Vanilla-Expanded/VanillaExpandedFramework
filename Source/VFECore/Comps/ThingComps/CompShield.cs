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
                    var equipment = EquippingPawn.equipment;

                    // Too few hands
                    if (!EquippingPawn.CanUseShields())
                        return false;

                    // Pawn's primary is this shield or is usable with shields
                    var primary = equipment.Primary;
                    if (primary == parent || primary.def.UsableWithShields())
                        return true;

                    // Primary not usable with shields
                    if (!primary.def.UsableWithShields())
                        return false;

                    // Dual wielding - has offhand
                    if (ModCompatibilityCheck.DualWield && primary != null && NonPublicMethods.DualWield.Ext_Pawn_EquipmentTracker_TryGetOffHandEquipment(EquippingPawn.equipment, out ThingWithComps offHand))
                        return false;
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
