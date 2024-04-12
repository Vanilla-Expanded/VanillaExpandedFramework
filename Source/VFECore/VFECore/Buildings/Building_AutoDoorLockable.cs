using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VFECore
{
    public enum DoorAccess
    {
        Default,
        Everyone,
        OnlyColonistsAndAnimals,
        OnlyColonistsAndAnimalsAndFriendlies,
        OnlyDrafted
    }

    [StaticConstructorOnStartup]
    public class Building_AutoDoorLockable : Building_Door
    {
        public static readonly Texture2D DoorStateButton = ContentFinder<Texture2D>.Get("UI/Overlays/DoorStateButton");

        private DoorAccess curDoorAccess = DoorAccess.Default;

        private Dictionary<DoorAccess, string> doorStates = new Dictionary<DoorAccess, string> {
            {DoorAccess.Default, "VEF.DoorLockDefault".Translate()},
            {DoorAccess.Everyone, "VEF.DoorLockEveryone".Translate()},
            {DoorAccess.OnlyColonistsAndAnimals, "VEF.DoorLockOnlyColonistsAndAnimals".Translate()},
            {DoorAccess.OnlyColonistsAndAnimalsAndFriendlies, "VEF.DoorLockOnlyColonistsAndAnimalsAndFriendlies".Translate()},
            {DoorAccess.OnlyDrafted, "VEF.DoorLockOnlyDrafted".Translate()},
        };
        public override bool PawnCanOpen(Pawn p)
        {
            switch (curDoorAccess)
            {
                case DoorAccess.Default: return base.PawnCanOpen(p);
                case DoorAccess.Everyone: return true;
                case DoorAccess.OnlyColonistsAndAnimals: return OnlyColonistsAndAnimals(p);
                case DoorAccess.OnlyColonistsAndAnimalsAndFriendlies: return OnlyColonistsAndAnimalsAndFriendlies(p);
                case DoorAccess.OnlyDrafted: return OnlyDrafted(p);
                default: return true;
            }
        }

        public Material DoorStateMaterial
        {
            get
            {
                switch (curDoorAccess)
                {
                    case DoorAccess.Everyone: return MaterialPool.MatFrom("UI/Overlays/DoorStateOverlay_Green");
                    case DoorAccess.OnlyColonistsAndAnimals: return MaterialPool.MatFrom("UI/Overlays/DoorStateOverlay_Orange");
                    case DoorAccess.OnlyColonistsAndAnimalsAndFriendlies: return MaterialPool.MatFrom("UI/Overlays/DoorStateOverlay_Blue");
                    case DoorAccess.OnlyDrafted: return MaterialPool.MatFrom("UI/Overlays/DoorStateOverlay_Red");
                    default: return MaterialPool.MatFrom("UI/Overlays/DoorStateOverlay_Green");
                }
            }
        }
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            if (!this.Open)
            {
                var num = Mathf.Clamp01((float)ticksSinceOpen / (float)TicksToOpenNow);
                if (num == 0)
                {
                    Vector3 drawPos = DrawPos;
                    drawPos.y = AltitudeLayer.DoorMoveable.AltitudeFor() + 1;
                    Graphics.DrawMesh(MeshPool.plane10, drawPos, base.Rotation.AsQuat, DoorStateMaterial, 0);
                }
            }
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            var command = new Command_Action();
            command.action = delegate ()
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (var doorState in doorStates)
                {
                    list.Add(new FloatMenuOption(doorState.Value, delegate
                    {
                        curDoorAccess = doorState.Key;
                    }, MenuOptionPriority.Default));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            };
            command.defaultLabel = "VEF.DoorLockState".Translate(doorStates[curDoorAccess]);
            command.defaultDesc = "VEF.DoorLockStateDesc".Translate();
            command.Disabled = !this.powerComp.PowerOn;
            command.disabledReason = "VEF.DoorLockStatePowerOff".Translate();
            command.icon = DoorStateButton;
            yield return command;
        }
        private bool OnlyColonistsAndAnimals(Pawn p)
        {
            if (p.Faction == this.Faction)
            {
                return true;
            }
            return false;
        }

        private bool OnlyColonistsAndAnimalsAndFriendlies(Pawn p)
        {
            if (p.Faction != null && (p.Faction == this.Faction || !p.Faction.HostileTo(this.Faction)))
            {
                return true;
            }
            return false;
        }
        private bool OnlyDrafted(Pawn p)
        {
            if (p.Faction == this.Faction && p.Drafted)
            {
                return true;
            }
            return false;
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref curDoorAccess, "curDoorAccess");
        }
    }
}
