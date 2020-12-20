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
        public HeavyWeaponsMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<HeavyWeaponsSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            var things = DefDatabase<ThingDef>.AllDefsListForReading;
            foreach (var thing in things)
            {
                if (thing.HasModExtension<HeavyWeapon>())
                {
                    if (settings.weaponStates == null) 
                        settings.weaponStates = new Dictionary<string, bool>();
                    if (settings.weaponHPStates == null)
                        settings.weaponHPStates = new Dictionary<string, int>();

                    if (!settings.weaponStates.ContainsKey(thing.defName))
                    {
                        settings.weaponStates[thing.defName] = true;
                    }
                    if (!settings.weaponHPStates.ContainsKey(thing.defName))
                    {
                        var options = thing.GetModExtension<HeavyWeapon>();
                        settings.weaponHPStates[thing.defName] = options.weaponHitPointsDeductionOnShot;
                        Log.Message("options.weaponHitPointsDeductionOnShot: " + thing + " - " + options.weaponHitPointsDeductionOnShot);
                    }
                }
            }
            settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Vanilla Weapons Expanded: Heavy";
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
            DoDefsAlter();
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