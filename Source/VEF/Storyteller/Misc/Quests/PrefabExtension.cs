using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VEF.Storyteller
{
    public class PrefabExtension : DefModExtension
    {
        public List<PrefabRoofData> roofs;
        public List<PrefabPawnSpawnData> pawns;
    }

    public class PrefabRoofData
    {
        public RoofDef def;
        public List<CellRect> rects;

        // Virtual methods in case we need to make custom data for roofs.
        // For example, colored VacBarrierRoof from VGE.
        public virtual void SetRoof(Map map, IntVec3 root, Rot4 rot)
        {
            foreach (var rect in rects)
            {
                foreach (var cell in rect.Cells)
                {
                    SetRoofAt(map, root + PrefabUtility.GetAdjustedLocalPosition(cell, rot));
                }
            }
        }

        protected virtual void SetRoofAt(Map map, IntVec3 pos)
        {
            map.roofGrid.SetRoof(pos, def);
        }
    }

    public class PrefabPawnSpawnData
    {
        public PawnKindDef kind;
        public IntRange count = IntRange.One;
        public IntVec3 position = IntVec3.Invalid;
        public List<IntVec3> positions;
        public FactionDef factionOverride;
        public float chance = 1f;
        public int initialAttemptRadius = 4;

        public virtual void SpawnPawns(PrefabDef prefab, Map map, IntVec3 root, Rot4 rot, Faction faction, List<Thing> spawned, Action<Thing> onSpawned)
        {
            if (!Rand.Chance(chance))
                return;
            if (kind == null)
            {
                Log.Error($"Trying to spawn a pawn for prefab {prefab} but {nameof(kind)} is not specified.");
                return;
            }
            if (count.IsInvalid)
            {
                Log.Error($"Trying to spawn a pawn for prefab {prefab} but {nameof(count)} is invalid.");
                return;
            }
            if (!position.IsValid && positions.NullOrEmpty())
            {
                Log.Error($"Trying to spawn a pawn for prefab {prefab} but {nameof(position)} and {nameof(positions)} are not specified or valid.");
                return;
            }

            var spawnFaction = faction;
            if (factionOverride != null && Find.FactionManager.AllFactionsListForReading.Where(x => x.def == factionOverride).TryRandomElement(out var foundFaction))
                spawnFaction = foundFaction;

            var range = count.RandomInRange;
            for (var i = 0; i < range; i++)
            {
                var rootCell = positions.NullOrEmpty() ? position : positions.RandomElement();
                if (!rootCell.IsValid) rootCell = root;
                var spawnCell = CellFinder.RandomSpawnCellForPawnNear(root + rootCell, map, initialAttemptRadius);
                if (!spawnCell.IsValid) continue;
                if (!spawnCell.InBounds(map)) continue;

                var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, spawnFaction, forceGenerateNewPawn: true));
                GenSpawn.Spawn(pawn, spawnCell, map);
                spawned?.Add(pawn);
                onSpawned?.Invoke(pawn);
            }
        }
    }
}