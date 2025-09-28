using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;


namespace VEF.Weapons
{
    [StaticConstructorOnStartup]
    internal static class VerbUtility
    {
        public static Dictionary<Verb, VerbProperties> oldProperties = new Dictionary<Verb, VerbProperties>();

        public static void TryModifyThingsVerbs(ThingWithComps thing)
        {
            VanillaExpandedFramework_StatsReportUtility_DrawStatsReport_Patch.interruptWork = true;
            if (thing != null)
            {
                if (thing is Pawn pawn2)
                {
                    var verbRangeMultiplier = pawn2.GetVerbRangeMultiplier();
                    if (verbRangeMultiplier != 1f)
                        foreach (var verb in GetAllVerbs(pawn2))
                        {
                            TryResetVerbProps(verb);
                            ModifyVerb(verb, verbRangeMultiplier);
                        }
                    else
                        ResetVerbs(GetAllVerbs(pawn2));
                }
                else if (thing.def?.Verbs?.Any() ?? false)
                {
                    var verbs = AllVerbsFrom(thing);
                    var curPawn = GetPawnAsHolder(thing);
                    if (curPawn != null)
                    {
                        var verbRangeMultiplier = curPawn.GetVerbRangeMultiplier();
                        if (verbRangeMultiplier != 1f)
                            foreach (var verb in verbs)
                            {
                                TryResetVerbProps(verb);
                                ModifyVerb(verb, verbRangeMultiplier);
                            }
                        else
                            ResetVerbs(verbs);
                    }
                    else
                        ResetVerbs(verbs);
                }
            }

            VanillaExpandedFramework_StatsReportUtility_DrawStatsReport_Patch.interruptWork = false;
        }

        private static void ResetVerbs(List<Verb> verbs)
        {
            foreach (var verb in verbs) TryResetVerbProps(verb);
        }

        private static List<Verb> GetAllVerbs(Pawn pawn)
        {
            var allVerbs = new List<Verb>();
            try
            {
                if (pawn.VerbTracker != null)
                {
                    allVerbs.AddRange(pawn.VerbTracker.AllVerbs);
                }
            } catch (Exception e)
            {
                Log.Error($"[VEF] Error while getting all verbs for pawn {pawn.Name}: {e.Message}");
            }
            try
            {
                if (pawn.equipment != null)
                {
                    allVerbs.AddRange(pawn.equipment.AllEquipmentVerbs);
                }
            } catch (Exception e)
            {
                Log.Error($"[VEF] Error while getting all equipment verbs for pawn {pawn.Name}: {e.Message}");
            }
            try
            {
                if (pawn.apparel != null)
                {
                    allVerbs.AddRange(pawn.apparel.AllApparelVerbs);
                }
            } catch (Exception e)
            {
                Log.Error($"[VEF] Error while getting all apparel verbs for pawn {pawn.Name}: {e.Message}");
            }

            return allVerbs;
        }

        private static List<Verb> AllVerbsFrom(ThingWithComps thingWithComps)
        {
            var allVerbs = new List<Verb>();
            foreach (var comp in thingWithComps.AllComps)
            {
                if (comp is IVerbOwner verbOwner)
                {
                    foreach (var verb in verbOwner.VerbTracker.AllVerbs)
                        allVerbs.Add(verb);
                }
            }
            allVerbs = allVerbs.Distinct().ToList();
            return allVerbs;
        }

        private static void TryResetVerbProps(Verb verb)
        {
            if (oldProperties.TryGetValue(verb, out var oldVerbProps)) verb.verbProps = oldVerbProps;
        }

        private static void ModifyVerb(Verb verb, float verbRangeMultiplier)
        {
            oldProperties[verb] = verb.verbProps;
            if (verbRangeMultiplier != 1f) ModifyVerbRangeBy(verb, verbRangeMultiplier);
        }

        public static Pawn GetPawnAsHolder(this Thing thing)
        {
            var pawn = GetPawnAsHolderInt(thing);
            if (pawn?.carryTracker is not null) // we are filtering dummy outfit pawn here...
            {
                return pawn;
            }
            return pawn;
        }

        private static Pawn GetPawnAsHolderInt(Thing thing)
        {
            if (thing.ParentHolder is Pawn_EquipmentTracker pawn_EquipmentTracker) return pawn_EquipmentTracker.pawn;
            if (thing.ParentHolder is Pawn_ApparelTracker pawn_ApparelTracker) return pawn_ApparelTracker.pawn;
            return null;
        }

        public static float GetVerbRangeMultiplier(this Pawn pawn)
        {
            try
            {
                return pawn.GetStatValueForPawn(VEFDefOf.VEF_VerbRangeFactor, pawn);
            }
            catch
            {
                return 1f;
            }
        }

        private static void ModifyVerbRangeBy(Verb verb, float multiplier)
        {
            //Log.Message("1 Verb: " + verb + " - " + verb.verbProps.range);
            var newProperties = verb.verbProps.MemberwiseClone();
            var field = Traverse.Create(newProperties).Field("range");
            field.SetValue(field.GetValue<float>() * multiplier);
            verb.verbProps = newProperties;
            //Log.Message("2 Verb: " + verb + " - " + verb.verbProps.range);
        }
    }

    [HarmonyPatch(typeof(StatsReportUtility), "DrawStatsReport", typeof(Rect), typeof(Thing))]
    public static class VanillaExpandedFramework_StatsReportUtility_DrawStatsReport_Patch
    {
        public static ThingWithComps weaponToLookUp;

        public static bool interruptWork;

        public static void Prefix(Rect rect, Thing thing, out List<VerbProperties> __state)
        {
            __state = null;
            if (!interruptWork && thing is ThingWithComps weapon && weapon.def.IsRangedWeapon)
            {
                weaponToLookUp = weapon;
                var pawn = weapon.GetPawnAsHolder();
                if (pawn != null)
                {
                    var verbRangeMultiplier = pawn.GetVerbRangeMultiplier();
                    if (verbRangeMultiplier != 1f)
                    {
                        __state = weapon.def.Verbs;
                        var customVerbs = new List<VerbProperties>();
                        foreach (var verbProps in weapon.def.Verbs)
                        {
                            var newProps = verbProps.MemberwiseClone();
                            var fieldRange = Traverse.Create(newProps).Field("range");
                            fieldRange.SetValue(fieldRange.GetValue<float>() * verbRangeMultiplier);
                            customVerbs.Add(newProps);
                        }

                        Traverse.Create(weapon.def).Field("verbs").SetValue(customVerbs);
                    }
                }
            }
        }

        public static void Postfix(List<VerbProperties> __state)
        {
            if (__state != null)
            {
                Traverse.Create(weaponToLookUp.def).Field("verbs").SetValue(__state);
                weaponToLookUp = null;
            }
        }
    }

    [HarmonyPatch(typeof(ThingWithComps), "SpawnSetup")]
    public static class VanillaExpandedFramework_ThingWithComps_SpawnSetup_Patch
    {
        public static void Postfix(ThingWithComps __instance)
        {
            VerbUtility.TryModifyThingsVerbs(__instance);
        }
    }

    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "AddEquipment")]
    public static class VanillaExpandedFramework_Pawn_EquipmentTracker_AddEquipment_Patch
    {
        public static void Postfix(Pawn_EquipmentTracker __instance, ref ThingWithComps newEq)
        {
            VerbUtility.TryModifyThingsVerbs(newEq);
            var comp = newEq.TryGetComp<CompWeaponHediffs>();
            if (comp != null)
            {
                comp.AssignHediffs();
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "TryDropEquipment")]
    public static class VanillaExpandedFramework_Pawn_EquipmentTracker_TryDropEquipment_Patch
    {
        public static void Postfix(Pawn_EquipmentTracker __instance, ThingWithComps eq, ThingWithComps resultingEq, IntVec3 pos, bool forbid = true)
        {
            VerbUtility.TryModifyThingsVerbs(resultingEq);
            var comp = resultingEq.TryGetComp<CompWeaponHediffs>();
            if (comp != null)
            {
                comp.AssignHediffs();
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), "TryDrop",
        new[] {typeof(Apparel), typeof(Apparel), typeof(IntVec3), typeof(bool)},
        new[] {ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal, ArgumentType.Normal})]
    public static class VanillaExpandedFramework_Pawn_ApparelTracker_TryDrop_Patch
    {
        public static void Postfix(Pawn_ApparelTracker __instance, Apparel ap)
        {
            VerbUtility.TryModifyThingsVerbs(ap);
        }
    }

    [HarmonyPatch(typeof(HediffSet), nameof(HediffSet.DirtyCache))]
    public static class VanillaExpandedFramework_HediffSet_DirtyCache_Patch
    {
        private static void Postfix(HediffSet __instance)
        {
            VerbUtility.TryModifyThingsVerbs(__instance.pawn);
        }
    }

    [HarmonyPatch(typeof(ThingWithComps), "ExposeData")]
    public static class VanillaExpandedFramework_ThingWithComps_ExposeData_Patch
    {
        public static void Postfix(ThingWithComps __instance)
        {
            if (Scribe.mode == LoadSaveMode.PostLoadInit) VerbUtility.TryModifyThingsVerbs(__instance);
        }
    }
}
