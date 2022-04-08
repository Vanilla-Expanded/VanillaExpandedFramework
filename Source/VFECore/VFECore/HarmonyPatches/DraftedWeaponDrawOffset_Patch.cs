using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace VFECore
{
    // Offset the position and rotation of weapons on drafted pawns, if custom offset data is provided for the weapon
    [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.DrawEquipmentAiming))]
    public static class DraftedWeaponDrawOffset_Patch
    {
        static readonly FieldInfo pawnField;
        static readonly MethodInfo getCarryWeaponOpenly;

        static readonly Dictionary<ThingDef, ThingDefExtension> thingExtensionCache;

        static DraftedWeaponDrawOffset_Patch()
        {
            pawnField = typeof(PawnRenderer).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
            getCarryWeaponOpenly = typeof(PawnRenderer).GetMethod("CarryWeaponOpenly", BindingFlags.NonPublic | BindingFlags.Instance);

            thingExtensionCache = new Dictionary<ThingDef, ThingDefExtension>();
        }

        public static ThingDefExtension GetThingDefExtension(ThingDef thingDef)
        {
            if (thingExtensionCache.ContainsKey(thingDef))
            {
                return thingExtensionCache[thingDef];
            }
            else
            {
                var ext = thingDef.GetModExtension<ThingDefExtension>();
                thingExtensionCache.Add(thingDef, ext);
                return ext;
            }
        }

        // Highest priority, since we're altering the initial rendering angle used by the rest of the original method
        [HarmonyPriority(Priority.First)]
        public static void Prefix(PawnRenderer __instance, Thing eq, ref Vector3 drawLoc, ref float aimAngle)
        {
            // Get and cache, as it is faster to retreive from dictionary than iterating over all mod extensions every time
            ThingDefExtension thingDefExtension = GetThingDefExtension(eq.def);

            // Check if the extension is null first, prevent other method calls
            if (thingDefExtension?.draftedDrawOffsets != null)
            {
                var canCarryWeaponOpenly = getCarryWeaponOpenly?.Invoke(__instance, null);

                if (canCarryWeaponOpenly != null && (bool)canCarryWeaponOpenly)
                {
                    // Get pawn only if canCarryWeaponOpenly isn't null, instead of before, prevent method call
                    Pawn pawn = pawnField.GetValue(__instance) as Pawn;

                    // Every offset is null by default, faster than checking if two vect are equal
                    // I've been told it is better to call getter only once and store the value
                    Rot4 pawnRot = pawn.Rotation;
                    if (pawnRot == Rot4.South && thingDefExtension.draftedDrawOffsets.south != null)
                    {
                        drawLoc -= new Vector3(0f, 0f, -0.22f) - thingDefExtension.draftedDrawOffsets.south.posOffset;
                        aimAngle = thingDefExtension.draftedDrawOffsets.south.angOffset;
                    }
                    else if (pawnRot == Rot4.North && thingDefExtension.draftedDrawOffsets.north != null)
                    {
                        drawLoc -= new Vector3(0f, 0f, -0.11f) - thingDefExtension.draftedDrawOffsets.north.posOffset;
                        aimAngle = thingDefExtension.draftedDrawOffsets.north.angOffset;
                    }
                    else if (pawnRot == Rot4.East && thingDefExtension.draftedDrawOffsets.east.posOffset != null)
                    {
                        drawLoc -= new Vector3(0.2f, 0f, -0.22f) - thingDefExtension.draftedDrawOffsets.east.posOffset;
                        aimAngle = thingDefExtension.draftedDrawOffsets.east.angOffset;
                    }
                    else if (pawnRot == Rot4.West && thingDefExtension.draftedDrawOffsets.west.posOffset != null)
                    {
                        drawLoc -= new Vector3(-0.2f, 0f, -0.22f) - thingDefExtension.draftedDrawOffsets.west.posOffset;
                        aimAngle = thingDefExtension.draftedDrawOffsets.west.angOffset;
                    }
                }
            }
        }
    }
}