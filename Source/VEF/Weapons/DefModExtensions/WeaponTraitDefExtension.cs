using RimWorld;
using Verse;
using System.Collections.Generic;

namespace VEF.Weapons
{
    public class WeaponTraitDefExtension : DefModExtension
    {
        //Swaps the projectiles produced by this unique weapon for another ones
        public ThingDef projectileOverride;
        //Swaps the sound produced when firing this unique weapon for another one
        public SoundDef soundOverride;
        //Swaps the sound produced when hitting a pawn with this weapon with melee
        public SoundDef meleeSoundOverride;
        //Swaps the graphic of the weapon. Supports Graphic_Single and Graphic_Random, as well as CutOutComplex
        public Dictionary<ThingDef,GraphicData> graphicOverrides;      
        //Add this ability to the wielder when equipping the weapon (and remove it when not in his possession)
        public AbilityDef abilityToAdd;
        //Add this thought to the wielder when killing something with the weapon
        public ThoughtDef killThought;
        //This scales the draw size of the weapon. What a silly thing.
        public float sizeMultiplier = 1;
        //This will make the gun create random projectiles every time it fires. Shenanigan potential: maximum
        public bool randomprojectiles = false;
    }

  
}
