// using System.Collections.Generic;
// using Verse;
//
// namespace VEF.Maps;
//
// public class CustomListerThingsByGroup(Map map) : MapComponent(map)
// {
//     private static readonly DefMapArray<ThingDef, ushort> GroupsForDef = new(DefMapArray<ThingDef, ushort>.InitialArrayState.Null);
//     private static readonly List<Thing> EmptyList = [];
//
//     private readonly DefMap<CustomListerThingGroupDef, List<Thing>> listsByGroup = [];
//
//     public List<Thing> ThingsInGroup(CustomListerThingGroupDef group)
//     {
//         if (group == null)
//         {
//             Log.Error($"Attempting to access a group with null {nameof(CustomListerThingGroupDef)}.");
//             return EmptyList;
//         }
//
//         return listsByGroup[group.index] ?? EmptyList;
//     }
//
//     public static bool GroupIncludes(Thing thing, CustomListerThingGroupDef group)
//     {
//         return thing?.def != null && group.Worker.Contains(thing.def);
//     }
//
//     public void Add(Thing t)
//     {
//         if (t.def == null)
//             return;
//
//         var groups = GroupsForDef[t.def] ?? RecacheGroupsFor(t);
//         for (var i = 0; i < groups.Length; i++)
//         {
//             var list = listsByGroup[i];
//             if (list == null)
//                 listsByGroup[i] = list = [];
//             list.Add(t);
//         }
//     }
//
//     public void Remove(Thing t)
//     {
//         if (t.def == null)
//             return;
//
//         var groups = GroupsForDef[t.def] ?? RecacheGroupsFor(t);
//         for (var i = 0; i < groups.Length; i++)
//         {
//             var list = listsByGroup[i];
//             if (list == null)
//                 listsByGroup[i] = list = [];
//             list.Remove(t);
//         }
//     }
//
//     public void Clear()
//     {
//         for (var i = 0; i < listsByGroup.Count; i++)
//         {
//             if (listsByGroup[i] != null)
//                 listsByGroup[i].Clear();
//         }
//     }
//
//     private static ushort[] RecacheGroupsFor(Thing t)
//     {
//         var list = new List<ushort>();
//
//         for (ushort i = 0; i < DefDatabase<CustomListerThingGroupDef>.DefCount; i++)
//         {
//             if (GroupIncludes(t, DefDatabase<CustomListerThingGroupDef>.AllDefsListForReading[i]))
//                 list.Add(i);
//         }
//
//         var array = GroupsForDef[t.def] = list.NullOrEmpty() ? [] : list.ToArray();
//         return array;
//     }
// }