using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace VFECore
{

    [StaticConstructorOnStartup]
    public static class NonPublicMethods
    {

        public static ApplyArmourDelegate<float, float, float, Thing, DamageDef, Pawn, bool> ArmorUtility_ApplyArmor = (ApplyArmourDelegate<float, float, float, Thing, DamageDef, Pawn, bool>)
            Delegate.CreateDelegate(typeof(ApplyArmourDelegate<float, float, float, Thing, DamageDef, Pawn, bool>), AccessTools.Method(typeof(ArmorUtility), "ApplyArmor"));

        public static Action<CompUseEffect_FinishRandomResearchProject, ResearchProjectDef, Pawn> CompUseEffect_FinishRandomResearchProject_FinishInstantly = (Action<CompUseEffect_FinishRandomResearchProject, ResearchProjectDef, Pawn>)
            Delegate.CreateDelegate(typeof(Action<CompUseEffect_FinishRandomResearchProject, ResearchProjectDef, Pawn>), null, AccessTools.Method(typeof(CompUseEffect_FinishRandomResearchProject), "FinishInstantly"));

        public static Func<IntVec3, Rot4, ThingDef, Map, bool> SiegeBlueprintPlacer_CanPlaceBlueprintAt = (Func<IntVec3, Rot4, ThingDef, Map, bool>)
            Delegate.CreateDelegate(typeof(Func<IntVec3, Rot4, ThingDef, Map, bool>), AccessTools.Method(typeof(SiegeBlueprintPlacer), "CanPlaceBlueprintAt"));
        public static Func<ThingDef, Rot4, Map, IntVec3> SiegeBlueprintPlacer_FindArtySpot = (Func<ThingDef, Rot4, Map, IntVec3>)
            Delegate.CreateDelegate(typeof(Func<ThingDef, Rot4, Map, IntVec3>), AccessTools.Method(typeof(SiegeBlueprintPlacer), "FindArtySpot"));
        public static Func<float, Map, IEnumerable<Blueprint_Build>> SiegeBlueprintPlacer_PlaceArtilleryBlueprints = (Func<float, Map, IEnumerable<Blueprint_Build>>)
            Delegate.CreateDelegate(typeof(Func<float, Map, IEnumerable<Blueprint_Build>>), AccessTools.Method(typeof(SiegeBlueprintPlacer), "PlaceArtilleryBlueprints"));
        public static Func<Map, IEnumerable<Blueprint_Build>> SiegeBlueprintPlacer_PlaceSandbagBlueprints = (Func<Map, IEnumerable<Blueprint_Build>>)
            Delegate.CreateDelegate(typeof(Func<Map, IEnumerable<Blueprint_Build>>), AccessTools.Method(typeof(SiegeBlueprintPlacer), "PlaceSandbagBlueprints"));

        public static Func<Thing, StatDef, string> StatWorker_InfoTextLineFromGear = (Func<Thing, StatDef, string>)
            Delegate.CreateDelegate(typeof(Func<Thing, StatDef, string>), null, AccessTools.Method(typeof(StatWorker), "InfoTextLineFromGear"));
        public static Func<Thing, StatDef, float> StatWorker_StatOffsetFromGear = (Func<Thing, StatDef, float>)
            Delegate.CreateDelegate(typeof(Func<Thing, StatDef, float>), null, AccessTools.Method(typeof(StatWorker), "StatOffsetFromGear"));

        public delegate void ApplyArmourDelegate<A, B, C, D, E, F, G>(ref A first, B second, C third, D fourth, ref E fifth, F sixth, out G seventh);
        public delegate C FuncOut<A, B, C>(A first, out B second);

        [StaticConstructorOnStartup]
        public static class DualWield
        {
            static DualWield()
            {
                if (ModCompatibilityCheck.DualWield)
                {
                    var extPawnEquipmentTracker = GenTypes.GetTypeInAnyAssemblyNew("DualWield.Ext_Pawn_EquipmentTracker", "DualWield");

                    Ext_Pawn_EquipmentTracker_MakeRoomForOffHand = (Action<Pawn_EquipmentTracker, ThingWithComps>)
                        Delegate.CreateDelegate(typeof(Action<Pawn_EquipmentTracker, ThingWithComps>), AccessTools.Method(extPawnEquipmentTracker, "MakeRoomForOffHand"));

                    Ext_Pawn_EquipmentTracker_TryGetOffHandEquipment = (FuncOut<Pawn_EquipmentTracker, ThingWithComps, bool>)
                        Delegate.CreateDelegate(typeof(FuncOut<Pawn_EquipmentTracker, ThingWithComps, bool>), AccessTools.Method(extPawnEquipmentTracker, "TryGetOffHandEquipment"));
                }
            }

            public static Action<Pawn_EquipmentTracker, ThingWithComps> Ext_Pawn_EquipmentTracker_MakeRoomForOffHand;
            public static FuncOut<Pawn_EquipmentTracker, ThingWithComps, bool> Ext_Pawn_EquipmentTracker_TryGetOffHandEquipment;
        }

        [StaticConstructorOnStartup]
        public static class RimCities
        {
            static RimCities()
            {
                if (ModCompatibilityCheck.RimCities)
                {
                    var genCity = GenTypes.GetTypeInAnyAssemblyNew("Cities.GenCity", "Cities");

                    GenCity_RandomCityFaction = (Func<Predicate<Faction>, Faction>)
                        Delegate.CreateDelegate(typeof(Func<Predicate<Faction>, Faction>), AccessTools.Method(genCity, "RandomCityFaction"));
                }
            }

            public static Func<Predicate<Faction>, Faction> GenCity_RandomCityFaction;
        }

    }

}
