using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace HeavyWeapons
{
    class HeavyWeaponsMod : Mod
    {
        public static HeavyWeaponsSettings settings;

        public static bool heavyWeaponsExist;
        public HeavyWeaponsMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<HeavyWeaponsSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            if (heavyWeaponsExist)
            {
                return "Vanilla Weapons Expanded: Heavy";
            }
            return "";
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            DefsAlterer.DoDefsAlter();
        }
    }
    [StaticConstructorOnStartup]
    public static class DefsAlterer
    {
        static DefsAlterer()
        {
            Setup();
            DoDefsAlter();
        }

        public static void Setup()
        {
            var things = DefDatabase<ThingDef>.AllDefsListForReading;
            foreach (var thing in things)
            {
                if (thing.HasModExtension<HeavyWeapon>())
                {
                    HeavyWeaponsMod.heavyWeaponsExist = true;
                    if (HeavyWeaponsMod.settings.weaponStates == null)
                        HeavyWeaponsMod.settings.weaponStates = new Dictionary<string, bool>();
                    if (HeavyWeaponsMod.settings.weaponHPStates == null)
                        HeavyWeaponsMod.settings.weaponHPStates = new Dictionary<string, int>();

                    if (!HeavyWeaponsMod.settings.weaponStates.ContainsKey(thing.defName))
                    {
                        HeavyWeaponsMod.settings.weaponStates[thing.defName] = true;
                    }
                    if (!HeavyWeaponsMod.settings.weaponHPStates.ContainsKey(thing.defName))
                    {
                        var options = thing.GetModExtension<HeavyWeapon>();
                        HeavyWeaponsMod.settings.weaponHPStates[thing.defName] = options.weaponHitPointsDeductionOnShot;
                    }
                }
            }
        }
        public static void DoDefsAlter()
        {
            foreach (var weaponState in HeavyWeaponsMod.settings.weaponStates)
            {
                if (!weaponState.Value)
                {
                    var defToAlter = DefDatabase<ThingDef>.GetNamedSilentFail(weaponState.Key);
                    if (defToAlter != null)
                    {
                        var options = defToAlter.GetModExtension<HeavyWeapon>();
                        options.weaponHitPointsDeductionOnShot = 0;
                    }
                }
            }
            foreach (var weaponState in HeavyWeaponsMod.settings.weaponHPStates)
            {
                var defToAlter = DefDatabase<ThingDef>.GetNamedSilentFail(weaponState.Key);
                if (defToAlter != null)
                {
                    var options = defToAlter.GetModExtension<HeavyWeapon>();
                    if (options.weaponHitPointsDeductionOnShot != weaponState.Value)
                    {
                        options.weaponHitPointsDeductionOnShot = weaponState.Value;
                    }
                }
            }
        }
    }
}