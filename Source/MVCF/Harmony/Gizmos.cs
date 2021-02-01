using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MVCF.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

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

            if (!__instance.Drafted || __instance.pawn.equipment.Primary != null && __instance.pawn.equipment
                .Primary.def.IsRangedWeapon || !__instance.pawn.AllRangedVerbsPawnNoEquipment().Any())
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

            var man = pawn.Manager();

            if (man.ManagedVerbs.Count(mv =>
                mv.Enabled && !mv.Verb.IsMeleeAttack && (mv.Props == null || !mv.Props.canFireIndependently)) >= 2)
                yield return pawn.GetMainAttackGizmoForPawn();

            foreach (var gizmo in from verb in man.ManagedVerbs
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

            if (__instance.Faction != Faction.OfPlayer) yield break;
            var man = __instance.Manager();
            if (__instance.CurJobDef == JobDefOf.AttackStatic && man.CurrentVerb != null)
                yield return new Command_Action
                {
                    defaultLabel = "CommandStopForceAttack".Translate(),
                    defaultDesc = "CommandStopForceAttackDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Halt"),
                    action = delegate
                    {
                        __instance.jobs.EndCurrentJob(JobCondition.InterruptForced);
                        man.CurrentVerb = null;
                        SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                    },
                    hotKey = KeyBindingDefOf.Misc5
                };
            if (!__instance.RaceProps.Animal) yield break;
            foreach (var mv in man.ManagedVerbs.Where(mv => !mv.Verb.IsMeleeAttack))
                if (mv.Verb.verbProps.hasStandardCommand)
                    foreach (var gizmo in mv.Verb.GetGizmosForVerb(mv))
                        yield return gizmo;
                else
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
            var man = gizmo.verb?.caster is IFakeCaster caster
                ? (caster.RealCaster() as Pawn)?.Manager()?.GetManagedVerbForVerb(verb, false)
                : gizmo.verb?.CasterPawn?.Manager(false)?.GetManagedVerbForVerb(verb, false);
            if (man == null) return false;
            if (man.Props != null && man.Props.separateToggle) return false;
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

    [HarmonyPatch(typeof(CompEquippable), "GetVerbsCommands")]
    public class CompEquippable_GetVerbsCommands
    {
        public static bool Prefix(ref IEnumerable<Command> __result, CompEquippable __instance)
        {
            var rangedVerbs = __instance.AllVerbs.Where(v => !v.IsMeleeAttack).ToList();
            if (rangedVerbs.Count <= 1) return true;
            var man = __instance.PrimaryVerb?.CasterPawn?.Manager(false);
            __result = rangedVerbs
                .SelectMany(v => v.GetGizmosForVerb(man?.GetManagedVerbForVerb(v)))
                .OfType<Command>();
            return false;
        }
    }
}