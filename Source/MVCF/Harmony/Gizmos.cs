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
{ // ReSharper disable InconsistentNaming
    public class Gizmos
    {
        private static MethodInfo CreateVerbTargetCommand;

        public static void DoHumanoidPatches(HarmonyLib.Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(Pawn_DraftController), "GetGizmos"),
                postfix: new HarmonyMethod(typeof(Gizmos), "GetGizmos_Postfix"));
            harm.Patch(AccessTools.Method(typeof(PawnAttackGizmoUtility), "GetAttackGizmos"),
                postfix: new HarmonyMethod(typeof(Gizmos), "GetAttackGizmos_Postfix"));
        }

        public static void DoAnimalPatches(HarmonyLib.Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(Pawn), "GetGizmos"),
                postfix: new HarmonyMethod(typeof(Gizmos), "Pawn_GetGizmos_Postfix"));
        }

        public static void DoIntegratedTogglePatches(HarmonyLib.Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(Command), "GizmoOnGUIInt"),
                transpiler: new HarmonyMethod(typeof(Gizmos), "GizmoOnGUI_Transpile"));
        }

        public static void DoExtraEquipmentPatches(HarmonyLib.Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(Pawn), "GetGizmos"),
                postfix: new HarmonyMethod(typeof(Gizmos), "Pawn_GetGizmos_Postfix"));
            harm.Patch(AccessTools.Method(typeof(CompEquippable), "GetVerbsCommands"),
                new HarmonyMethod(typeof(Gizmos), "GetVerbsCommands_Prefix"));
            CreateVerbTargetCommand = AccessTools.Method(typeof(VerbTracker), "CreateVerbTargetCommand");
        }

        public static IEnumerable<Gizmo> GetGizmos_Postfix(IEnumerable<Gizmo> __result, Pawn_DraftController __instance)
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

        public static IEnumerable<Gizmo> GetAttackGizmos_Postfix(IEnumerable<Gizmo> __result, Pawn pawn)
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

            if (pawn.CurJobDef == JobDefOf.AttackStatic && man.CurrentVerb != null)
                yield return new Command_Action
                {
                    defaultLabel = "CommandStopForceAttack".Translate(),
                    defaultDesc = "CommandStopForceAttackDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Halt"),
                    action = delegate
                    {
                        pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                        man.CurrentVerb = null;
                        SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                    },
                    hotKey = KeyBindingDefOf.Misc5
                };
        }

        public static IEnumerable<Gizmo> Pawn_GetGizmos_Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            foreach (var gizmo in __result) yield return gizmo;

            if (__instance.Faction != Faction.OfPlayer) yield break;
            if (!__instance.RaceProps.Animal) yield break;
            var man = __instance.Manager();
            if (man == null) yield break;
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
            foreach (var mv in man.ManagedVerbs.Where(mv => !mv.Verb.IsMeleeAttack))
                if (mv.Verb.verbProps.hasStandardCommand)
                    foreach (var gizmo in mv.Verb.GetGizmosForVerb(mv))
                        yield return gizmo;
                else
                    yield return new Command_ToggleVerbUsage(mv);
        }

        public static bool GetVerbsCommands_Prefix(ref IEnumerable<Command> __result, CompEquippable __instance)
        {
            var rangedVerbs = __instance.AllVerbs.Where(v => !v.IsMeleeAttack).ToList();
            var melee = VerbManager.PreferMelee(__instance.parent);
            if (rangedVerbs.Count <= 1 && !melee) return true;
            var man = __instance.PrimaryVerb?.CasterPawn?.Manager(false);
            __result = rangedVerbs
                .SelectMany(v => v.GetGizmosForVerb(man?.GetManagedVerbForVerb(v)))
                .OfType<Command>();
            if (melee)
                __result = __result.Prepend((Command) CreateVerbTargetCommand.Invoke(__instance.verbTracker,
                    new object[] {__instance.parent, __instance.AllVerbs.First(v => v.verbProps.IsMeleeAttack)}));
            return false;
        }

        public static IEnumerable<CodeInstruction> GizmoOnGUI_Transpile(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var list = instructions.ToList();
            var field = AccessTools.Field(typeof(GizmoGridDrawer), "customActivator");
            var idx = list.FindIndex(ins => ins.LoadsField(field));
            var method = AccessTools.Method(typeof(Widgets), "ButtonInvisible");
            var label = list[list.FindIndex(ins => ins.Calls(method)) + 1].operand;
            var list2 = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Gizmos), nameof(DrawToggle))),
                new CodeInstruction(OpCodes.Brtrue_S, label)
            };
            list2[0].labels = list[idx].labels.ListFullCopy();
            list[idx].labels.Clear();
            list.InsertRange(idx, list2);
            return list;
        }

        public static bool DrawToggle(Command command, Rect butRect, GizmoRenderParms parms)
        {
            if (parms.shrunk) return false;
            if (!(command is Command_VerbTarget gizmo)) return false;
            var verb = gizmo.verb;
            if (!verb.CasterIsPawn) return false;
            var pawn = verb.CasterPawn;
            if (pawn.Faction != Faction.OfPlayer) return false;
            var manager = pawn.Manager(false);
            var man = manager?.GetManagedVerbForVerb(verb, false);
            if (man == null) return false;
            if (man.GetToggleType() != ManagedVerb.ToggleType.Integrated) return false;
            if (!pawn.RaceProps.Animal && (man.Props?.canFireIndependently ?? false) && manager.AllVerbs.Count(v => !v.IsMeleeAttack) <= 1) return false;
            var rect = command.TopRightLabel.NullOrEmpty()
                ? butRect.RightPart(0.35f).TopPart(0.35f)
                : butRect
                    .LeftPart(0.35f).TopPart(0.35f);
            if (Mouse.IsOver(rect)) TooltipHandler.TipRegion(rect, "MVCF.ToggleAuto".Translate());

            if (Widgets.ButtonImage(rect,
                man.Enabled ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
            {
                Event.current.Use();
                man.Toggle();
                return true;
            }

            return false;
        }
    }
}