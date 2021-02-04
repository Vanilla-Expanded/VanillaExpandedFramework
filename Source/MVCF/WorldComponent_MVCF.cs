using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RimWorld.Planet;
using Verse;

namespace MVCF
{
    public class WorldComponent_MVCF : WorldComponent
    {
        private static WorldComponent_MVCF localCache;

        public readonly List<System.WeakReference<VerbManager>> allManagers =
            new List<System.WeakReference<VerbManager>>();

        private readonly int key;

        private readonly ConditionalWeakTable<Pawn, VerbManager> managers =
            new ConditionalWeakTable<Pawn, VerbManager>();

        public readonly List<System.WeakReference<VerbManager>> TickManagers =
            new List<System.WeakReference<VerbManager>>();

        public Dictionary<Pawn, Verb> currentVerbSaved = new Dictionary<Pawn, Verb>();

        public WorldComponent_MVCF(World world) : base(world)
        {
            key = world.ConstantRandSeed;
            localCache = this;
        }

        public static WorldComponent_MVCF GetComp()
        {
            var getKey = Find.World.ConstantRandSeed;
            if (getKey != localCache.key)
                localCache = Find.World.GetComponent<WorldComponent_MVCF>();

            return localCache;
        }

        public VerbManager GetManagerFor(Pawn pawn, bool createIfMissing = true)
        {
            if (managers.TryGetValue(pawn, out var manager)) return manager;
            if (!createIfMissing) return null;
            manager = new VerbManager();
            manager.Initialize(pawn);
            if (currentVerbSaved != null && currentVerbSaved.TryGetValue(pawn, out var currentVerb))
                manager.CurrentVerb = currentVerb;
            managers.Add(pawn, manager);
            allManagers.Add(new System.WeakReference<VerbManager>(manager));
            return manager;
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            foreach (var wr in TickManagers)
                if (wr.TryGetTarget(out var man))
                    man.Tick();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            currentVerbSaved.Clear();
            allManagers.ForEach(vm =>
            {
                if (vm.TryGetTarget(out var man) && man.Pawn != null && man.Pawn.Spawned && !man.Pawn.Dead)
                    currentVerbSaved.SetOrAdd(man.Pawn, man.CurrentVerb);
            });
            Scribe_Collections.Look(ref currentVerbSaved, "currentVerbs", LookMode.Reference, LookMode.Reference);
        }
    }
}