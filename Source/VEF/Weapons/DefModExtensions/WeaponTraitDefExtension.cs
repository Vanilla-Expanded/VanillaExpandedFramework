using RimWorld;
using Verse;

namespace VEF.Weapons
{
    public class WeaponTraitDefExtension : DefModExtension
    {
        //Swaps the projectiles produced by this unique weapon for another ones
        public ThingDef projectileOverride;
        //Swaps the sound produced when firing this unique weapon for another one
        public SoundDef soundOverride;
        //Swaps the graphic of the weapon. Supports Graphic_Single and Graphic_Random, as well as CutOutComplex
        public GraphicData graphicOverride;      
        //Add this ability to the wielder when equipping the weapon (and remove it when not in his possession)
        public AbilityDef abilityToAdd;
        //Add this hediff to the wielder when equipping the weapon (and remove it when not in his possession)
        public HediffDef hediffToAdd;
        //Add this thought to the wielder when killing something with the weapon
        public ThoughtDef killThought;
        //This scales the draw size of the weapon. What a silly thing.
        public float sizeMultiplier = 1;
    }

  
}
