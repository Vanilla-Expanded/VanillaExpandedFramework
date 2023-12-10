namespace VFECore.Abilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using RimWorld;
    using Verse;

    public class CompAbilitiesApparel : ThingComp
    {
        public CompProperties_AbilitiesApparel Props => (CompProperties_AbilitiesApparel) this.props;

        private Pawn pawn;
        private Pawn Pawn => (this.parent as Apparel)?.Wearer;
        private List<Ability> abilitiesToTick = new List<Ability>();
        private List<Abilities.Ability> givenAbilities = new List<Abilities.Ability>();
        public List<Abilities.Ability> GivenAbilities => givenAbilities;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);

            foreach (AbilityDef abilityDef in this.Props.abilities)
            {
                Abilities.Ability ability = (Abilities.Ability)Activator.CreateInstance(abilityDef.abilityClass);
                ability.def    = abilityDef;
                ability.holder = this.parent;
                ability.Init();
                this.givenAbilities.Add(ability);
                if (ability.def.needsTicking)
                {
                    this.abilitiesToTick.Add(ability);
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetWornGizmosExtra()) 
                yield return gizmo;

            if (this.Pawn == null)
                yield break;
            
            if (this.Pawn != this.pawn)
            {
                this.pawn = this.Pawn;
                foreach (Abilities.Ability ability in this.givenAbilities)
                {
                    ability.pawn = this.pawn;
                    ability.Init();
                }
            }

            foreach (Abilities.Ability ability in this.givenAbilities) 
                if(ability.ShowGizmoOnPawn())
                    yield return ability.GetGizmo();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref this.givenAbilities, nameof(this.givenAbilities), LookMode.Deep);

            if (this.givenAbilities == null)
                this.givenAbilities = new List<Abilities.Ability>();
            else if(Scribe.mode == LoadSaveMode.LoadingVars)
            {
                foreach (Abilities.Ability ability in this.givenAbilities)
                {
                    ability.holder = this.parent;
                }
                if (this.givenAbilities?.Any() ?? false)
                {
                    this.abilitiesToTick = this.givenAbilities.Where(x => x.def.needsTicking).ToList();
                }
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            int abilitiesToTickCount = this.abilitiesToTick.Count;
            for (var i = 0; i < abilitiesToTickCount; i++)
            {
                this.abilitiesToTick[i].Tick();
            }
        }
    }

    public class CompProperties_AbilitiesApparel : CompProperties
    {
        public List<AbilityDef> abilities;

        public CompProperties_AbilitiesApparel() => 
            this.compClass = typeof(CompAbilitiesApparel);
    }
}