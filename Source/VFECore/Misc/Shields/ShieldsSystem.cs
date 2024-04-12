using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace VFECore.Shields
{
    public static class ShieldsSystem
    {
        public static Dictionary<Pawn, List<HediffComp_Draw>> HediffDrawsByPawn = new Dictionary<Pawn, List<HediffComp_Draw>>();

        private static bool          drawPatchesApplied;
        private static bool          shieldPatchesApplied;
        private static HarmonyMethod MyMethod(this string name) => new HarmonyMethod(typeof(ShieldsSystem), name);

        public static void ApplyDrawPatches()
        {
            if (drawPatchesApplied) return;
            drawPatchesApplied = true;
            VFECore.harmonyInstance.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.SpawnSetup)), postfix: nameof(OnPawnSpawn).MyMethod());
            VFECore.harmonyInstance.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.DeSpawn)),    postfix: nameof(OnPawnDespawn).MyMethod());
            VFECore.harmonyInstance.Patch(AccessTools.Method(typeof(Pawn), "DrawAt"),     postfix: nameof(PawnPostDrawAt).MyMethod());
        }

        public static void ApplyShieldPatches()
        {
            if (shieldPatchesApplied) return;
            shieldPatchesApplied = true;
            VFECore.harmonyInstance.Patch(AccessTools.Method(typeof(ThingWithComps), nameof(ThingWithComps.PreApplyDamage)),
                postfix: nameof(PostPreApplyDamage).MyMethod());
            VFECore.harmonyInstance.Patch(AccessTools.Method(typeof(Verb), nameof(Verb.CanHitTarget)), postfix: nameof(CanHitTargetFrom_Postfix).MyMethod());
        }

        public static void OnPawnSpawn(Pawn __instance)
        {
            HediffDrawsByPawn.Add(__instance,
                __instance.health.hediffSet.hediffs.OfType<HediffWithComps>().SelectMany(hediff => hediff.comps).OfType<HediffComp_Draw>().ToList());
        }

        public static void OnPawnDespawn(Pawn __instance)
        {
            HediffDrawsByPawn.Remove(__instance);
        }

        public static void PawnPostDrawAt(Pawn __instance, Vector3 drawLoc)
        {
            if (HediffDrawsByPawn.TryGetValue(__instance, out var list))
                for (var i = 0; i < list.Count; i++)
                    list[i].DrawAt(drawLoc);
        }

        public static void PostPreApplyDamage(ThingWithComps __instance, ref DamageInfo dinfo, ref bool absorbed)
        {
            if (absorbed || !(__instance is Pawn pawn)) return;
            foreach (var shield in pawn.health.hediffSet.hediffs.OfType<HediffWithComps>().SelectMany(hediff => hediff.comps).OfType<HediffComp_Shield>())
            {
                shield.PreApplyDamage(ref dinfo, ref absorbed);
                if (absorbed) break;
            }
        }

        public static void CanHitTargetFrom_Postfix(Verb __instance, ref bool __result)
        {
            if (__result && __instance.CasterIsPawn && __instance.CasterPawn is {} pawn
             && pawn.health.hediffSet.hediffs.OfType<HediffWithComps>()
                   .SelectMany(hediff => hediff.comps)
                   .OfType<HediffComp_Shield>()
                   .Any(shield => !shield.AllowVerbCast(__instance)))
                __result = false;
        }
    }
}
