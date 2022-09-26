using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VFECore
{

    [StaticConstructorOnStartup]
    public static class NonPublicMethods
    {

        public static ApplyArmourDelegate<float, float, float, Thing, DamageDef, Pawn, bool> ArmorUtility_ApplyArmor = (ApplyArmourDelegate<float, float, float, Thing, DamageDef, Pawn, bool>)
            Delegate.CreateDelegate(typeof(ApplyArmourDelegate<float, float, float, Thing, DamageDef, Pawn, bool>), AccessTools.Method(typeof(ArmorUtility), "ApplyArmor"));

        public static Func<IntVec3, Rot4, ThingDef, Map, ThingDef, bool> SiegeBlueprintPlacer_CanPlaceBlueprintAt = (Func<IntVec3, Rot4, ThingDef, Map, ThingDef, bool>)
            Delegate.CreateDelegate(typeof(Func<IntVec3, Rot4, ThingDef, Map, ThingDef, bool>), AccessTools.Method(typeof(SiegeBlueprintPlacer), "CanPlaceBlueprintAt"));
        public static Func<ThingDef, Rot4, Map, IntVec3> SiegeBlueprintPlacer_FindArtySpot = (Func<ThingDef, Rot4, Map, IntVec3>)
            Delegate.CreateDelegate(typeof(Func<ThingDef, Rot4, Map, IntVec3>), AccessTools.Method(typeof(SiegeBlueprintPlacer), "FindArtySpot"));

        public delegate void ApplyArmourDelegate<A, B, C, D, E, F, G>(ref A first, B second, C third, D fourth, ref E fifth, F sixth, out G seventh);
        public delegate C FuncOut<A, B, C>(A first, out B second);

        public static Action<Projectile> Projectile_ImpactSomething = (Action<Projectile>)
            Delegate.CreateDelegate(typeof(Action<Projectile>), null, AccessTools.Method(typeof(Projectile), "ImpactSomething"));

        public static Action<Pawn, PawnGenerationRequest> GenerateSkills = (Action<Pawn, PawnGenerationRequest>)
            Delegate.CreateDelegate(typeof(Action<Pawn, PawnGenerationRequest>), null, AccessTools.Method(typeof(PawnGenerator), "GenerateSkills"));

        public static MethodInfo RenderMouseAttachments = AccessTools.Method(typeof(DeepResourceGrid), "RenderMouseAttachments");

        [StaticConstructorOnStartup]
        public static class DualWield
        {
            static DualWield()
            {
                if (ModCompatibilityCheck.DualWield)
                {
                    #region Ext_Pawn_EquipmentTracker
                    var extPawnEquipmentTracker = GenTypes.GetTypeInAnyAssembly("DualWield.Ext_Pawn_EquipmentTracker", "DualWield");

                    Ext_Pawn_EquipmentTracker_MakeRoomForOffHand = (Action<Pawn_EquipmentTracker, ThingWithComps>)
                        Delegate.CreateDelegate(typeof(Action<Pawn_EquipmentTracker, ThingWithComps>), AccessTools.Method(extPawnEquipmentTracker, "MakeRoomForOffHand"));

                    Ext_Pawn_EquipmentTracker_TryGetOffHandEquipment = (FuncOut<Pawn_EquipmentTracker, ThingWithComps, bool>)
                        Delegate.CreateDelegate(typeof(FuncOut<Pawn_EquipmentTracker, ThingWithComps, bool>), AccessTools.Method(extPawnEquipmentTracker, "TryGetOffHandEquipment"));
                    #endregion
                    #region Ext_ThingDef
                    var extThingDef = GenTypes.GetTypeInAnyAssembly("DualWield.Ext_ThingDef", "DualWield");

                    Ext_ThingDef_CanBeOffHand = (Func<ThingDef, bool>)Delegate.CreateDelegate(typeof(Func<ThingDef, bool>), AccessTools.Method(extThingDef, "CanBeOffHand"));
                    Ext_ThingDef_IsTwoHand = (Func<ThingDef, bool>)Delegate.CreateDelegate(typeof(Func<ThingDef, bool>), AccessTools.Method(extThingDef, "IsTwoHand"));
                    #endregion
                }
            }

            public static Action<Pawn_EquipmentTracker, ThingWithComps> Ext_Pawn_EquipmentTracker_MakeRoomForOffHand;
            public static FuncOut<Pawn_EquipmentTracker, ThingWithComps, bool> Ext_Pawn_EquipmentTracker_TryGetOffHandEquipment;

            public static Func<ThingDef, bool> Ext_ThingDef_CanBeOffHand;
            public static Func<ThingDef, bool> Ext_ThingDef_IsTwoHand;

        }

        [StaticConstructorOnStartup]
        public static class RimCities
        {
            static RimCities()
            {
                if (ModCompatibilityCheck.RimCities)
                {
                    var genCity = GenTypes.GetTypeInAnyAssembly("Cities.GenCity", "Cities");

                    GenCity_RandomCityFaction = (Func<Predicate<Faction>, Faction>)
                        Delegate.CreateDelegate(typeof(Func<Predicate<Faction>, Faction>), AccessTools.Method(genCity, "RandomCityFaction"));
                }
            }

            public static Func<Predicate<Faction>, Faction> GenCity_RandomCityFaction;
        }

    }

}
