using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using VEF.Pawns;

namespace VEF.Pawns
{

    public static class VanillaExpandedFramework_PawnGenerator_GenerateGearFor_Patch
    {

        [HarmonyPatch(typeof(PawnGenerator), "GenerateGearFor")]
        public static class GenerateGearFor
        {
            public static void Postfix(Pawn pawn)
            {
                // Also generate shield
                PawnShieldGenerator.TryGenerateShieldFor(pawn);

                pawn.story?.AllBackstories?.OfType<VEBackstoryDef>().SelectMany(selector: bd => bd.forcedItems).Do(action: tdcrc =>
                {
                    int count = tdcrc.countRange.RandomInRange;

                    while (count > 0)
                    {
                        Thing thing = ThingMaker.MakeThing(tdcrc.thingDef, GenStuff.RandomStuffFor(tdcrc.thingDef));
                        thing.stackCount = Mathf.Min(count, tdcrc.thingDef.stackLimit);
                        count -= thing.stackCount;
                        pawn.inventory?.TryAddItemNotForSale(thing);
                    }
                });
            }

        }

        
    }
}