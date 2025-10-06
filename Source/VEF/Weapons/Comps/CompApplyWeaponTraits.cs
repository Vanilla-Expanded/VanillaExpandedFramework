﻿using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace VEF.Weapons
{
    public class CompApplyWeaponTraits : ThingComp
    {

        List<WeaponTraitDefExtension> contentDetails = new List<WeaponTraitDefExtension>();

        CompUniqueWeapon cachedComp;

        public List<WeaponTraitDefExtension> GetDetails()
        {          
            if (contentDetails.NullOrEmpty())
            {
                CompUniqueWeapon comp = GetComp();
                if (comp != null && comp.TraitsListForReading?.Count>0)
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

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!GetDetails().NullOrEmpty())
            {
                LongEventHandler.ExecuteWhenFinished(delegate { ChangeGraphic(); });
            }
        }

        public override void Notify_DefsHotReloaded()
        {
            base.Notify_DefsHotReloaded();
            if (!GetDetails().NullOrEmpty())
            {
                LongEventHandler.ExecuteWhenFinished(delegate { ChangeGraphic(); });
            }
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
            if (!this.parent.def.IsApparel) {
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
            base.Notify_Equipped(pawn);
            foreach(WeaponTraitDefExtension extension in contentDetails)
            {
                if (extension?.abilityToAdd != null)
                {
                    pawn.abilities?.GainAbility(extension.abilityToAdd);
                }

              
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            foreach (WeaponTraitDefExtension extension in contentDetails)
            {
                if (extension?.abilityToAdd != null)
                {
                    pawn.abilities?.RemoveAbility(extension.abilityToAdd);
                }
               
            }
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
