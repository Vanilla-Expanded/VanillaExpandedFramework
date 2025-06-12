
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.Sound;
using static HarmonyLib.Code;
using Verse.Noise;
using Verse.AI;



namespace VEF.Buildings
{
    public class SwappableBuilding : Building
    {

        SwappableBuildingDetails cachedExtension;

        public SwappableBuildingDetails SwappableExtension
        {
            get
            {
                if (cachedExtension is null)
                {
                    cachedExtension = this.def.GetModExtension<SwappableBuildingDetails>();
                }
                return cachedExtension;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {

            foreach (Gizmo c in base.GetGizmos())
            {
                yield return c;
            }
            if (Prefs.DevMode)
            {

                Command_Action command_Action = new Command_Action();

                command_Action.defaultLabel = "Activate";

                command_Action.action = delegate
                {
                    Notify_Swap();
                };

                yield return command_Action;

            }


        }

        public virtual void Notify_Swap()
        {
            if (SwappableExtension != null)
            {
                if (SwappableExtension.buildingLeft != null)
                {

                    Thing buildingToMake = GenSpawn.Spawn(ThingMaker.MakeThing(SwappableExtension.buildingLeft), Position, Map, Rotation);

                    if (buildingToMake.def.CanHaveFaction)
                    {
                        buildingToMake.SetFaction(this.Faction);
                    }
                }
                if (SwappableExtension.deconstructSound != null)
                {
                    SwappableExtension.deconstructSound.PlayOneShot(this);
                }

                if (this.Spawned)
                {
                    this.DeSpawn();
                }

            }

        }

    }
}