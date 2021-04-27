
using Verse;
using System;
using RimWorld;
using System.Collections.Generic;
using System.Linq;


namespace AnimalBehaviours
{

    public static class AnimalCollectionClass
    {

        //This static class stores lists of animals for different things.

        //Currently, a list of draft-capable animals for Genetic Rim, an integer with the current number of animal control hubs built
        //and a list of hovering animals for CompProperties_Floating

        public static IDictionary<Thing, bool[]> draftable_animals = new Dictionary<Thing, bool[]>();

        public static HashSet<Thing> floating_animals = new HashSet<Thing>();

        public static int numberOfAnimalControlHubsBuilt = 0;


        public static void AddDraftableAnimalToList(Thing thing, bool[] abilityArray)
        {

            if (!draftable_animals.ContainsKey(thing))
            {
                draftable_animals.Add(thing, abilityArray);
            }
        }

        public static void RemoveDraftableAnimalFromList(Thing thing)
        {
            if (draftable_animals.ContainsKey(thing))
            {
                draftable_animals.Remove(thing);
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
