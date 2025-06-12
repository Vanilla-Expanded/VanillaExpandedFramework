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
        //If specified, this WeaponTraitDef will only appear on this weapon ThingDef
        public ThingDef onlyForThisWeapon;
        //Add this ability to the wielder when equipping the weapon (and remove it when not in his possession)
        public AbilityDef abilityToAdd;
    }
}
