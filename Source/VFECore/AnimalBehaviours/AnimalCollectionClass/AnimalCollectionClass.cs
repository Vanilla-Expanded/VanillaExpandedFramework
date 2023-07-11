
using Verse;
using System;
using RimWorld;
using System.Collections.Generic;
using System.Linq;


namespace AnimalBehaviours
{
    [StaticConstructorOnStartup]
    public static class AnimalCollectionClass
    {

        //This static class stores lists of animals and pawns for different things.

    
        // A list of draft-capable animals for Genetic Rim
        public static HashSet<Thing> draftable_animals = new HashSet<Thing>();

        // A list of hovering animals for CompProperties_Floating
        public static HashSet<Thing> floating_animals = new HashSet<Thing>();

        // A list of waterstriding pawns (no terrain costs, but only in water)
        public static HashSet<Thing> waterstriding_pawns = new HashSet<Thing>();

        // A list of animals that don't flee for combat for CompProperties_DoesntFlee
        public static HashSet<Thing> nofleeing_animals = new HashSet<Thing>();

        // A list of animals that can use abilities
        public static HashSet<Thing> abilityUsing_animals = new HashSet<Thing>();

        // A list of animals that don't produce filth for CompProperties_NoFilth
        public static HashSet<Thing> nofilth_animals = new HashSet<Thing>();

        // A list of animals unaffected by the animal disease incident
        public static HashSet<Thing> nodisease_animals = new HashSet<Thing>();

        // A list of animals that eat weird things to cache them for CompProperties_EatWeirdFood and its Harmony patch
        public static HashSet<Thing> weirdeEaters_animals = new HashSet<Thing>();

        // A list of animals that don't have taming decay
        public static HashSet<ThingDef> notamingdecay_animals = new HashSet<ThingDef>();

        // A list of animals for ComLastStand    
        public static IDictionary<Thing, float> lastStand_animals = new Dictionary<Thing, float>();

        // A list of Salamander graphic paths    
        public static IDictionary<Thing, string> salamander_graphics = new Dictionary<Thing, string>();

        // A list of animals disabled from quests. Loaded once from XML lists in this class' constructor
        public static HashSet<PawnKindDef> questDisabledAnimals = new HashSet<PawnKindDef>();



        // An integer with the current number of animal control hubs built
        public static int numberOfAnimalControlHubsBuilt = 0;

        public static bool IsDraftableAnimal(this Pawn pawn)
        {
            return draftable_animals.Contains(pawn);
        }

        public static bool IsAbilityUserAnimal(this Pawn pawn)
        {
            return abilityUsing_animals.Contains(pawn)&&pawn.Faction?.IsPlayer==true && pawn.MentalState is null;
        }

        public static bool IsLastStandAnimal(this Pawn pawn)
        {
            return lastStand_animals.ContainsKey(pawn);
        }

        public static float LastStandAnimalRate(this Pawn pawn)
        {
            return lastStand_animals[pawn];
        }

        public static bool IsDraftableControllableAnimal(this Pawn pawn)
        {
            return pawn.IsDraftableAnimal() && pawn.Faction != null && pawn.Faction.IsPlayer && pawn.MentalState is null;
        }
        public static void AddDraftableAnimalToList(Thing thing)
        {

            if (!draftable_animals.Contains(thing))
            {
                draftable_animals.Add(thing);
            }
        }

        public static void RemoveDraftableAnimalFromList(Thing thing)
        {
            if (draftable_animals.Contains(thing))
            {
                draftable_animals.Remove(thing);
            }

        }

        public static void AddLastStandAnimalToList(Thing thing, float rate)
        {

            if (!lastStand_animals.ContainsKey(thing))
            {
                lastStand_animals.Add(thing,rate);
                
            }
        }

        public static void RemoveLastStandAnimalFromList(Thing thing)
        {
            if (lastStand_animals.ContainsKey(thing))
            {
                lastStand_animals.Remove(thing);
            }

        }

        public static void AddAbilityUsingAnimalToList(Thing thing)
        {

            if (!abilityUsing_animals.Contains(thing))
            {
                abilityUsing_animals.Add(thing);
            }
        }

        public static void RemoveAbilityUsingFromList(Thing thing)
        {
            if (abilityUsing_animals.Contains(thing))
            {
                abilityUsing_animals.Remove(thing);
            }

        }

        public static void AddNoDiseasesAnimalToList(Thing thing)
        {

            if (!nodisease_animals.Contains(thing))
            {
                nodisease_animals.Add(thing);
            }
        }

        public static void RemoveNoDiseasesAnimalFromList(Thing thing)
        {
            if (nodisease_animals.Contains(thing))
            {
                nodisease_animals.Remove(thing);
            }

        }

        public static void AddGraphicPathToList(Thing thing,string graphicPath)
        {
            if (!salamander_graphics.ContainsKey(thing))
            {
                salamander_graphics.Add(thing, graphicPath);
            }
           
        }

        public static void RemoveGraphicPathFromList(Thing thing, string graphicPath)
        {
            if (salamander_graphics.ContainsKey(thing))
            {
                salamander_graphics.Remove(thing);
            }


        }

        public static void AddFloatingAnimalToList(Thing thing)
        {

            if (!floating_animals.Contains(thing))
            {
                floating_animals.Add(thing);
            }
        }

        public static void RemoveFloatingAnimalFromList(Thing thing)
        {
            if (floating_animals.Contains(thing))
            {
                floating_animals.Remove(thing);
            }

        }

        public static void AddWaterstridingPawnToList(Thing thing)
        {

            if (!waterstriding_pawns.Contains(thing))
            {
                waterstriding_pawns.Add(thing);
            }
        }

        public static void RemoveWaterstridingPawnFromList(Thing thing)
        {
            if (waterstriding_pawns.Contains(thing))
            {
                waterstriding_pawns.Remove(thing);
            }

        }

        public static void AddNoTamingDecayAnimalToList(ThingDef thing)
        {

            if (!notamingdecay_animals.Contains(thing))
            {
                notamingdecay_animals.Add(thing);
            }
        }

        public static void RemoveNoTamingDecayAnimalFromList(ThingDef thing)
        {
            if (notamingdecay_animals.Contains(thing))
            {
                notamingdecay_animals.Remove(thing);
            }

        }

        public static bool IsNoTamingDecayAnimal(this ThingDef pawn)
        {
            return notamingdecay_animals.Contains(pawn);
        }

        public static void AddNoFilthAnimalToList(Thing thing)
        {

            if (!nofilth_animals.Contains(thing))
            {
                nofilth_animals.Add(thing);
            }
        }

        public static void RemoveNoFilthAnimalFromList(Thing thing)
        {
            if (nofilth_animals.Contains(thing))
            {
                nofilth_animals.Remove(thing);
            }

        }

        public static void AddNotFleeingAnimalToList(Thing thing)
        {

            if (!nofleeing_animals.Contains(thing))
            {
                nofleeing_animals.Add(thing);
            }
        }

        public static void RemoveNotFleeingAnimalFromList(Thing thing)
        {
            if (nofleeing_animals.Contains(thing))
            {
                nofleeing_animals.Remove(thing);
            }

        }

        public static void AddWeirdEaterAnimalToList(Thing thing)
        {

            if (!weirdeEaters_animals.Contains(thing))
            {
                weirdeEaters_animals.Add(thing);
            }
        }

        public static void RemoveWeirdEaterAnimalFromList(Thing thing)
        {
            if (weirdeEaters_animals.Contains(thing))
            {
                weirdeEaters_animals.Remove(thing);
            }

        }



        public static void AddControlHubBuilt()
        {
            numberOfAnimalControlHubsBuilt++;
        }

        public static void RemoveControlHubBuilt()
        {
            if (numberOfAnimalControlHubsBuilt > 0)
            {
                numberOfAnimalControlHubsBuilt--;
            }

        }


        static AnimalCollectionClass()
        {
            
            HashSet<AnimalsDisabledFromQuestsDef> allUnaffectedLists = DefDatabase<AnimalsDisabledFromQuestsDef>.AllDefsListForReading.ToHashSet();
            foreach (AnimalsDisabledFromQuestsDef individualList in allUnaffectedLists)
            {
                questDisabledAnimals.AddRange(individualList.disabledFromQuestsPawns);
            }
        }




    }
}
