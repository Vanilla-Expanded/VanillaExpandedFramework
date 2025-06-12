using System.Collections.Generic;
using System.Linq;
using VEF.AnimalBehaviours;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Weapons
{
    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    static class VanillaExpandedFramework_FloatMenuMakerMap_AddHumanlikeOrders_Patch
    {
        //TODO - Assigned to Taranchuk

        /* static HashSet<ThingDef> allToolsList = new HashSet<ThingDef>();

         static FloatMenuMakerMap_AddHumanlikeOrders_Patch()
         {

             HashSet<ToolsUsableByNonViolentPawnsDef> allLists = DefDatabase<ToolsUsableByNonViolentPawnsDef>.AllDefsListForReading.ToHashSet();
             foreach (ToolsUsableByNonViolentPawnsDef individualList in allLists)
             {
                 allToolsList.AddRange(individualList.toolsUsableByNonViolentPawns);
             }
         }

         private static bool inMenuMaker;

         public static void Prefix(Pawn pawn)
         {
             inMenuMaker = pawn.WorkTagIsDisabled(WorkTags.Violent);
         }

         public static void Postfix()
         {
             inMenuMaker = false;
         }

         [HarmonyPatch(typeof(ThingDef), nameof(ThingDef.IsWeapon), MethodType.Getter)]
         [HarmonyPostfix]
         public static bool IsWeapon(bool __result, ThingDef __instance)
         {
             if (inMenuMaker && IsToolDef(__instance))
             {
                 return false;
             }

             return __result;
         }

         [HarmonyPatch(typeof(ThingDef), nameof(ThingDef.IsRangedWeapon), MethodType.Getter)]
         [HarmonyPostfix]
         public static bool IsRangedWeapon(bool __result, ThingDef __instance)
         {
             if (inMenuMaker && IsToolDef(__instance))
             {
                 return false;
             }

             return __result;
         }



         public static bool IsToolDef(ThingDef thingDef)
         {
             return allToolsList?.Contains(thingDef)==true;
         }
        
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
         
         
         */
    }
}
