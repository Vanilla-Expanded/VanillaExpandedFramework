using System;
using HarmonyLib;
using MonoMod.Utils;
using Verse;

namespace MVCF.ModCompat;

[StaticConstructorOnStartup]
public static class DualWieldCompat
{
    public static bool Active;
    private static readonly Func<ThingWithComps, bool> isOffHand;

    private static readonly TryGetOffHandEquipmentType tryGetOffHandEquipment;

    static DualWieldCompat()
    {
        if (!ModLister.HasActiveModWithName("Dual Wield")) return;
        Log.Message("[MVCF] Activating Dual Wield compatibility...");
        Active = true;
        isOffHand = AccessTools.Method(AccessTools.TypeByName("DualWield.Ext_ThingWithComps"), "IsOffHand").CreateDelegate<Func<ThingWithComps, bool>>();
        tryGetOffHandEquipment = AccessTools.Method(AccessTools.TypeByName("DualWield.Ext_Pawn_EquipmentTracker"), "TryGetOffHandEquipment")
            .CreateDelegate<TryGetOffHandEquipmentType>();
    }

    public static bool HasOffHand(this Pawn pawn) => tryGetOffHandEquipment(pawn.equipment, out _);

    public static ThingWithComps GetOffHand(this Pawn pawn) => tryGetOffHandEquipment(pawn.equipment, out var eq) ? eq : null;

    public static bool IsOffHand(this ThingWithComps t) => isOffHand(t);

    private delegate bool TryGetOffHandEquipmentType(Pawn_EquipmentTracker tracker, out ThingWithComps result);
}