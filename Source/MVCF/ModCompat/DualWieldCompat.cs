using System;
using HarmonyLib;
using MonoMod.Utils;
using Verse;

namespace MVCF.ModCompat;

[StaticConstructorOnStartup]
public static class DualWieldCompat
{
    public static bool Active; // ReSharper disable InconsistentNaming
    private static readonly Func<ThingWithComps, bool> isOffHand;
    public static bool DoNullCheck => Active && mode == Mode.DualWield;

    private static readonly TryGetOffHandEquipmentType tryGetOffHandEquipment;
    private static readonly Func<Thing, bool> isOffHandedWeapon;
    private static readonly Func<Pawn, bool> hasOffHand;
    private static readonly GetOffHand_Int getOffHand;

    private static readonly Mode mode;
// ReSharper enable InconsistentNaming

    static DualWieldCompat()
    {
        if (ModLister.HasActiveModWithName("Dual Wield"))
        {
            Log.Message("[MVCF] Activating Dual Wield compatibility...");
            Active = true;
            mode = Mode.DualWield;
            isOffHand = AccessTools.Method(AccessTools.TypeByName("DualWield.Ext_ThingWithComps"), "IsOffHand").CreateDelegate<Func<ThingWithComps, bool>>();
            tryGetOffHandEquipment = AccessTools.Method(AccessTools.TypeByName("DualWield.Ext_Pawn_EquipmentTracker"), "TryGetOffHandEquipment")
               .CreateDelegate<TryGetOffHandEquipmentType>();
        }

        if (ModLister.HasActiveModWithName("Tacticowl (temporary beta)") || ModLister.HasActiveModWithName("Tacticowl"))
        {
            Log.Message("[MVCF] Activating Tacticowl compatibility...");
            Active = true;
            mode = Mode.Tacticowl;
            isOffHandedWeapon = AccessTools.Method(AccessTools.TypeByName("Tacticowl.DualWieldExtensions"), "IsOffHandedWeapon")
               .CreateDelegate<Func<Thing, bool>>();
            hasOffHand = AccessTools.Method(AccessTools.TypeByName("Tacticowl.DualWieldExtensions"), "HasOffHand")
               .CreateDelegate<Func<Pawn, bool>>();
            getOffHand = AccessTools.Method(AccessTools.TypeByName("Tacticowl.DualWieldExtensions"), "GetOffHander")
               .CreateDelegate<GetOffHand_Int>();
        }
    }

    public static bool HasOffHand(this Pawn pawn) =>
        mode switch
        {
            Mode.DualWield => tryGetOffHandEquipment(pawn.equipment, out _),
            Mode.Tacticowl => hasOffHand(pawn),
            _ => throw new ArgumentOutOfRangeException()
        };

    public static ThingWithComps GetOffHand(this Pawn pawn) =>
        mode switch
        {
            Mode.DualWield => tryGetOffHandEquipment(pawn.equipment, out var eq) ? eq : null,
            Mode.Tacticowl => getOffHand(pawn, out var eq) ? eq : null,
            _ => throw new ArgumentOutOfRangeException()
        };

    public static bool IsOffHand(this ThingWithComps t) =>
        mode switch
        {
            Mode.DualWield => isOffHand(t),
            Mode.Tacticowl => isOffHandedWeapon(t),
            _ => throw new ArgumentOutOfRangeException()
        };

    private enum Mode
    {
        DualWield, Tacticowl
    }

    private delegate bool TryGetOffHandEquipmentType(Pawn_EquipmentTracker tracker, out ThingWithComps result);

    private delegate bool GetOffHand_Int(Pawn pawn, out ThingWithComps thing);
}
