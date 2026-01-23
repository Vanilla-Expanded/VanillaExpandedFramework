using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using static HarmonyLib.Code;
using Verse.Sound;
using System;
using System.Text;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;



namespace VEF.Weapons
{
    public class CompApplyWeaponTraits : ThingComp
    {
        public AbilityWithChargesDetails cachedAbilityWithChargesDetails;
        public AbilityDef abilityWithCharges;
        public int maxCharges;
        public int currentCharges; 
     
        public string LabelRemaining => $"{currentCharges} / {maxCharges}";

        List<WeaponTraitDefExtension> contentDetails = null;

        CompUniqueWeapon cachedComp;
        CompEquippable cachedEquippableComp;

        public List<WeaponTraitDefExtension> GetDetails()
        {
            if (contentDetails == null)
            {
                contentDetails = new List<WeaponTraitDefExtension>();
                CompUniqueWeapon comp = GetComp();
                if (comp != null && comp.TraitsListForReading?.Count > 0)
                {
                    foreach (WeaponTraitDef item in comp.TraitsListForReading)
                    {
                        WeaponTraitDefExtension extension = item.GetModExtension<WeaponTraitDefExtension>();
                        if (extension != null)
                        {
                            contentDetails.Add(extension);
                        }
                    }
                }
            }
            return contentDetails;
        }

        public CompUniqueWeapon GetComp()
        {
            if (cachedComp == null)
            {
                cachedComp = this.parent.GetComp<CompUniqueWeapon>();
            }
            return cachedComp;
        }

        public CompEquippable GetEquippableComp()
        {
            return cachedEquippableComp ??= this.parent.GetComp<CompEquippable>();
        }

        public AbilityWithChargesDetails AbilityDetailsForWeapon(List<WeaponTraitDefExtension> traits)
        {
            if(cachedAbilityWithChargesDetails is null)
            {
                cachedAbilityWithChargesDetails = traits.Where(x => x.abilityWithCharges != null)?.Select(x => x.abilityWithCharges).FirstOrFallback();
            }return cachedAbilityWithChargesDetails;


        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!GetDetails().NullOrEmpty())
            {
                LongEventHandler.ExecuteWhenFinished(delegate { ChangeGraphic(); });
                CalculateAbilities();
            }
        }

        public void CalculateAbilities()
        {
            if (!GetDetails().NullOrEmpty())
            {
                if (abilityWithCharges is null)
                {
                   
                    abilityWithCharges = AbilityDetailsForWeapon(GetDetails())?.abilityDef;
                    if (abilityWithCharges != null)
                    {

                        maxCharges = AbilityDetailsForWeapon(GetDetails()).maxCharges;
                        currentCharges = maxCharges;
                    }
                }

            }

        }

        public override string CompInspectStringExtra()
        {
            if (AbilityDetailsForWeapon(GetDetails()) is null)
            {
                return null;
            }
            return "ChargesRemaining".Translate(AbilityDetailsForWeapon(GetDetails()).chargeNoun) + ": " + LabelRemaining;
        }


        public override void Notify_DefsHotReloaded()
        {
            base.Notify_DefsHotReloaded();
            if (!GetDetails().NullOrEmpty())
            {
                LongEventHandler.ExecuteWhenFinished(delegate { ChangeGraphic(); });
                CalculateAbilities();
            }
        }

        public void Notify_ForceRefresh()
        {
            if (!GetDetails().NullOrEmpty())
            {
                LongEventHandler.ExecuteWhenFinished(delegate { ChangeGraphic(); });
                CalculateAbilities();
            }
        }

        public void DeleteCaches()
        {
            contentDetails = null;
            cachedComp = null;
            cachedEquippableComp = null;
            cachedAbilityWithChargesDetails = null;
            abilityWithCharges = null;
            ReinitializeVerbsIfNeeded();
        }

        public void ReinitializeVerbsIfNeeded()
        {
            if (AreVerbsDirty())
            {
                // Technically not load... But it sets the verbs to null, which
                // will cause them to be reinitialized the next time they are accessed.
                GetEquippableComp().VerbTracker.VerbsNeedReinitOnLoad();
            }
        }

        public bool AreVerbsDirty()
        {
            var comp = GetEquippableComp();
            // Null equippable/verb tracker, can't do anything
            if (comp?.VerbTracker == null)
                return false;

            foreach (var verbProps in comp.VerbProperties)
            {
                if (!comp.VerbTracker.AllVerbs.Any(v => v.verbProps == verbProps))
                    return true;
            }

            return false;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this.maxCharges, "maxCharges", 0);
            Scribe_Values.Look(ref this.currentCharges, "currentCharges", 0);
            Scribe_Defs.Look(ref abilityWithCharges, "abilityWithCharges");

            if (!GetDetails().NullOrEmpty())
            {
                LongEventHandler.ExecuteWhenFinished(delegate { ChangeGraphic(); });
                CalculateAbilities();
            }

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                ReinitializeVerbsIfNeeded();
            }
        }

        public void ChangeGraphic()
        {
            if (!this.parent.def.IsApparel)
            {
                WeaponTraitDefExtension extension = GetDetails().Where(x => x.graphicOverrides != null && x.graphicOverrides.ContainsKey(this.parent.def))?.OrderByDescending(x => x.graphicOverridePriority).FirstOrFallback();
                GraphicData data = extension?.graphicOverrides[this.parent.def] ?? this.parent.def.graphicData;
                float size = GetDetails().Where(x => x.sizeMultiplier != 1)?.Select(x => x.sizeMultiplier)?.Aggregate(1, (float acc, float current) => acc * current) ?? 1;
                Shader shader = data.shaderType?.Shader ?? ShaderTypeDefOf.Cutout.Shader;
                Color color = GetComp().ForceColor() ?? Color.white;

                if (data.graphicClass == typeof(Graphic_Single))
                {
                    Graphic_Single newGraphicSingle = (Graphic_Single)GraphicDatabase.Get<Graphic_Single>(data.texPath, shader, new Vector2(size, size), color);
                    ReflectionCache.weaponGraphic(parent) = newGraphicSingle;
                }
                else if (data.graphicClass == typeof(Graphic_Random))
                {
                    Graphic_Random newGraphicRandom = (Graphic_Random)GraphicDatabase.Get<Graphic_Random>(data.texPath, shader, new Vector2(size, size), color);
                    ReflectionCache.weaponGraphic(parent) = newGraphicRandom;
                    ReflectionCache.weaponGraphic(parent) = new Graphic_RandomRotated(ReflectionCache.weaponGraphic(parent), 35);
                }
            }
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            if (!GetDetails().NullOrEmpty())
            {
                LongEventHandler.ExecuteWhenFinished(delegate { ChangeGraphic(); });
            
            }

            foreach (WeaponTraitDefExtension extension in GetDetails())
            {
                if (extension?.abilityToAdd != null)
                {
                    pawn.abilities?.GainAbility(extension.abilityToAdd);
                    if (extension.abilityToAdd.cooldownTicksRange != null)
                    {
                        Ability ability = pawn.abilities.GetAbility(extension.abilityToAdd);
                        ability.StartCooldown(ability.def.cooldownTicksRange.RandomInRange);
                    }                  
                    pawn.abilities?.Notify_TemporaryAbilitiesChanged();
                }
            }
            if (pawn.abilities != null && abilityWithCharges != null)
            {
                pawn.abilities.GainAbility(abilityWithCharges);
                pawn.abilities.Notify_TemporaryAbilitiesChanged();
                Ability ability = pawn.abilities.GetAbility(abilityWithCharges);
                ability.maxCharges = maxCharges;
                ability.RemainingCharges = currentCharges;
            }
            base.Notify_Equipped(pawn);

        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            foreach (WeaponTraitDefExtension extension in GetDetails())
            {
                if (extension?.abilityToAdd != null)
                {                 
                    pawn.abilities?.RemoveAbility(extension.abilityToAdd);                   
                    pawn.abilities?.Notify_TemporaryAbilitiesChanged();
                }
            }
            if (pawn.abilities != null && abilityWithCharges != null)
            {
                Ability ability = pawn.abilities.GetAbility(abilityWithCharges);
                maxCharges = ability.maxCharges;
                currentCharges = ability.RemainingCharges;
                pawn.abilities?.RemoveAbility(abilityWithCharges);
                pawn.abilities?.Notify_TemporaryAbilitiesChanged();
            }
            base.Notify_Unequipped(pawn);

        }

        public override void Notify_KilledPawn(Pawn pawn)
        {
            base.Notify_KilledPawn(pawn);
            foreach (WeaponTraitDefExtension extension in GetDetails())
            {
                if (extension?.killHediff != null)
                {
                    float num = extension.killHediffSeverity;
                    if (extension.killHediff== HediffDefOf.ToxicBuildup)
                    {                       
                        num *= Mathf.Max(1f - pawn.GetStatValue(StatDefOf.ToxicResistance), 0f);
                        num *= Mathf.Max(1f - pawn.GetStatValue(StatDefOf.ToxicEnvironmentResistance), 0f);                                        
                    }
                    if (num != 0f)
                    {
                        HealthUtility.AdjustSeverity(pawn, extension.killHediff, num);
                    }
                }
            }
        }

        public override float GetStatFactor(StatDef stat)
        {
            float num = 1f;
            if (GetDetails()?.Count > 0)
            {
                foreach (WeaponTraitDefExtension extension in GetDetails())
                {
                    if (!extension.conditionalStatAffecters.NullOrEmpty())
                    {
                        for (int i = 0; i < extension.conditionalStatAffecters.Count; i++)
                        {
                            ConditionalStatAffecter conditionalStatAffecter = extension.conditionalStatAffecters[i];
                            if (conditionalStatAffecter.statFactors != null && conditionalStatAffecter.Applies(StatRequest.For(this.parent)))
                            {
                                num *= conditionalStatAffecter.statFactors.GetStatFactorFromList(stat);
                            }
                        }
                    }

                }

            }
               
            return num;
        }

        public override float GetStatOffset(StatDef stat)
        {
            float num = 0f;
            if (GetDetails()?.Count > 0)
            {
                foreach (WeaponTraitDefExtension extension in GetDetails())
                {
                    if (!extension.conditionalStatAffecters.NullOrEmpty())
                    {
                        for (int i = 0; i < extension.conditionalStatAffecters.Count; i++)
                        {
                            ConditionalStatAffecter conditionalStatAffecter = extension.conditionalStatAffecters[i];
                            if (conditionalStatAffecter.statOffsets != null && conditionalStatAffecter.Applies(StatRequest.For(this.parent)))
                            {
                                num += conditionalStatAffecter.statOffsets.GetStatOffsetFromList(stat);
                            }
                        }
                    }

                }

            }

            return num;
        }

        public override void GetStatsExplanation(StatDef stat, StringBuilder sb, string whitespace = "")
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (GetComp()?.TraitsListForReading!=null)
            {
                foreach (WeaponTraitDef trait in GetComp().TraitsListForReading)
                {
                    WeaponTraitDefExtension extension = trait.GetModExtension<WeaponTraitDefExtension>();
                    if (extension!=null&&!extension.conditionalStatAffecters.NullOrEmpty())
                    {
                        for (int i = 0; i < extension.conditionalStatAffecters.Count; i++)
                        {
                            ConditionalStatAffecter conditionalStatAffecter = extension.conditionalStatAffecters[i];
                            if(conditionalStatAffecter.statOffsets != null)
                            {
                                float statOffsetFromList = conditionalStatAffecter.statOffsets.GetStatOffsetFromList(stat);
                                if (statOffsetFromList!=0&&conditionalStatAffecter.Applies(StatRequest.For(this.parent)))
                                {
                                    sb.AppendLine(whitespace + "    " + trait.LabelCap + " (" + conditionalStatAffecter.Label + "): " + stat.ValueToString(statOffsetFromList, ToStringNumberSense.Offset, finalized: false));

                                }
                            }
                            if (conditionalStatAffecter.statFactors != null)
                            {
                                float statFactorFromList = conditionalStatAffecter.statFactors.GetStatFactorFromList(stat);
                                if (statFactorFromList!=1&& conditionalStatAffecter.Applies(StatRequest.For(this.parent)))
                                {
                                    sb.AppendLine(whitespace + "    " + trait.LabelCap + " (" + conditionalStatAffecter.Label + "): " + stat.ValueToString(statFactorFromList, ToStringNumberSense.Factor, finalized: false));
                                }
                            }                              
                        }
                    }
                }

            }
                
            if (stringBuilder.Length != 0)
            {
                sb.AppendLine(whitespace + "StatsReport_WeaponTraits".Translate() + ":");
                sb.Append(stringBuilder.ToString());
            }

        }

        public bool NeedsReload()
        {
            if (AbilityDetailsForWeapon(GetDetails()) == null)
            {
                return false;
            }
            return currentCharges != maxCharges;
        }

        public int MinAmmoNeeded()
        {
            if (!NeedsReload())
            {
                return 0;
            }
            return AbilityDetailsForWeapon(GetDetails()).ammoCountPerCharge;
        }

        public int MaxAmmoNeeded()
        {
            if (!NeedsReload())
            {
                return 0;
            }
            return AbilityDetailsForWeapon(GetDetails()).ammoCountPerCharge * (maxCharges - currentCharges);
        }

        public void ReloadFrom(Pawn pawn,Thing ammo)
        {
            if (!NeedsReload())
            {
                return;
            }

            if (ammo.stackCount < AbilityDetailsForWeapon(GetDetails()).ammoCountPerCharge)
            {
                return;
            }
            int num = Mathf.Clamp(ammo.stackCount / AbilityDetailsForWeapon(GetDetails()).ammoCountPerCharge, 0, maxCharges - currentCharges);
            ammo.SplitOff(num * AbilityDetailsForWeapon(GetDetails()).ammoCountPerCharge).Destroy();
            currentCharges += num;
            Ability ability = pawn.abilities.GetAbility(abilityWithCharges);
            ability.RemainingCharges += num;
            if (AbilityDetailsForWeapon(GetDetails()).soundReload != null)
            {
                AbilityDetailsForWeapon(GetDetails()).soundReload.PlayOneShot(new TargetInfo(parent.PositionHeld, parent.MapHeld));
            }
        }

        public void Notify_UsedAbility()
        {
            currentCharges--;
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            IEnumerable<StatDrawEntry> enumerable = base.SpecialDisplayStats();
            if (enumerable != null)
            {
                foreach (StatDrawEntry item in enumerable)
                {
                    yield return item;
                }
            }
            if (AbilityDetailsForWeapon(GetDetails())?.abilityDef != null)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Weapon, "Stat_Thing_ReloadChargesRemaining_Name".Translate(AbilityDetailsForWeapon(GetDetails()).ChargeNounArgument), LabelRemaining, "Stat_Thing_ReloadChargesRemaining_Desc".Translate(AbilityDetailsForWeapon(GetDetails()).ChargeNounArgument), 5440);
                yield return new StatDrawEntry(StatCategoryDefOf.Weapon, "VEF.Weapons.Stat_Thing_MaterialPerCharge".Translate(), "VEF.Weapons.Stat_Thing_MaterialPerCharge_Value".Translate(AbilityDetailsForWeapon(GetDetails()).ammoCountPerCharge, AbilityDetailsForWeapon(GetDetails()).ammoDef.LabelCap, AbilityDetailsForWeapon(GetDetails()).maxCharges), "VEF.Weapons.Stat_Thing_MaterialPerCharge_Desc".Translate(AbilityDetailsForWeapon(GetDetails()).ammoDef.label), 5440);

            }
        }



    }
}
