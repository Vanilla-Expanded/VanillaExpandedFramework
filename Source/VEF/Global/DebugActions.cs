using System.Collections.Generic;

namespace VEF
{
    using LudeonTK;
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine;
    using Verse;

    public static class DebugActions
    {
        [DebugAction("Spawning", actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
        public static void SpawnWorldObjectLayered()
        {

            Ray ray            = Find.WorldCamera.ScreenPointToRay(UI.MousePositionOnUI * Prefs.UIScale);
            int worldLayerMask = WorldCameraManager.WorldLayerMask;

            PlanetLayer layerOld = PlanetLayer.Selected;
            PlanetLayer.Selected = Find.WorldGrid.FirstLayerOfDef(PlanetLayerDefOf.Surface);
            WorldTerrainColliderManager.EnsureRaycastCollidersUpdated();

            PlanetTile planetTile = PlanetTile.Invalid;

            if (Physics.Raycast(ray, out RaycastHit hitInfo, 1500f, worldLayerMask))
                foreach (WorldDrawLayerBase allVisibleDrawLayer in Find.World.renderer.AllVisibleDrawLayers)
                    if (allVisibleDrawLayer is WorldDrawLayer { Raycastable: true } worldDrawLayer && worldDrawLayer.TryGetTileFromRayHit(hitInfo, out PlanetTile id))
                        planetTile = id;

            PlanetLayer.Selected = layerOld;

            List<DebugMenuOption> list = [];

            foreach ((int _, PlanetLayer planetLayer) in Find.WorldGrid.PlanetLayers)
            {
                PlanetLayer layer = planetLayer;
                list.Add(new DebugMenuOption($"({planetLayer.LayerID}) {planetLayer.Def.defName}", DebugMenuOptionMode.Action, () =>
                                                                                                                               {
                                                                                                                                   planetTile = layer.GetClosestTile_NewTemp(planetTile);
                                                                                                                                   if (!planetTile.Valid)
                                                                                                                                   {
                                                                                                                                       Messages.Message("Invalid", MessageTypeDefOf.RejectInput, historical: false);
                                                                                                                                       return;
                                                                                                                                   }
                                                                                                                                   
                                                                                                                                   List<DebugMenuOption> list2 = [];
                                                                                                                                   foreach (WorldObjectDef allDef in DefDatabase<WorldObjectDef>.AllDefs)
                                                                                                                                   {
                                                                                                                                       WorldObjectDef localDef = allDef;
                                                                                                                                       list2.Add(new DebugMenuOption(localDef.defName, DebugMenuOptionMode.Action, delegate
                                                                                                                                       {
                                                                                                                                           planetTile = layer.GetClosestTile_NewTemp(planetTile);
                                                                                                                                           if (!planetTile.Valid)
                                                                                                                                           {
                                                                                                                                               Messages.Message("Invalid", MessageTypeDefOf.RejectInput, historical: false);
                                                                                                                                           }
                                                                                                                                           else
                                                                                                                                           {
                                                                                                                                               WorldObject worldObject = WorldObjectMaker.MakeWorldObject(localDef);
                                                                                                                                               worldObject.Tile = planetTile;
                                                                                                                                               Find.WorldObjects.Add(worldObject);
                                                                                                                                           }
                                                                                                                                       }));
                                                                                                                                   }

                                                                                                                                   Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
                                                                                                                               }));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
        }
    }
}
