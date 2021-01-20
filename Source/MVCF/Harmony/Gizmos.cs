using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MVCF.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace MVCF.Harmony
{
    [HarmonyPatch(typeof(Pawn_DraftController), "GetGizmos")]
    public class Pawn_DraftController_GetGizmos
    {
        // ReSharper disable InconsistentNaming
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn_DraftController __instance)
            // ReSharper enable InconsistentNaming
        {
            foreach (var gizmo in __result) yield return gizmo;

            if (!__instance.Drafted || !__instance.pawn.AllRangedVerbsPawnNoEquipment().Any() ||
                __instance.pawn.equipment.Primary != null && __instance.pawn.equipment.Primary.def.IsRangedWeapon)
                yield break;
            yield return new Command_Toggle
            {
                hotKey = KeyBindingDefOf.Misc6,
                isActive = () => __instance.FireAtWill,
                toggleAction = () => { __instance.FireAtWill = !__instance.FireAtWill; },
                icon = TexCommand.FireAtWill,
                defaultLabel = "CommandFireAtWillLabel".Translate(),
                defaultDesc = "CommandFireAtWillDesc".Translate(),
                tutorTag = "FireAtWillToggle"
            };
        }
    }

    [HarmonyPatch(typeof(PawnAttackGizmoUtility), "GetAttackGizmos")]
    public class PawnAttackGizmoUtility_GetAttackGizmos
    {
        // ReSharper disable once InconsistentNaming
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn pawn)
        {
            foreach (var gizmo in __result) yield return gizmo;

            if (pawn.Manager().ManagedVerbs.Count(mv => mv.Enabled && !mv.Verb.IsMeleeAttack) >= 2)
                yield return pawn.GetMainAttackGizmoForPawn();

            foreach (var gizmo in from verb in pawn.Manager().ManagedVerbs
                where (verb.Source == VerbSource.Hediff || verb.Source == VerbSource.RaceDef) &&
                      verb.Verb.verbProps.hasStandardCommand
                from gizmo in verb.Verb
                    .GetGizmosForVerb(verb)
                select gizmo)
                yield return gizmo;
        }
    }

    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public class Pawn_GetGizmos
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            foreach (var gizmo in __result) yield return gizmo;

            if (!__instance.RaceProps.Animal || __instance.Faction != Faction.OfPlayer) yield break;
            foreach (var mv in __instance.Manager().ManagedVerbs.Where(mv => !mv.Verb.IsMeleeAttack))
                yield return new Command_ToggleVerbUsage(mv);
        }
    }

    [HarmonyPatch(typeof(Command), "GizmoOnGUIInt")]
    public class Command_GizmoOnGUI
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var list = instructions.ToList();
            var method = AccessTools.Method(typeof(Widgets), "ButtonInvisible");
            var idx = list.FindIndex(ins =>
                ins.opcode == OpCodes.Call && ((MethodInfo) ins.operand).FullDescription() == method.FullDescription());
            var label = list[idx + 1].operand;
            var list2 = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Command_GizmoOnGUI), "DrawToggle")),
                new CodeInstruction(OpCodes.Brtrue_S, label)
            };
            list2[0].labels = list[idx - 2].labels.ListFullCopy();
            list[idx - 2].labels.Clear();
            list.InsertRange(idx - 2, list2);
            return list;
        }

        public static bool DrawToggle(Command command, Rect butRect, bool shrunk)
        {
            if (shrunk) return false;
            if (!(command is Command_VerbTarget gizmo)) return false;
            var verb = gizmo.verb;
            var man = gizmo.verb?.CasterPawn?.Manager()?.GetManagedVerbForVerb(verb);
            if (man?.Props == null || man.Props.separateToggle) return false;
            var rect = command.TopRightLabel.NullOrEmpty()
                ? butRect.RightPart(0.35f).TopPart(0.35f)
                : butRect
                    .LeftPart(0.35f).TopPart(0.35f);
            if (Mouse.IsOver(rect))
            {
                TipSignal sig = "MVCF.ToggleAuto".Translate();
                TooltipHandler.TipRegion(rect, sig);
            }

            if (Widgets.ButtonImage(rect,
                man.Enabled ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
            {
                Event.current.Use();
                man.Enabled = !man.Enabled;
                return true;
            }

            return false;
        }
    }
}