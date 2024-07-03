using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using HarmonyLib;

namespace HeavyWeapons
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            new Harmony("OskarPotocki.HeavyWeapons").PatchAll();
        }
    }
    public static class Patch_FloatMenuMakerMap
    {
    
        [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
        public static class AddHumanlikeOrders_Fix
        {
            public static void Postfix(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> opts)
            {
                IntVec3 c = IntVec3.FromVector3(clickPos);
                if (pawn.equipment != null)
                {
                    List<Thing> thingList = c.GetThingList(pawn.Map);
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        var options = thingList[i].def.GetModExtension<HeavyWeapon>();
                        if (options != null && options.isHeavy)
                        {
                            var equipment = (ThingWithComps)thingList[i];
                            TaggedString toCheck = "Equip".Translate(equipment.LabelShort);
                            FloatMenuOption floatMenuOption = opts.FirstOrDefault((FloatMenuOption x) => x.Label.Contains(toCheck));
                            if (floatMenuOption != null && !CanEquip(pawn, options))
                            {
                                opts.Remove(floatMenuOption);
                                opts.Add(new FloatMenuOption("CannotEquip".Translate(equipment.LabelShort) + " (" + options.disableOptionLabelKey.Translate(pawn.LabelShort) + ")", null));
                            }
                            break;
                        }
                    }
                }
            }
    
            public static bool CanEquip(Pawn pawn, HeavyWeapon options)
            {
                if (pawn.story?.traits != null && options.supportedTraits != null)
                {
                    if (pawn.story.traits.allTraits.Any(x => options.supportedTraits.Contains(x.def.defName)))
                    {
                        return true;
                    }
                }
                if (pawn.genes != null && options.supportedGenes != null)
                {
                    foreach (string gene in options.supportedGenes)
                    {
                        if(pawn.genes.HasActiveGene(DefDatabase <GeneDef>.GetNamedSilentFail(gene)))
                        {
                            return true;
                        }
                    }
                    
                }
                if (pawn.apparel.WornApparel != null)
                {
                    foreach (var ap in pawn.apparel.WornApparel)
                    {
                        if (options.supportedArmors?.Contains(ap.def.defName) ?? false)
                        {
                            return true;
                        }
                        if (ap.def.apparel?.bodyPartGroups?.Contains(BodyPartGroupDefOf.Torso) ?? false)
                        {
                            if (ap.def.tradeTags != null)
                            {
                                if (ap.def.tradeTags.Contains("HiTechArmor") || ap.def.tradeTags.Contains("Warcasket"))
                                {
                                    return true;
                                }
                            }

                        }
    
                    }
                }
                return false;
            }
        }
    }
}
