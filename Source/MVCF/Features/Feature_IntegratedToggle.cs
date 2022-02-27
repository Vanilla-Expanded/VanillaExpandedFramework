using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using MVCF.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace MVCF.Features
{
    public class Feature_IntegratedToggle : Feature
    {
        public override string Name => "IntegratedToggle";

        public override IEnumerable<Patch> GetPatches()
        {
            foreach (var patch in base.GetPatches()) yield return patch;

            yield return Patch.Transpiler(AccessTools.Method(typeof(Command), "GizmoOnGUIInt"), AccessTools.Method(GetType(), nameof(GizmoOnGUI_Transpile)));
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
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldarg_1),
                new(OpCodes.Ldarg_2),
                new(OpCodes.Call, AccessTools.Method(typeof(Feature_IntegratedToggle), nameof(DrawToggle))),
                new(OpCodes.Brtrue_S, label)
            };
            list2[0].labels = list[idx].labels.ListFullCopy();
            list[idx].labels.Clear();
            list.InsertRange(idx, list2);
            return list;
        }

        public static bool DrawToggle(Command command, Rect butRect, GizmoRenderParms parms)
        {
            if (parms.shrunk) return false;
            if (command is not Command_VerbTarget gizmo) return false;
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

            if (Widgets.ButtonImage(rect, man.GetToggleStatus() ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
            {
                Event.current.Use();
                man.Toggle();
                return true;
            }

            return false;
        }
    }
}