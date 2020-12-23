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
    class HeavyWeaponsSettings : ModSettings
    {
        public Dictionary<string, bool> weaponStates = new Dictionary<string, bool>();
        public Dictionary<string, int> weaponHPStates = new Dictionary<string, int>();
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref weaponStates, "weaponStates", LookMode.Value, LookMode.Value, ref weaponsKeys1, ref boolValues);
            Scribe_Collections.Look(ref weaponHPStates, "weaponHPStates", LookMode.Value, LookMode.Value, ref weaponsKeys2, ref floatValues);
        }

        private List<string> weaponsKeys1;
        private List<bool> boolValues;

        private List<string> weaponsKeys2;
        private List<int> floatValues;
        public void DoSettingsWindowContents(Rect inRect)
        {
            var keys = weaponStates.Keys.ToList().OrderByDescending(x => x).ToList();
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Rect rect2 = new Rect(0f, 0f, inRect.width - 30f, keys.Count * 74);
            Widgets.BeginScrollView(rect, ref scrollPosition, rect2, true);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect2);
            for (int num = keys.Count - 1; num >= 0; num--)
            {
                var test = weaponStates[keys[num]];
                var def = DefDatabase<ThingDef>.GetNamed(keys[num], false);
                if (def != null)
                {
                    listingStandard.CheckboxLabeled("Enable HP deduction per shot for " + def.label + ":", ref test);
                    weaponStates[keys[num]] = test;
                    if (!test)
                    {
                        weaponHPStates[keys[num]] = 0;
                    }
                    listingStandard.Label("Adjust HP deduction per shot for " + def.label + ": " + weaponHPStates[keys[num]]);
                    var value = listingStandard.Slider(weaponHPStates[keys[num]], 0, 100);
                    weaponHPStates[keys[num]] = (int)value;
                }

            }
            listingStandard.End();
            Widgets.EndScrollView();
            base.Write();
        }
        private static Vector2 scrollPosition = Vector2.zero;

    }
}

