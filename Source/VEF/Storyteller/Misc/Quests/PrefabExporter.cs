using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using System.Text;
using System.Reflection;
using LudeonTK;

namespace VEF.Storyteller
{
    public static class PrefabExporter
    {
        private static readonly FieldInfo thingsField = typeof(PrefabDef).GetField("things", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static readonly FieldInfo terrainField = typeof(PrefabDef).GetField("terrain", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        [DebugAction("KCSG", "Mass export prefabs", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void MassExportPrefabs()
        {
            DebugToolsGeneral.GenericRectTool("Mass export", delegate (CellRect rect)
            {
                Find.WindowStack.Add(new Dialog_NameMassExport(rect));
            }, closeOnComplete: true);
        }

        public static string GeneratePrefabXml(PrefabDef prefabDef, string defName, CellRect cellRect, bool includeRoof = false)
        {
            var sb = new StringBuilder();
            var indent = "  ";
            sb.AppendLine("<PrefabDef>");
            sb.AppendLine(indent + "<defName>" + defName + "</defName>");
            sb.AppendLine($"{indent}<size>({cellRect.Width},{cellRect.Height})</size>");

            var things = (List<PrefabThingData>)thingsField?.GetValue(prefabDef);
            if (things != null && things.Count > 0)
            {
                sb.AppendLine(indent + "<things>");
                for (var i = 0; i < things.Count; i++)
                {
                    var thingData = things[i];
                    sb.AppendLine(indent + indent + "<" + thingData.def.defName + ">");
                    if (thingData.rects != null)
                    {
                        sb.AppendLine(indent + indent + indent + "<rects>");
                        foreach (var rect in thingData.rects)
                        {
                            sb.AppendLine($"{indent}{indent}{indent}{indent}<li>{rect}</li>");
                        }
                        sb.AppendLine(indent + indent + indent + "</rects>");
                    }
                    else if (thingData.positions != null)
                    {
                        sb.AppendLine(indent + indent + indent + "<positions>");
                        foreach (var position in thingData.positions)
                        {
                            sb.AppendLine($"{indent}{indent}{indent}{indent}<li>{position}</li>");
                        }
                        sb.AppendLine(indent + indent + indent + "</positions>");
                    }
                    else
                    {
                        sb.AppendLine($"{indent}{indent}{indent}<position>{thingData.position}</position>");
                    }
                    if (thingData.relativeRotation != RotationDirection.None)
                    {
                        sb.AppendLine(indent + indent + indent + "<relativeRotation>" + Enum.GetName(typeof(RotationDirection), thingData.relativeRotation) + "</relativeRotation>");
                    }
                    if (thingData.stuff != null)
                    {
                        sb.AppendLine(indent + indent + indent + "<stuff>" + thingData.stuff.defName + "</stuff>");
                    }
                    if (thingData.quality.HasValue)
                    {
                        sb.AppendLine($"{indent}{indent}{indent}<quality>{thingData.quality}</quality>");
                    }
                    if (thingData.hp != 0)
                    {
                        sb.AppendLine($"{indent}{indent}{indent}<hp>{thingData.hp}</hp>");
                    }
                    if (thingData.stackCountRange != IntRange.One)
                    {
                        sb.AppendLine($"{indent}{indent}{indent}<stackCountRange>{thingData.stackCountRange.min}~{thingData.stackCountRange.max}</stackCountRange>");
                    }
                    if (thingData.colorDef != null)
                    {
                        sb.AppendLine($"{indent}{indent}{indent}<colorDef>{thingData.colorDef}</colorDef>");
                    }
                    if (thingData.color != default(Color))
                    {
                        sb.AppendLine($"{indent}{indent}{indent}<color>{thingData.color}</color>");
                    }
                    sb.AppendLine(indent + indent + "</" + thingData.def.defName + ">");
                }
                sb.AppendLine(indent + "</things>");
            }

            var terrain = (List<PrefabTerrainData>)terrainField?.GetValue(prefabDef);
            if (terrain != null && terrain.Count > 0)
            {
                sb.AppendLine(indent + "<terrain>");
                foreach (var item in terrain)
                {
                    sb.AppendLine(indent + indent + "<" + item.def.defName + ">");
                    if (item.color != null)
                    {
                        sb.AppendLine($"{indent}{indent}{indent}<color>{item.color}</color>");
                    }
                    sb.AppendLine(indent + indent + indent + "<rects>");
                    foreach (var rect2 in item.rects)
                    {
                        sb.AppendLine($"{indent}{indent}{indent}{indent}<li>{rect2}</li>");
                    }
                    sb.AppendLine(indent + indent + indent + "</rects>");
                    sb.AppendLine(indent + indent + "</" + item.def.defName + ">");
                }
                sb.AppendLine(indent + "</terrain>");
            }

            if (includeRoof)
            {
                var map = Find.CurrentMap;
                var roofDict = new Dictionary<RoofDef, List<IntVec3>>();
                foreach (var cell in cellRect.Cells)
                {
                    var roof = map.roofGrid.RoofAt(cell);
                    if (roof != null)
                    {
                        if (!roofDict.ContainsKey(roof)) roofDict[roof] = new List<IntVec3>();
                        roofDict[roof].Add(cell);
                    }
                }

                if (roofDict.Count > 0)
                {
                    sb.AppendLine(indent + "<modExtensions>");
                    sb.AppendLine(indent + indent + "<li Class=\"VEF.Storyteller.PrefabExtension\">");
                    sb.AppendLine(indent + indent + indent + "<roofs>");
                    foreach (var kvp in roofDict)
                    {
                        sb.AppendLine(indent + indent + indent + indent + "<li>");
                        sb.AppendLine(indent + indent + indent + indent + indent + "<def>" + kvp.Key.defName + "</def>");
                        sb.AppendLine(indent + indent + indent + indent + indent + "<rects>");
                        foreach (var r in cellRect.EnumerateRectanglesCovering(c => kvp.Value.Contains(c)))
                        {
                            sb.AppendLine($"{indent}{indent}{indent}{indent}{indent}{indent}<li>{r.MovedBy(-cellRect.Min)}</li>");
                        }
                        sb.AppendLine(indent + indent + indent + indent + indent + "</rects>");
                        sb.AppendLine(indent + indent + indent + indent + "</li>");
                    }
                    sb.AppendLine(indent + indent + indent + "</roofs>");
                    sb.AppendLine(indent + indent + "</li>");
                    sb.AppendLine(indent + "</modExtensions>");
                }
            }

            sb.AppendLine("</PrefabDef>");
            return sb.ToString();
        }

        private class Dialog_NameMassExport : Window
        {
            private string prefix = "NewPrefab";
            private bool includeRoof = true;
            private CellRect rect;
            public override Vector2 InitialSize => new Vector2(300f, 150f);

            public Dialog_NameMassExport(CellRect rect)
            {
                this.rect = rect;
                doCloseX = true;
                forcePause = true;
                absorbInputAroundWindow = true;
            }

            public override void DoWindowContents(Rect inRect)
            {
                var listing = new Listing_Standard();
                listing.Begin(inRect);
                listing.Label("Prefix:");
                prefix = listing.TextEntry(prefix);
                listing.CheckboxLabeled("Include roof", ref includeRoof);
                if (listing.ButtonText("Accept"))
                {
                    MassExport(rect, prefix, includeRoof);
                    Close();
                }
                listing.End();
            }

            private void MassExport(CellRect rect, string prefix, bool includeRoof)
            {
                var currentMap = Find.CurrentMap;
                var list = new List<CellRect>();
                var width = rect.maxX - rect.minX + 1;
                var height = rect.maxZ - rect.minZ + 1;
                var separatorX = new HashSet<int>();
                for (var x = rect.minX; x <= rect.maxX; x++)
                {
                    var goldCount = 0;
                    for (var z = rect.minZ; z <= rect.maxZ; z++)
                    {
                        var terrain = new IntVec3(x, 0, z).GetTerrain(currentMap);
                        if (terrain != null && terrain.defName == "GoldTile")
                        {
                            goldCount++;
                        }
                    }
                    if (goldCount >= Math.Max(5, height * 0.3f))
                    {
                        separatorX.Add(x);
                    }
                }
                var separatorZ = new HashSet<int>();
                for (var z = rect.minZ; z <= rect.maxZ; z++)
                {
                    var goldCount = 0;
                    for (var x = rect.minX; x <= rect.maxX; x++)
                    {
                        var terrain = new IntVec3(x, 0, z).GetTerrain(currentMap);
                        if (terrain != null && terrain.defName == "GoldTile")
                        {
                            goldCount++;
                        }
                    }
                    if (goldCount >= Math.Max(5, width * 0.3f))
                    {
                        separatorZ.Add(z);
                    }
                }
                var xIntervals = new List<(int min, int max)>();
                var startX = -1;
                for (var x = rect.minX; x <= rect.maxX; x++)
                {
                    if (separatorX.Contains(x))
                    {
                        if (startX != -1)
                        {
                            xIntervals.Add((startX, x - 1));
                            startX = -1;
                        }
                    }
                    else if (startX == -1)
                    {
                        startX = x;
                    }
                }
                if (startX != -1)
                {
                    xIntervals.Add((startX, rect.maxX));
                }
                var zIntervals = new List<(int min, int max)>();
                var startZ = -1;
                for (var z = rect.minZ; z <= rect.maxZ; z++)
                {
                    if (separatorZ.Contains(z))
                    {
                        if (startZ != -1)
                        {
                            zIntervals.Add((startZ, z - 1));
                            startZ = -1;
                        }
                    }
                    else if (startZ == -1)
                    {
                        startZ = z;
                    }
                }
                if (startZ != -1)
                {
                    zIntervals.Add((startZ, rect.maxZ));
                }
                for (var i = 0; i < zIntervals.Count; i++)
                {
                    var zInt = zIntervals[i];
                    for (var k = 0; k < xIntervals.Count; k++)
                    {
                        var xInt = xIntervals[k];
                        var subRect = CellRect.FromLimits(xInt.min, zInt.min, xInt.max, zInt.max);
                        if (subRect.Width >= 2 && subRect.Height >= 2)
                        {
                            list.Add(subRect);
                        }
                    }
                }
                list = list.OrderByDescending((CellRect r) => r.maxZ).ThenBy((CellRect r) => r.minX).ToList();
                var sb = new StringBuilder();
                for (var j = 0; j < list.Count; j++)
                {
                    var prefabDef = PrefabUtility.CreatePrefab(list[j], true, true);
                    var value = GeneratePrefabXml(prefabDef, $"{prefix}_{j + 1}", list[j], includeRoof);
                    sb.AppendLine(value);
                }
                GUIUtility.systemCopyBuffer = sb.ToString();
                Messages.Message($"Copied {list.Count} prefabs to clipboard.", MessageTypeDefOf.NeutralEvent, historical: false);
            }
        }
    }
}