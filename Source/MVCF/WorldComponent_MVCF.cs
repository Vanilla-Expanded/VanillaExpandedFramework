using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RimWorld.Planet;
using Verse;

namespace MVCF
{
    public class WorldComponent_MVCF : WorldComponent
    {
        public static WorldComponent_MVCF Instance;

        private readonly ConditionalWeakTable<Pawn, VerbManager> managers = new();

        public readonly List<System.WeakReference<VerbManager>> TickManagers = new();

        public WorldComponent_MVCF(World world) : base(world) => Instance = this;

        public void SaveManager(Pawn pawn)
        {
            if (managers.TryGetValue(pawn, out var man)) managers.Remove(pawn);
            else man = null;
            Scribe_Deep.Look(ref man, "MVCF_VerbManager");
            if (man is null) return;
            managers.Add(pawn, man);
            if (Scribe.mode == LoadSaveMode.PostLoadInit) man.Initialize(pawn);
        }

        public VerbManager GetManagerFor(Pawn pawn, bool createIfMissing = true)
        {
            if (managers.TryGetValue(pawn, out var manager) && manager is not null) return manager;
            if (!createIfMissing) return null;
            manager = new VerbManager();
            manager.Initialize(pawn);
            managers.Add(pawn, manager);
            return manager;
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < TickManagers.Count; i++)
                if (TickManagers[i].TryGetTarget(out var man))
                    man.Tick();
        }
    }
}