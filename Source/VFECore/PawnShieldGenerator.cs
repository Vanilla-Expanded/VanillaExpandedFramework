using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFECore
{

    public static class PawnShieldGenerator
    {

        private static List<ThingStuffPair> allShieldPairs;
        private static List<ThingStuffPair> workingShields = new List<ThingStuffPair>();

        public static void Reset()
        {
            Predicate<ThingDef> isShield = (ThingDef td) => td.equipmentType == EquipmentType.Primary && td.GetCompProperties<CompProperties_Shield>() is CompProperties_Shield shieldProps && !shieldProps.shieldTags.NullOrEmpty();
            allShieldPairs = ThingStuffPair.AllWith(isShield);
            using (IEnumerator<ThingDef> enumerator = (from td in DefDatabase<ThingDef>.AllDefs where isShield(td) select td).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    ThingDef thingDef = enumerator.Current;
                    float num = (from pa in allShieldPairs where pa.thing == thingDef select pa).Sum((ThingStuffPair pa) => pa.Commonality);
                    float num2 = thingDef.generateCommonality / num;
                    if (num2 != 1f)
                    {
                        for (int i = 0; i < allShieldPairs.Count; i++)
                        {
                            ThingStuffPair thingStuffPair = allShieldPairs[i];
                            if (thingStuffPair.thing == thingDef)
                            {
                                allShieldPairs[i] = new ThingStuffPair(thingStuffPair.thing, thingStuffPair.stuff, thingStuffPair.commonalityMultiplier * num2);
                            }
                        }
                    }
                }
                enumerator.Dispose();
            }
        }

        public static void TryGenerateShieldFor(Pawn pawn)
        {
            workingShields.Clear();
            var kindDefExtension = PawnKindDefExtension.Get(pawn.kindDef);

            // No shieldTags
            if (kindDefExtension.shieldTags.NullOrEmpty())
                return;

            // Not a tool user
            if (!pawn.RaceProps.ToolUser)
                return;

            // Primary unusable with shields
            if (pawn.equipment.Primary is ThingWithComps primary && !primary.def.UsableWithShields())
                return;

            // Has multiple weapons
            if (pawn.equipment.AllEquipmentListForReading.Count(t => t.def.equipmentType == EquipmentType.Primary) > 1)
                return;

            // Not enough manipulation
            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || !pawn.CanUseShields())
                return;

            // Pacifist
            if (pawn.story != null && pawn.WorkTagIsDisabled(WorkTags.Violent))
                return;

            float randomInRange = kindDefExtension.shieldMoney.RandomInRange;
            for (int i = 0; i < allShieldPairs.Count; i++)
            {
                ThingStuffPair w = allShieldPairs[i];
                if (w.Price <= randomInRange)
                {
                    var shieldProps = w.thing.GetCompProperties<CompProperties_Shield>();
                    if (kindDefExtension.shieldTags == null || kindDefExtension.shieldTags.Any((string tag) => shieldProps.shieldTags.Contains(tag)))
                    {
                        if (w.thing.generateAllowChance >= 1f || Rand.ChanceSeeded(w.thing.generateAllowChance, pawn.thingIDNumber ^ (int)w.thing.shortHash ^ 28554824))
                        {
                            workingShields.Add(w);
                        }
                    }
                }
            }
            if (workingShields.Count == 0)
            {
                return;
            }
            //pawn.equipment.DestroyAllEquipment(DestroyMode.Vanish);
            ThingStuffPair thingStuffPair;
            if (workingShields.TryRandomElementByWeight((ThingStuffPair w) => w.Commonality * w.Price, out thingStuffPair))
            {
                var shield = (ThingWithComps)ThingMaker.MakeThing(thingStuffPair.thing, thingStuffPair.stuff);
                PawnGenerator.PostProcessGeneratedGear(shield, pawn);

                // Colour the shield
                if (pawn.Faction != null)
                {
                    var thingDefExtension = ThingDefExtension.Get(thingStuffPair.thing);
                    if (!thingDefExtension.useFactionColourForPawnKinds.NullOrEmpty() && thingDefExtension.useFactionColourForPawnKinds.Contains(pawn.kindDef))
                        shield.SetColor(pawn.Faction.Color);
                }

                pawn.equipment.AddShield(shield);
                shield.GetComp<CompShield>().equippedOffHand = true;
            }
            workingShields.Clear();
        }

    }

}
