namespace VFECore.Abilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AnimalBehaviours;
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine;
    using Verse;
    using Verse.Sound;

    public class CompAbilities : CompShieldBubble, PawnGizmoProvider
    {
        private new Pawn Pawn => (Pawn) this.parent;

        private List<Abilities.Ability> learnedAbilities = new List<Abilities.Ability>();
        private List<Ability> abilitiesToTick = new List<Ability>();

        public Abilities.Ability  currentlyCasting;
        public GlobalTargetInfo[] currentlyCastingTargets;

        private float energyMax;
        public override float EnergyMax => this.energyMax;

        protected override float EnergyGainPerTick => 0f;

        private float breakTicks = -1;

        public List<Abilities.Ability> LearnedAbilities => this.learnedAbilities;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if(this.learnedAbilities == null)
                this.learnedAbilities = new List<Abilities.Ability>();
            this.ticksToReset = int.MaxValue;
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            this.ticksToReset = int.MaxValue;
        }

        public void GiveAbility(AbilityDef abilityDef)
        {
            if (this.learnedAbilities.Any(ab => ab.def == abilityDef))
                return;

            Abilities.Ability ability = (Abilities.Ability) Activator.CreateInstance(abilityDef.abilityClass);
            ability.def    = abilityDef;
            ability.pawn   = this.Pawn;
            ability.holder = this.Pawn;
            ability.Init();

            this.learnedAbilities.Add(ability);
            if (ability.def.needsTicking)
            {
                this.abilitiesToTick.Add(ability);
            }
            this.learnedAbilities = this.LearnedAbilities.OrderBy(ab => ab.def.requiredHediff?.minimumLevel ?? 0).GroupBy(ab => ab.Hediff).SelectMany(grp => grp).ToList();
        }

        public bool HasAbility(AbilityDef abilityDef)
        {
            foreach (Abilities.Ability learnedAbility in this.learnedAbilities)
                if (learnedAbility.def == abilityDef)
                    return true;
            return false;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.ShieldState == ShieldState.Active)
            {
                this.breakTicks--;
                if(this.breakTicks <= 0)
                    this.Break();
            }
            int abilitiesToTickCount = this.abilitiesToTick.Count;
            for (var i = 0; i < abilitiesToTickCount; i++)
                this.abilitiesToTick[i].Tick();
        }

        public override string CompInspectStringExtra() => string.Empty;

        public List<GlobalTargetInfo> tmpCurrentlyCastingTargets;
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref this.learnedAbilities, nameof(this.learnedAbilities), LookMode.Deep);
            Scribe_References.Look(ref this.currentlyCasting, nameof(this.currentlyCasting));

            tmpCurrentlyCastingTargets = currentlyCastingTargets?.ToList() ?? new List<GlobalTargetInfo>();
            Scribe_Collections.Look(ref this.tmpCurrentlyCastingTargets, nameof(this.currentlyCastingTargets));
            currentlyCastingTargets = this.tmpCurrentlyCastingTargets?.ToArray();

            Scribe_Values.Look(ref this.energyMax, nameof(this.energyMax));
            Scribe_Values.Look(ref this.shieldPath, nameof(this.shieldPath));

            if (this.learnedAbilities == null)
                this.learnedAbilities = new List<Abilities.Ability>();
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                foreach (Abilities.Ability ability in this.learnedAbilities)
                {
                    ability.holder = this.parent;
                }
            }
            else if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                foreach (Ability ability in this.learnedAbilities)
                {
                    if (ability.pawn == null && this.parent is Pawn pawn)
                    {
                        ability.pawn = pawn;
                    }
                }
            }
            if (this.learnedAbilities?.Any() ?? false)
            {
                this.abilitiesToTick = this.learnedAbilities.Where(x => x.def.needsTicking).ToList();
            }
        }

        protected override void Break()
        {
            base.Break();
            this.energyMax = 0f;
        }

        protected override void Reset() => 
            this.ticksToReset = int.MaxValue;

        public bool ReinitShield(float newEnergy, string shieldTexturePath, int duration)
        {
            if (newEnergy < this.energy)
                return false;

            if (this.Pawn.Spawned)
            {
                SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(this.Pawn.Position, this.Pawn.Map));
                FleckMaker.ThrowLightningGlow(this.Pawn.TrueCenter(), this.Pawn.Map, 3f);
            }



            this.ticksToReset = -1;
            this.breakTicks   = duration;
            this.energyMax    = newEnergy;
            this.energy       = newEnergy;
            this.shieldPath   = shieldTexturePath;

            return true;
        }

        public IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Abilities.Ability ability in this.learnedAbilities)
                if (ability.ShowGizmoOnPawn())
                    yield return ability.GetGizmo();

            var hediffsAbilities = new List<Hediff_Abilities>();
            Pawn.health.hediffSet.GetHediffs<Hediff_Abilities>(ref hediffsAbilities);
            foreach (Hediff_Abilities hediff in hediffsAbilities)
            {
                foreach (Gizmo gizmo in hediff.DrawGizmos())
                    yield return gizmo;
            }
        }

        private string currentShieldPath;
        private string shieldPath;

        protected override Material BubbleMat
        {
            get
            {
                if (this.bubbleMat is null || this.currentShieldPath != this.shieldPath)
                {
                    if (this.shieldPath.NullOrEmpty())
                        return base.BubbleMat;
                    this.bubbleMat         = MaterialPool.MatFrom(this.shieldPath, ShaderDatabase.Transparent, this.Props.shieldColor);
                    this.currentShieldPath = this.shieldPath;
                }

                return this.bubbleMat;
            }
        }
    }
}
