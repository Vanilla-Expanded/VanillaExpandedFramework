using System;
using System.Collections.Generic;
using Verse;
using RimWorld;



namespace VanillaMemesExpanded
{

    public class ExtendedMemeProperties : DefModExtension
    {
        //Used to make a meme only be choosable for a certain structure
        public string neededMeme;

        //Used to make a meme only be choosable if another different meme is chosen too
        public List<string> requiredMemes;

        //Used to make all members of the Ideoligion acquire a given trait
        public TraitDef forcedTrait;

        //Used to make the colony start with an opinion offset with all factions
        public int factionOpinionOffset;

        //Used to make all members of the Ideoligion acquire a given ability
        public List<AbilityDef> abilitiesGiven;

        //Used to remove designators if meme is part of the primary ideo
        public List<ThingDef> removedDesignators;
    }

}
