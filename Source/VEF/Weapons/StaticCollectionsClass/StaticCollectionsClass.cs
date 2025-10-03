﻿
using Verse;
using System;
using RimWorld;
using System.Collections.Generic;
using System.Linq;


namespace VEF.Weapons
{
    [StaticConstructorOnStartup]
    public static class StaticCollectionsClass
    {
        public static List<ThingDef> projectilesInGame = new List<ThingDef>();
        public static List<ThingDef> uniqueWeaponsInGame = new List<ThingDef>();

        static StaticCollectionsClass()
        {
            if (ModsConfig.OdysseyActive)
            {
                List<ThingDef> uniqueWeapons = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.GetCompProperties<CompProperties_UniqueWeapon>() != null).ToList();
                uniqueWeaponsInGame = uniqueWeapons;

                if (uniqueWeapons.Count > 0)
                {
                    foreach (ThingDef thingDef in uniqueWeapons)
                    {
                        thingDef.comps.Add(new CompProperties_ApplyWeaponTraits());
                    }

                }

                projectilesInGame = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.projectile!= null && x.GetModExtension<ProjectileExtension>()?.excludeFromStaticCollection!=true).ToList();

            }
        }




    }
}
