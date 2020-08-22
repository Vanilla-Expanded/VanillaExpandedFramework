using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaStorytellersExpanded
{
	// Token: 0x0200001D RID: 29
	[StaticConstructorOnStartup]
	public static class NonPublicMethods
	{
		// Token: 0x0400003D RID: 61
		public static NonPublicMethods.ApplyArmourDelegate<float, float, float, Thing, DamageDef, Pawn, bool> ArmorUtility_ApplyArmor = (NonPublicMethods.ApplyArmourDelegate<float, float, float, Thing, DamageDef, Pawn, bool>)Delegate.CreateDelegate(typeof(NonPublicMethods.ApplyArmourDelegate<float, float, float, Thing, DamageDef, Pawn, bool>), AccessTools.Method(typeof(ArmorUtility), "ApplyArmor", null, null));

		// Token: 0x0400003E RID: 62
		public static Action<CompUseEffect_FinishRandomResearchProject, ResearchProjectDef, Pawn> CompUseEffect_FinishRandomResearchProject_FinishInstantly = (Action<CompUseEffect_FinishRandomResearchProject, ResearchProjectDef, Pawn>)Delegate.CreateDelegate(typeof(Action<CompUseEffect_FinishRandomResearchProject, ResearchProjectDef, Pawn>), null, AccessTools.Method(typeof(CompUseEffect_FinishRandomResearchProject), "FinishInstantly", null, null));

		// Token: 0x0400003F RID: 63
		public static Func<IntVec3, Rot4, ThingDef, Map, ThingDef, bool> SiegeBlueprintPlacer_CanPlaceBlueprintAt = (Func<IntVec3, Rot4, ThingDef, Map, ThingDef, bool>)Delegate.CreateDelegate(typeof(Func<IntVec3, Rot4, ThingDef, Map, ThingDef, bool>), AccessTools.Method(typeof(SiegeBlueprintPlacer), "CanPlaceBlueprintAt", null, null));

		// Token: 0x04000040 RID: 64
		public static Func<ThingDef, Rot4, Map, IntVec3> SiegeBlueprintPlacer_FindArtySpot = (Func<ThingDef, Rot4, Map, IntVec3>)Delegate.CreateDelegate(typeof(Func<ThingDef, Rot4, Map, IntVec3>), AccessTools.Method(typeof(SiegeBlueprintPlacer), "FindArtySpot", null, null));

		// Token: 0x04000041 RID: 65
		public static Func<float, Map, IEnumerable<Blueprint_Build>> SiegeBlueprintPlacer_PlaceArtilleryBlueprints = (Func<float, Map, IEnumerable<Blueprint_Build>>)Delegate.CreateDelegate(typeof(Func<float, Map, IEnumerable<Blueprint_Build>>), AccessTools.Method(typeof(SiegeBlueprintPlacer), "PlaceArtilleryBlueprints", null, null));

		// Token: 0x04000042 RID: 66
		public static Func<Map, IEnumerable<Blueprint_Build>> SiegeBlueprintPlacer_PlaceCoverBlueprints = (Func<Map, IEnumerable<Blueprint_Build>>)Delegate.CreateDelegate(typeof(Func<Map, IEnumerable<Blueprint_Build>>), AccessTools.Method(typeof(SiegeBlueprintPlacer), "PlaceCoverBlueprints", null, null));

		// Token: 0x04000043 RID: 67
		public static Func<Thing, StatDef, string> StatWorker_InfoTextLineFromGear = (Func<Thing, StatDef, string>)Delegate.CreateDelegate(typeof(Func<Thing, StatDef, string>), null, AccessTools.Method(typeof(StatWorker), "InfoTextLineFromGear", null, null));

		// Token: 0x04000044 RID: 68
		public static Func<Thing, StatDef, float> StatWorker_StatOffsetFromGear = (Func<Thing, StatDef, float>)Delegate.CreateDelegate(typeof(Func<Thing, StatDef, float>), null, AccessTools.Method(typeof(StatWorker), "StatOffsetFromGear", null, null));

		// Token: 0x0200001E RID: 30
		// (Invoke) Token: 0x0600003B RID: 59
		public delegate void ApplyArmourDelegate<A, B, C, D, E, F, G>(ref A first, B second, C third, D fourth, ref E fifth, F sixth, out G seventh);

		// Token: 0x0200001F RID: 31
		// (Invoke) Token: 0x0600003F RID: 63
		public delegate C FuncOut<A, B, C>(A first, out B second);

		// Token: 0x02000020 RID: 32
		[StaticConstructorOnStartup]
		public static class DualWield
		{
			// Token: 0x06000042 RID: 66 RVA: 0x00002F10 File Offset: 0x00001110
			static DualWield()
			{
				bool dualWield = ModCompatibilityCheck.DualWield;
				if (dualWield)
				{
					Type typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly("DualWield.Ext_Pawn_EquipmentTracker", "DualWield");
					NonPublicMethods.DualWield.Ext_Pawn_EquipmentTracker_MakeRoomForOffHand = (Action<Pawn_EquipmentTracker, ThingWithComps>)Delegate.CreateDelegate(typeof(Action<Pawn_EquipmentTracker, ThingWithComps>), AccessTools.Method(typeInAnyAssembly, "MakeRoomForOffHand", null, null));
					NonPublicMethods.DualWield.Ext_Pawn_EquipmentTracker_TryGetOffHandEquipment = (NonPublicMethods.FuncOut<Pawn_EquipmentTracker, ThingWithComps, bool>)Delegate.CreateDelegate(typeof(NonPublicMethods.FuncOut<Pawn_EquipmentTracker, ThingWithComps, bool>), AccessTools.Method(typeInAnyAssembly, "TryGetOffHandEquipment", null, null));
					Type typeInAnyAssembly2 = GenTypes.GetTypeInAnyAssembly("DualWield.Ext_ThingDef", "DualWield");
					NonPublicMethods.DualWield.Ext_ThingDef_CanBeOffHand = (Func<ThingDef, bool>)Delegate.CreateDelegate(typeof(Func<ThingDef, bool>), AccessTools.Method(typeInAnyAssembly2, "CanBeOffHand", null, null));
					NonPublicMethods.DualWield.Ext_ThingDef_IsTwoHand = (Func<ThingDef, bool>)Delegate.CreateDelegate(typeof(Func<ThingDef, bool>), AccessTools.Method(typeInAnyAssembly2, "IsTwoHand", null, null));
				}
			}

			// Token: 0x04000045 RID: 69
			public static Action<Pawn_EquipmentTracker, ThingWithComps> Ext_Pawn_EquipmentTracker_MakeRoomForOffHand;

			// Token: 0x04000046 RID: 70
			public static NonPublicMethods.FuncOut<Pawn_EquipmentTracker, ThingWithComps, bool> Ext_Pawn_EquipmentTracker_TryGetOffHandEquipment;

			// Token: 0x04000047 RID: 71
			public static Func<ThingDef, bool> Ext_ThingDef_CanBeOffHand;

			// Token: 0x04000048 RID: 72
			public static Func<ThingDef, bool> Ext_ThingDef_IsTwoHand;
		}

		// Token: 0x02000021 RID: 33
		[StaticConstructorOnStartup]
		public static class RimCities
		{
			// Token: 0x06000043 RID: 67 RVA: 0x00002FE4 File Offset: 0x000011E4
			static RimCities()
			{
				bool rimCities = ModCompatibilityCheck.RimCities;
				if (rimCities)
				{
					Type typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly("Cities.GenCity", "Cities");
					NonPublicMethods.RimCities.GenCity_RandomCityFaction = (Func<Predicate<Faction>, Faction>)Delegate.CreateDelegate(typeof(Func<Predicate<Faction>, Faction>), AccessTools.Method(typeInAnyAssembly, "RandomCityFaction", null, null));
				}
			}

			// Token: 0x04000049 RID: 73
			public static Func<Predicate<Faction>, Faction> GenCity_RandomCityFaction;
		}
	}
}
