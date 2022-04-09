using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFE.Mechanoids.HarmonyPatches
{
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class MachinesDie
    {
        public static void Postfix(Pawn __instance)
        {
            if (__instance.RaceProps.IsMechanoid && __instance.Faction == Faction.OfPlayer)
            {
                string ingredients = "";
                bool comma = false;
                foreach(ThingDefCountClass ingredient in __instance.def.butcherProducts)
                {
                    if (comma)
                        ingredients += ", ";
                    ingredients += ingredient.thingDef.label + " x" + ingredient.count;
                    comma = true;
                }
                Find.LetterStack.ReceiveLetter("VFEMechMachineDied".Translate(), "VFEMechMachineDiedDesc".Translate(ingredients), LetterDefOf.NegativeEvent, __instance);
            }
        }
    }
}
