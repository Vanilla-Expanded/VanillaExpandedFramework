using RimWorld;
using Verse;
using System.Collections.Generic;

namespace VEF.Weapons
{
    public class WeaponTraitDefExtension : DefModExtension
    {
        //Swaps the projectiles produced by this unique weapon for another ones
        public ThingDef projectileOverride;
        //This allows users to specify different projectiles for different guns. If not found, projectileOverride will be used
        public Dictionary<ThingDef, ThingDef> projectileOverrides;
        //If set to true, this projectile override will be ignored if another WeaponTraitDef adds a different override
        public bool lowPreferenceProjectileOverride = false;
        //Swaps the sound produced when firing this unique weapon for another one
        public SoundDef soundOverride;
        //Swaps the sound produced when hitting a pawn with this weapon with melee
        public SoundDef meleeSoundOverride;
        //Override the damage type of the melee attacks with this weapon
        public DamageDef meleeDamageOverride;
        //Swaps the graphic of the weapon. Supports Graphic_Single and Graphic_Random, as well as CutOutComplex
        public Dictionary<ThingDef,GraphicData> graphicOverrides;
        //Graphic overrides will be chosen by the order of this number. Higher takes precedence.
        public float graphicOverridePriority = 100;
        //Add this ability to the wielder when equipping the weapon (and remove it when not in his possession)
        public AbilityDef abilityToAdd;
        //Add this hediff to the wielder after killing a target
        public HediffDef killHediff;
        public float killHediffSeverity = 1;
        //This scales the draw size of the weapon. What a silly thing.
        public float sizeMultiplier = 1;
        //This will make the gun create random projectiles every time it fires. Shenanigan potential: maximum
        public bool randomprojectiles = false;
        //If true, the MaxHitPoints stat of the weapon will be refreshed when the trait is added. This only works if the WeaponTraitDef
        //is assigned VEF.Weapons.WeaponTraitWorker_Extended as its workerClass
        public bool refreshMaxHitPointsStat = false;
        //Conditional stat affecters, like those in genes and precepts, are possible now
        public List<ConditionalStatAffecter> conditionalStatAffecters;
        //This is a system to add abilities with charges via traits, hopefully working better than the vanilla system
        public AbilityWithChargesDetails abilityWithCharges;
        ////Swaps the verbs used by this unique weapon for another ones
        public List<VerbProperties> verbsOverride;
        //This allows users to specify different verbs for different guns. If not found, verbsOverride will be used
        public Dictionary<ThingDef, List<VerbProperties>> verbsOverrides;
        

    }

    public class AbilityWithChargesDetails
    {
        public AbilityDef abilityDef;

        public int maxCharges;

        public ThingDef ammoDef;

        public int ammoCountPerCharge;

        public int baseReloadTicks = 60;

        public SoundDef soundReload;

        public string chargeNoun = "charge";

        public string cooldownGerund = "on cooldown";

        public NamedArgument ChargeNounArgument => chargeNoun.Named("CHARGENOUN");

    }

}
