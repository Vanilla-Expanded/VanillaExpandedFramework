using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using MonoMod.Utils;
using Verse;

namespace MVCF.Features.PatchSets
{
    public class PatchSet_DualWield : PatchSet
    {
        public override IEnumerable<Patch> GetPatches()
        {
            yield return Patch.Transpiler(AccessTools.Method(AccessTools.TypeByName("DualWield.Command_DualWield"), "ProcessInput"),
                AccessTools.Method(GetType(), nameof(Transpiler)));
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Command_VerbTarget), nameof(Command_VerbTarget.ProcessInput)));
            yield return new CodeInstruction(OpCodes.Ret);
        }
    }

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

        public static bool IsOffHand(this ThingWithComps t) => isOffHand(t);

        private delegate bool TryGetOffHandEquipmentType(Pawn_EquipmentTracker tracker, out ThingWithComps result);
    }
}