using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using static HarmonyLib.Code;
using RimWorld.Utility;

namespace VEF.Weapons
{
    public class CompApplyWeaponTraits : ThingComp
    {
        public AbilityWithChargesDetails abilityWithChargesDetails;
        public AbilityDef abilityWithCharges;
        public int maxCharges;
        public int currentCharges;

        public string LabelRemaining => $"{currentCharges} / {maxCharges}";

        List<WeaponTraitDefExtension> contentDetails = new List<WeaponTraitDefExtension>();

        CompUniqueWeapon cachedComp;

        public List<WeaponTraitDefExtension> GetDetails()
        {
            if (contentDetails.NullOrEmpty())
            {
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

        public AbilityWithChargesDetails AbilityForWeapon(List<WeaponTraitDefExtension> traits)
        {
            return traits.Where(x => x.abilityWithCharges != null)?.Select(x => x.abilityWithCharges).FirstOrFallback();
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
               if(abilityWithCharges is null)
                {
                    abilityWithChargesDetails = AbilityForWeapon(GetDetails());
                    abilityWithCharges = abilityWithChargesDetails?.abilityDef;
                    if (abilityWithCharges != null)
                    {

                        maxCharges = abilityWithChargesDetails.maxCharges;
                        currentCharges = maxCharges;
                    }
                }
                
            }

        }

        public override string CompInspectStringExtra()
        {
            if (abilityWithCharges is null)
            {
                return null;
            }
           
            return "ChargesRemaining".Translate(abilityWithChargesDetails.chargeNoun) + ": " + LabelRemaining;
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
            contentDetails.Clear();
            cachedComp = null;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            if (!GetDetails().NullOrEmpty())
            {
                LongEventHandler.ExecuteWhenFinished(delegate { ChangeGraphic(); });
            }
        }

        public void ChangeGraphic()
        {
            if (!this.parent.def.IsApparel)
            {
                WeaponTraitDefExtension extension = GetDetails().Where(x => x.graphicOverrides != null && x.graphicOverrides.ContainsKey(this.parent.def))?.RandomElementByWeightWithFallback(x => x.graphicOverrideCommonality);
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
            foreach (WeaponTraitDefExtension extension in contentDetails)
            {
                if (extension?.abilityToAdd != null)
                {
                    pawn.abilities?.GainAbility(extension.abilityToAdd);
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
            foreach (WeaponTraitDefExtension extension in contentDetails)
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
            foreach (WeaponTraitDefExtension extension in contentDetails)
            {
                if (extension?.killThought != null)
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(extension.killThought);
                }
            }
        }
    }
}
