namespace VFECore.Abilities
{
    using System.Collections.Generic;
    using Verse;

    public class AbilityManager : GameComponent
    {
        private List<Ability> abilities;
        public static AbilityManager Instance;
        public AbilityManager(Game game)
        {
            PreInit();
        }
        public void PreInit()
        {
            Instance = this;
            if (abilities == null)
            {
                abilities = new List<Ability>();
            };
        }
        public void AddAbility(Ability ability)
        {
            if (!abilities.Contains(ability))
            {
                abilities.Add(ability);
            }
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            foreach (var ability in abilities)
            {
                ability.Tick();
            }
        }

        public override void GameComponentUpdate()
        {
            base.GameComponentUpdate();
            foreach (var ability in abilities)
            {
                ability.Update();
            }
        }
        public override void ExposeData()
        {
            PreInit();
            base.ExposeData();
            Scribe_Collections.Look(ref abilities, "abilities", LookMode.Reference);
            abilities.RemoveAll(x => x is null || x.pawn is null);
        }
    }
}
