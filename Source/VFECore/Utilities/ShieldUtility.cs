using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFECore
{

    public static class ShieldUtility
    {

        public static int HandCount(this Pawn pawn)
        {
            int count = 0;
            var hediffSet = pawn.health.hediffSet;

            // Go through each manipulation limb and count any outside segments (i.e. anything that isn't a bone) that aren't missing
            var manipCoreLimbs = pawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ManipulationLimbCore).ToList();
            for (int i = 0; i < manipCoreLimbs.Count; i++)
            {
                var manipCore = manipCoreLimbs[i];
                count += manipCore.GetChildParts(BodyPartTagDefOf.ManipulationLimbSegment).Count(p => p.depth == BodyPartDepth.Outside && !hediffSet.PartIsMissing(p) || hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(p));
            }

            return count;
        }

        public static bool CanUseShields(this Pawn p) => p.HandCount() > 1;

        public static bool IsShield(this Thing thing, out CompShield shieldComp)
        {
            shieldComp = thing.TryGetComp<CompShield>();
            return shieldComp != null;
        }

        public static bool IsShield(this ThingDef tDef)
        {
            return tDef.HasComp(typeof(CompShield));
        }

        public static bool UsableWithShields(this ThingDef def)
        {
            // If Dual Wield is active, return whether or not the weapon isn't two-handed and can be equipped off hand
            if (ModCompatibilityCheck.DualWield)
                return !NonPublicMethods.DualWield.Ext_ThingDef_IsTwoHand(def) && NonPublicMethods.DualWield.Ext_ThingDef_CanBeOffHand(def);

            // Otherwise return based on DefModExtension
            return ThingDefExtension.Get(def).usableWithShields;
        }

        public static ThingWithComps OffHandShield(this Pawn pawn)
        {
            return pawn.apparel.WornApparel.FirstOrDefault(t => t.IsShield(out CompShield shieldComp) && shieldComp.equippedOffHand);
        }

        public static void MakeRoomForShield(this Pawn pawn, ThingWithComps eq)
        {
                if (pawn.OffHandShield() != null)
                {
                    if (pawn.apparel.TryDrop((Apparel)pawn.OffHandShield(), out Apparel thingWithComps, pawn.Position, true))
                    {
                        if (thingWithComps != null)
                        {
                            thingWithComps.SetForbidden(false, true);
                        }
                    }
                    else
                    {
                        Log.Error(pawn + " couldn't make room for shield " + eq, false);
                    }
                }

                // Taranchuk: no idea how to handle this
                //// Prevent infinite looping :P
                //else if (ModCompatibilityCheck.DualWield && NonPublicMethods.DualWield.Ext_Pawn_EquipmentTracker_TryGetOffHandEquipment(equipment, out ThingWithComps eq2))
                //    NonPublicMethods.DualWield.Ext_Pawn_EquipmentTracker_MakeRoomForOffHand(equipment, eq2);

        }

        public static void AddShield(this Pawn pawn, ThingWithComps newEq)
        {
            if (pawn.OffHandShield() != null)
            {
                Log.Error(string.Concat(new object[]
                {
                    "Pawn ",
                    pawn.LabelCap,
                    " got shield ",
                    newEq,
                    " while already having shield "
                }), false);
                return;
            }
            pawn.apparel.Wear((Apparel)newEq);
            newEq.GetComp<CompShield>().equippedOffHand = true;
        }
    }

}
