
using Verse;
using System;
using RimWorld;
using System.Collections.Generic;
using System.Linq;


namespace AnimalBehaviours
{

    public static class AnimalCollectionClass
    {

        //This static class stores lists of animals and pawns for different things.

    
        // A list of draft-capable animals for Genetic Rim
        public static HashSet<Thing> draftable_animals = new HashSet<Thing>();

        // A list of hovering animals for CompProperties_Floating
        public static HashSet<Thing> floating_animals = new HashSet<Thing>();

        // A list of animals that don't flee for combat for CompProperties_DoesntFlee
        public static HashSet<Thing> nofleeing_animals = new HashSet<Thing>();

        // A list of animals that don't produce filth for CompProperties_NoFilth
        public static HashSet<Thing> nofilth_animals = new HashSet<Thing>();

        // A list of animals that eat weird things to cache them for CompProperties_EatWeirdFood and its Harmony patch
        public static HashSet<Thing> weirdeEaters_animals = new HashSet<Thing>();

        // A list of Salamander graphic paths    
        public static IDictionary<Thing, string> salamander_graphics = new Dictionary<Thing, string>();


        // An integer with the current number of animal control hubs built
        public static int numberOfAnimalControlHubsBuilt = 0;

        public static bool IsDraftableAnimal(this Pawn pawn)
        {
            return draftable_animals.Contains(pawn);
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




    }
}
