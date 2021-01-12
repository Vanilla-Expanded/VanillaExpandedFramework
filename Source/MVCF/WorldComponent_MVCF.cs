using System.Runtime.CompilerServices;
using RimWorld.Planet;
using Verse;

namespace MVCF
{
    public class WorldComponent_MVCF : WorldComponent
    {
        private static WorldComponent_MVCF localCache;
        private readonly int key;

        private readonly ConditionalWeakTable<Pawn, VerbManager> managers =
            new ConditionalWeakTable<Pawn, VerbManager>();

        public WorldComponent_MVCF(World world) : base(world)
        {
            key = world.ConstantRandSeed;
            localCache = this;
        }

        public static WorldComponent_MVCF GetStorage()
        {
            var getKey = Find.World.ConstantRandSeed;
            if (getKey != localCache.key)
                localCache = Find.World.GetComponent<WorldComponent_MVCF>();

            return localCache;
        }

        public VerbManager GetManagerFor(Pawn pawn)
        {
            if (managers.TryGetValue(pawn, out var manager)) return manager;
            manager = new VerbManager();
            manager.Initialize(pawn);
            managers.Add(pawn, manager);
            return manager;
        }
    }
}