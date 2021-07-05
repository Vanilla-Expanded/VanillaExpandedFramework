using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaWeaponsExpandedLaser
{
    public class CompProperties_LaserCapacitor : CompProperties
    {
        public CompProperties_LaserCapacitor()
        {
            this.compClass = typeof(CompLaserCapacitor);
        }
        public bool Overheats = false;
        public bool OverheatDestroys = true;
        public float OverheatChance = 0.05f;
        public string OverheatBlastDamageDef = "Burn";
        public int OverheatBlastExtraDamage = 5;
        public float OverheatBlastRadius = 1.5f;
        public float WarmUpReductionPerShot = 0.1f;
        public ThingDef OverheatMoteThrown = null;
        public float OverheatMoteSize = 0.5f;
        public string UiIconPath = string.Empty;
    }

    // Token: 0x02000002 RID: 2
    public class CompLaserCapacitor : ThingComp
    {
        public CompProperties_LaserCapacitor Props => (CompProperties_LaserCapacitor)this.props;
        public LocalTargetInfo lastFiringLocation = null;
        public int shotstack = 0;
        public float originalwarmupTime;
        public CompEquippable equippable => this.parent.TryGetComp<CompEquippable>();
        public bool hotshot;
        public bool initalized;
        protected virtual bool IsWorn => (GetWearer != null);
        protected virtual Pawn GetWearer
        {
            get
            {
                if (ParentHolder != null && ParentHolder is Pawn_EquipmentTracker)
                {
                    return (Pawn)ParentHolder.ParentHolder;
                }
                else
                {
                    return null;
                }
            }
        }

        private Texture2D CommandTex
        {
            get
            {
                return Props.UiIconPath.NullOrEmpty() ? this.parent.def.uiIcon : ContentFinder<Texture2D>.Get(Props.UiIconPath);
            }
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {

            ThingWithComps owner = IsWorn ? GetWearer : parent;
            bool flag = Find.Selector.SingleSelectedThing == GetWearer;
            if (flag && GetWearer.Drafted && GetWearer.IsColonist)
            {

                int num = 700000101;
                yield return new Command_Toggle
                {
                    icon = this.CommandTex,
                    defaultLabel = "VWEL_ToggleHotshotLabel".Translate(),
                    defaultDesc = "VWEL_ToggleHotshotDesc".Translate(),
                    isActive = (() => hotshot),
                    toggleAction = delegate ()
                    {
                        hotshot = !hotshot;
                    },
                    activateSound = SoundDef.Named("Click"),
                    groupKey = num,
                    hotKey = KeyBindingDefOf.Misc2
                };
            }
            yield break;
        }
        

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_TargetInfo.Look(ref this.lastFiringLocation, "lastFiringLocation", LocalTargetInfo.Invalid);
            Scribe_Values.Look(ref this.shotstack, "shotstack", 0);
            Scribe_Values.Look(ref this.originalwarmupTime, "originalwarmupTime");
            Scribe_Values.Look(ref this.hotshot, "hotshot", false);
            Scribe_Values.Look(ref this.initalized, "initalized", false);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                if (initalized)
                {
                //    Log.Message("is got a wazer wifles");
                    this.originalwarmupTime = this.parent.def.Verbs[0].warmupTime;
                }
            }
        }

        public void CriticalOverheatExplosion(Verb_Shoot __instance)
        {
            Map map = __instance.caster.Map;
            if (__instance.Projectile.projectile.explosionEffect != null)
            {
                Effecter effecter = __instance.Projectile.projectile.explosionEffect.Spawn();
                effecter.Trigger(new TargetInfo(__instance.EquipmentSource.Position, map, false), new TargetInfo(__instance.EquipmentSource.Position, map, false));
                effecter.Cleanup();
            }
            IntVec3 position = __instance.caster.Position;
            Map map2 = map;
            float explosionRadius = Props.OverheatBlastRadius;
            DamageDef damageDef = DefDatabase<DamageDef>.GetNamed(Props.OverheatBlastDamageDef);
            Thing launcher = __instance.EquipmentSource;
            int DamageAmount = Props.OverheatBlastExtraDamage;
            float ArmorPenetration = __instance.Projectile.projectile.GetArmorPenetration(__instance.EquipmentSource, null);
            SoundDef soundExplode = __instance.Projectile.projectile.soundExplode == null ? damageDef.soundExplosion : __instance.Projectile.projectile.soundExplode;
            ThingDef equipmentDef = __instance.EquipmentSource.def;
            ThingDef def = __instance.EquipmentSource.def;
            Thing thing = __instance.EquipmentSource;
            ThingDef postExplosionSpawnThingDef = __instance.Projectile.projectile.postExplosionSpawnThingDef;
            float postExplosionSpawnChance = __instance.Projectile.projectile.postExplosionSpawnChance;
            int postExplosionSpawnThingCount = __instance.Projectile.projectile.postExplosionSpawnThingCount;
            ThingDef preExplosionSpawnThingDef = __instance.Projectile.projectile.preExplosionSpawnThingDef;
            GenExplosion.DoExplosion(position, map2, explosionRadius, damageDef, launcher, DamageAmount, ArmorPenetration, soundExplode);//, equipmentDef, def, thing, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, EquipmentSource.def.projectile.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef, EquipmentSource.def.projectile.preExplosionSpawnChance, EquipmentSource.def.projectile.preExplosionSpawnThingCount, EquipmentSource.def.projectile.explosionChanceToStartFire, EquipmentSource.def.projectile.explosionDamageFalloff);
            return;
        }
    }
}
