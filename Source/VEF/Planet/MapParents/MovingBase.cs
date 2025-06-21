using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VEF.Planet
{
    [HotSwappable]
    [StaticConstructorOnStartup]
    public class MovingBase : MapParent
    {
        
        public MovingBase_PathFollower pather;
        public MovingBase_Tweener tweener;
        private Material cachedMat;
        public override Vector3 DrawPos => tweener.TweenedPos;
        public MovingBaseDef def => base.def as MovingBaseDef;
        public static readonly Texture2D AttackCommand = ContentFinder<Texture2D>.Get("UI/Commands/AttackSettlement");
        public override Material Material
        {
            get
            {
                if (cachedMat == null)
                {
                    cachedMat = MaterialPool.MatFrom(base.def.expandingIconTexture, ShaderDatabase.WorldOverlayTransparentLit, base.Faction.Color, WorldMaterials.WorldObjectRenderQueue);
                }
                return cachedMat;
            }
        }

        public override Texture2D ExpandingIcon => pather.Moving ? base.ExpandingIcon : ContentFinder<Texture2D>.Get(base.def.texture);

        public MovingBase()
        {
            pather = new MovingBase_PathFollower(this);
            tweener = new MovingBase_Tweener(this);
        }
        public int TicksPerMove => def.ticksPerMove;

        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            alsoRemoveWorldObject = false;
            if (!base.Map.IsPlayerHome)
            {
                return !base.Map.mapPawns.AnyPawnBlockingMapRemoval;
            }
            return false;
        }

        public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
        {
            foreach (Gizmo caravanGizmo in base.GetCaravanGizmos(caravan))
            {
                yield return caravanGizmo;
            }

            if (Attackable)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.icon = AttackCommand;
                command_Action.defaultLabel = "CommandAttackSettlement".Translate();
                command_Action.defaultDesc = "VEF.CommandAttackMovingBaseDesc".Translate(def.label);
                command_Action.action = delegate
                {
                    this.Attack(caravan);
                };
                yield return command_Action;
            }
        }

        public void Attack(Caravan caravan)
        {
            if (HasMap)
            {
                LongEventHandler.QueueLongEvent(delegate
                {
                    AttackNow(caravan);
                }, "GeneratingMapForNewEncounter", doAsynchronously: false, null);
            }
            else
            {
                AttackNow(caravan);
            }
        }

        private void AttackNow(Caravan caravan)
        {
            bool mapWasGenerated = !HasMap;
            Map map = GetOrGenerateMapUtility.GetOrGenerateMap(Tile, null);
            DoMapGeneration(caravan, mapWasGenerated, map);
        }

        protected virtual void DoMapGeneration(Caravan caravan, bool mapWasGenerated, Map map) { }

        public MovingBase BaseVisitedNow(Caravan caravan)
        {
            if (!caravan.Spawned || caravan.pather.Moving)
            {
                return null;
            }
            List<MovingBase> bases = Find.WorldObjects.AllWorldObjects.OfType<MovingBase>()
            .Where(x => x.def == this.def).ToList();
            for (int i = 0; i < bases.Count; i++)
            {
                var settlement = bases[i];
                if (settlement.Tile == caravan.Tile)
                {
                    return settlement;
                }
            }
            return null;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
            foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(caravan))
            {
                yield return floatMenuOption;
            }
            foreach (FloatMenuOption floatMenuOption2 in GetFloatMenuOptions_MovingBase(caravan))
            {
                yield return floatMenuOption2;
            }
        }

        protected virtual IEnumerable<FloatMenuOption> GetFloatMenuOptions_MovingBase(Caravan caravan)
        {
            foreach (FloatMenuOption floatMenuOption4 in CaravanArrivalAction_AttackMovingBase.GetFloatMenuOptions(caravan, this))
            {
                yield return floatMenuOption4;
            }
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            tweener.ResetTweenedPosToRoot();
        }


        public PlanetTile BestGotoDestNear(PlanetTile tile)
        {
            Predicate<PlanetTile> predicate = delegate (PlanetTile t)
            {
                if (Find.World.Impassable(t))
                {
                    return false;
                }
                return CanReach(tile);
            };
            if (predicate(tile))
            {
                return tile;
            }
            GenWorldClosest.TryFindClosestTile(tile, predicate, out var foundTile, 50);
            return foundTile;
        }

        public bool CanReach(PlanetTile tile)
        {
            return Find.WorldReachability.CanReach(this.Tile, tile);
        }

        public override void PostRemove()
        {
            base.PostRemove();
            pather.StopDead();
        }

        public virtual bool Attackable => true;

        protected override void Tick()
        {
            base.Tick();
            if (HasMap is false)
            {
                pather.PatherTick();
                if (this.IsHashIntervalTick(30))
                {
                    tweener.TweenerTick();
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref pather, "pather", this);
        }
    }
}