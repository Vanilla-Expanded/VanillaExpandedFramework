using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaStorytellersExpanded
{
	// Token: 0x0200001C RID: 28
	[StaticConstructorOnStartup]
	public static class NonPublicFields
	{
		// Token: 0x04000035 RID: 53
		public static FieldInfo Pawn_EquipmentTracker_equipment = AccessTools.Field(typeof(Pawn_EquipmentTracker), "equipment");

		// Token: 0x04000036 RID: 54
		public static FieldInfo Pawn_HealthTracker_pawn = AccessTools.Field(typeof(Pawn_HealthTracker), "pawn");

		// Token: 0x04000037 RID: 55
		public static FieldInfo PawnRenderer_pawn = AccessTools.Field(typeof(PawnRenderer), "pawn");

		// Token: 0x04000038 RID: 56
		public static FieldInfo SiegeBlueprintPlacer_center = AccessTools.Field(typeof(SiegeBlueprintPlacer), "center");

		// Token: 0x04000039 RID: 57
		public static FieldInfo SiegeBlueprintPlacer_faction = AccessTools.Field(typeof(SiegeBlueprintPlacer), "faction");

		// Token: 0x0400003A RID: 58
		public static FieldInfo SiegeBlueprintPlacer_NumCoverRange = AccessTools.Field(typeof(SiegeBlueprintPlacer), "NumCoverRange");

		// Token: 0x0400003B RID: 59
		public static FieldInfo SiegeBlueprintPlacer_placedCoverLocs = AccessTools.Field(typeof(SiegeBlueprintPlacer), "placedCoverLocs");

		// Token: 0x0400003C RID: 60
		public static FieldInfo SiegeBlueprintPlacer_CoverLengthRange = AccessTools.Field(typeof(SiegeBlueprintPlacer), "CoverLengthRange");
	}
}
