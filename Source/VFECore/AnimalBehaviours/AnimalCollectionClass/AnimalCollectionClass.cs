
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
        public static IDictionary<Thing, bool[]> draftable_animals = new Dictionary<Thing, bool[]>();

        // A list of hovering animals for CompProperties_Floating
        public static HashSet<Thing> floating_animals = new HashSet<Thing>();

        // A list of animal graphic paths
        public static List<string> salamander_graphics = new List<string>();

        // An integer with the current number of animal control hubs built
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

        public static void AddGraphicPathToList(string graphicPath)
        {
           
            salamander_graphics.Add(graphicPath);           
            
        }

        public static void RemoveGraphicPathFromList(string graphicPath)
        {
            if (salamander_graphics.Contains(graphicPath))
            {
                salamander_graphics.Remove(graphicPath);
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
