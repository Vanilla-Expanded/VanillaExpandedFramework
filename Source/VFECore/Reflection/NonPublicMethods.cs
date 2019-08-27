using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFECore
{

    [StaticConstructorOnStartup]
    public static class NonPublicMethods
    {

        public static ApplyArmourDelegate<float, float, float, Thing, DamageDef, Pawn, bool> ArmorUtility_ApplyArmor = (ApplyArmourDelegate<float, float, float, Thing, DamageDef, Pawn, bool>)
            Delegate.CreateDelegate(typeof(ApplyArmourDelegate<float, float, float, Thing, DamageDef, Pawn, bool>), typeof(ArmorUtility).GetMethod("ApplyArmor", BindingFlags.NonPublic | BindingFlags.Static));

        public static Func<ThingDef, Rot4, Map, IntVec3> SiegeBlueprintPlacer_FindArtySpot = (Func<ThingDef, Rot4, Map, IntVec3>)
            Delegate.CreateDelegate(typeof(Func<ThingDef, Rot4, Map, IntVec3>), typeof(SiegeBlueprintPlacer).GetMethod("FindArtySpot", BindingFlags.NonPublic | BindingFlags.Static));
        public static Func<float, Map, IEnumerable<Blueprint_Build>> SiegeBlueprintPlacer_PlaceArtilleryBlueprints = (Func<float, Map, IEnumerable<Blueprint_Build>>)
            Delegate.CreateDelegate(typeof(Func<float, Map, IEnumerable<Blueprint_Build>>), typeof(SiegeBlueprintPlacer).GetMethod("PlaceArtilleryBlueprints", BindingFlags.NonPublic | BindingFlags.Static));

        public static Func<Thing, StatDef, string> StatWorker_InfoTextLineFromGear = (Func<Thing, StatDef, string>)
            Delegate.CreateDelegate(typeof(Func<Thing, StatDef, string>), typeof(StatWorker).GetMethod("InfoTextLineFromGear", BindingFlags.NonPublic | BindingFlags.Static));
        public static Func<Thing, StatDef, float> StatWorker_StatOffsetFromGear = (Func<Thing, StatDef, float>)
            Delegate.CreateDelegate(typeof(Func<Thing, StatDef, float>), typeof(StatWorker).GetMethod("StatOffsetFromGear", BindingFlags.NonPublic | BindingFlags.Static));

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
                        Delegate.CreateDelegate(typeof(Action<Pawn_EquipmentTracker, ThingWithComps>), extPawnEquipmentTracker.GetMethod("MakeRoomForOffHand", BindingFlags.Public | BindingFlags.Static));

                    Ext_Pawn_EquipmentTracker_TryGetOffHandEquipment = (FuncOut<Pawn_EquipmentTracker, ThingWithComps, bool>)
                        Delegate.CreateDelegate(typeof(FuncOut<Pawn_EquipmentTracker, ThingWithComps, bool>), extPawnEquipmentTracker.GetMethod("TryGetOffHandEquipment", BindingFlags.Public | BindingFlags.Static));
                }
            }

            public static Action<Pawn_EquipmentTracker, ThingWithComps> Ext_Pawn_EquipmentTracker_MakeRoomForOffHand;
            public static FuncOut<Pawn_EquipmentTracker, ThingWithComps, bool> Ext_Pawn_EquipmentTracker_TryGetOffHandEquipment;
        }

    }

}
