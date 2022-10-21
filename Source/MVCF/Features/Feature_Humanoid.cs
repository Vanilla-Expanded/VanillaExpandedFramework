using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MVCF.Features.PatchSets;
using MVCF.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace MVCF.Features;

public abstract class Feature_Humanoid : Feature
{
    public override IEnumerable<Patch> GetPatches()
    {
        foreach (var patch in base.GetPatches()) yield return patch;

        yield return Patch.Postfix(AccessTools.Method(typeof(Pawn_DraftController), "GetGizmos"),
            AccessTools.Method(GetType(), nameof(GetGizmos_Postfix)));
        yield return Patch.Postfix(AccessTools.Method(typeof(PawnAttackGizmoUtility), "GetAttackGizmos"),
            AccessTools.Method(GetType(), nameof(GetAttackGizmos_Postfix)));
        var type = typeof(Pawn_EquipmentTracker);
        yield return Patch.Postfix(AccessTools.Method(type, "Notify_EquipmentAdded"),
            AccessTools.Method(GetType(), nameof(EquipmentAdded_Postfix)));
        yield return Patch.Prefix(AccessTools.Method(type, "Notify_EquipmentRemoved"),
            AccessTools.Method(GetType(), nameof(EquipmentRemoved_Prefix)));
    }

    public override IEnumerable<PatchSet> GetPatchSets()
    {
        foreach (var patchSet in base.GetPatchSets()) yield return patchSet;
        yield return new PatchSet_Brawlers();
        yield return new PatchSet_Hunting();
        if (ModLister.HasActiveModWithName("Dual Wield")) yield return new PatchSet_DualWield();
    }

    public static IEnumerable<Gizmo> GetGizmos_Postfix(IEnumerable<Gizmo> __result, Pawn_DraftController __instance)
    {
        foreach (var gizmo in __result) yield return gizmo;

        if (!__instance.Drafted || (__instance.pawn.equipment.Primary != null && __instance.pawn.equipment
                .Primary.def.IsRangedWeapon) || !__instance.pawn.Manager().AllRangedVerbsNoEquipment.Any())
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
                mv.Enabled && !mv.Verb.IsMeleeAttack && mv.Props is not { canFireIndependently: true }) >= 2)
            yield return pawn.GetMainAttackGizmoForPawn();

        foreach (var gizmo in from verb in man.ManagedVerbs
                 where verb.Source is VerbSource.Hediff or VerbSource.RaceDef &&
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

    public static void EquipmentAdded_Postfix(ThingWithComps eq, Pawn_EquipmentTracker __instance)
    {
        __instance.pawn.Manager(false)?.AddVerbs(eq);
    }

    public static void EquipmentRemoved_Prefix(ThingWithComps eq, Pawn_EquipmentTracker __instance)
    {
        if (Base.IsIgnoredMod(eq?.def?.modContentPack?.Name)) return;
        if (Base.ShouldIgnore(eq)) return;
        if (DualWieldCompat.Active && eq.IsOffHand()) return;
        var comp = eq.TryGetComp<CompEquippable>();
        if (comp?.VerbTracker?.AllVerbs == null) return;
        var manager = __instance?.pawn?.Manager(false);
        if (manager == null) return;
        try { foreach (var verb in comp.VerbTracker.AllVerbs.Concat(manager.ExtraVerbsFor(eq))) manager.RemoveVerb(verb); } catch (NullReferenceException) { }
    }
}