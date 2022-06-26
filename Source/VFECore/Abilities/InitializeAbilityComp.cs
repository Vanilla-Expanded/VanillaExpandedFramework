namespace VFECore.Abilities
{
    using UnityEngine;
    using Verse;

    [StaticConstructorOnStartup]
    public static class InitializeAbilityComp
    {
        static InitializeAbilityComp()
        {
            for (int i = 0; i < DefDatabase<ThingDef>.AllDefsListForReading.Count; i++)
            {
                ThingDef def = DefDatabase<ThingDef>.AllDefsListForReading[i];

                if (def.race?.Humanlike ?? false)
                {
                    if (!def.comps.Any(cp => typeof(CompProperties_ShieldBubble).IsAssignableFrom(cp.compClass)))
                    {
                        CompProperties_ShieldBubble props = new CompProperties_ShieldBubble()
                                                            {
                                                                compClass             = typeof(CompAbilities),
                                                                blockRangedAttack     = true,
                                                                blockMeleeAttack      = false,
                                                                showWhenDrafted       = true,
                                                                showOnHostiles        = true,
                                                                showOnNeutralInCombat = true,
                                                                shieldTexPath         = "Other/ShieldBubble",
                                                                minShieldSize         = 1f,
                                                                maxShieldSize         = 1.5f,
                                                                shieldColor           = new Color(1, 1, 1, 1),
                                                                EnergyLossPerDamage   = 1f
                    };
                        def.comps.Add(props);
                        props.ResolveReferences(def);
                        props.PostLoadSpecial(def);
                    }
                }
            }
        }
    }
}
