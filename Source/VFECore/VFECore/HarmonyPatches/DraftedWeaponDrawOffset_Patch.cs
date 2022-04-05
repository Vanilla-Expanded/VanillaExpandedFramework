using HarmonyLib;
using System.Reflection;
using UnityEngine;
using Verse;

namespace VFECore
{
    // Offset the position and rotation of weapons on drafted pawns, if custom offset data is provided for the weapon
    [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.DrawEquipmentAiming))]
    public static class DraftedWeaponDrawOffset_Patch
    {
		static FieldInfo pawnField;
		static MethodInfo getCarryWeaponOpenly;

		static DraftedWeaponDrawOffset_Patch()
		{
			pawnField = typeof(PawnRenderer).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
			getCarryWeaponOpenly = typeof(PawnRenderer).GetMethod("CarryWeaponOpenly", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		// Highest priority, since we're altering the initial rendering angle used by the rest of the original method
		[HarmonyPriority(Priority.First)]
		public static void Prefix(PawnRenderer __instance, Thing eq, ref Vector3 drawLoc, ref float aimAngle)
		{
			Pawn pawn = pawnField.GetValue(__instance) as Pawn;
			ThingDefExtension thingDefExtension = eq.def.GetModExtension<ThingDefExtension>();

			var canCarryWeaponOpenly = getCarryWeaponOpenly?.Invoke(__instance, null);

			if (canCarryWeaponOpenly != null && (bool)canCarryWeaponOpenly && !pawn.stances.curStance.StanceBusy && thingDefExtension?.draftedDrawOffsets != null)
			{
				// As ThingDefExtension enforces default values for undefined parameters in every instance, we only
				// alter rendering of drafted weapons if genuine, non-default values are specified
				// (i.e. *not* Vector3(-999, -999, -999) for position and -999 for rotation)

				if (pawn.Rotation == Rot4.South)
				{
					if (thingDefExtension.draftedDrawOffsets.south.posOffset != new Vector3(-999, -999, -999))
					{
						drawLoc -= new Vector3(0f, 0f, -0.22f) - thingDefExtension.draftedDrawOffsets.south.posOffset;
					}

					if (thingDefExtension.draftedDrawOffsets.south.angOffset != -999f)
					{
						aimAngle = thingDefExtension.draftedDrawOffsets.south.angOffset;
					}
				}
				else if (pawn.Rotation == Rot4.North)
				{
					if (thingDefExtension.draftedDrawOffsets.north.posOffset != new Vector3(-999, -999, -999))
					{
						drawLoc -= new Vector3(0f, 0f, -0.11f) - thingDefExtension.draftedDrawOffsets.north.posOffset;
					}

					if (thingDefExtension.draftedDrawOffsets.north.angOffset != -999f)
					{
						aimAngle = thingDefExtension.draftedDrawOffsets.north.angOffset;
					}
				}
				else if (pawn.Rotation == Rot4.East)
				{
					if (thingDefExtension.draftedDrawOffsets.east.posOffset != new Vector3(-999, -999, -999))
					{
						drawLoc -= new Vector3(0.2f, 0f, -0.22f) - thingDefExtension.draftedDrawOffsets.east.posOffset;
					}

					if (thingDefExtension.draftedDrawOffsets.east.angOffset != -999f)
					{
						aimAngle = thingDefExtension.draftedDrawOffsets.east.angOffset;
					}
				}
				else if (pawn.Rotation == Rot4.West)
				{
					if (thingDefExtension.draftedDrawOffsets.west.posOffset != new Vector3(-999, -999, -999))
					{
						drawLoc -= new Vector3(-0.2f, 0f, -0.22f) - thingDefExtension.draftedDrawOffsets.west.posOffset;
					}

					if (thingDefExtension.draftedDrawOffsets.west.angOffset != -999f)
					{
						aimAngle = thingDefExtension.draftedDrawOffsets.west.angOffset;
					}
				}
			}
		}
	}
}
