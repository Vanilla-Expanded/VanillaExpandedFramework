// using System;
// using System.Collections.Generic;
// using Verse;
//
// namespace VEF.Maps;
//
// public class CustomListerThingGroupDef : Def
// {
//     // Worker class
//     public Type listerSelectorWorker = null;
//
//     // Used for specific defs worker
//     public List<ThingDef> defs;
//
//     // Used for comp workers targeting specific type
//     public Type specificType = null;
//     public bool allowSubtypes = true;
//
//     [Unsaved] private CustomGroupWorker worker;
//
//     public CustomGroupWorker Worker
//     {
//         get
//         {
//             if (worker == null)
//             {
//                 try
//                 {
//                     worker = (CustomGroupWorker)Activator.CreateInstance(listerSelectorWorker, this);
//                 }
//                 catch (Exception e)
//                 {
//                     worker = new CustomGroupWorker_Nothing(this);
//                     Log.Error($"Exception occured initializing worker for {nameof(CustomListerThingGroupDef)} with defName {defName}. Exception:\n{e}");
//                 }
//             }
//
//             return worker;
//         }
//     }
//
//     public abstract class CustomGroupWorker(CustomListerThingGroupDef def)
//     {
//         public readonly CustomListerThingGroupDef def = def;
//
//         public abstract bool Contains(ThingDef thing);
//     }
//
//     private class CustomGroupWorker_Nothing(CustomListerThingGroupDef def) : CustomGroupWorker(def)
//     {
//         public override bool Contains(ThingDef thing) => false;
//     }
//
//     public class CustomGroupWorker_SpecifiedDefs(CustomListerThingGroupDef def) : CustomGroupWorker(def)
//     {
//         public override bool Contains(ThingDef thing) => def.defs != null && def.defs.Contains(thing);
//     }
//
//     private class CustomGroupWorker_SpecificThing(CustomListerThingGroupDef def) : CustomGroupWorker(def)
//     {
//         public override bool Contains(ThingDef thing)
//         {
//             if (def.specificType == null)
//                 return false;
//             return def.specificType == thing.thingClass || (def.allowSubtypes && def.specificType.IsAssignableFrom(thing.thingClass));
//         }
//     }
//
//     private class CustomGroupWorker_SpecificComp(CustomListerThingGroupDef def) : CustomGroupWorker(def)
//     {
//         public override bool Contains(ThingDef thing)
//         {
//             if (def.specificType == null)
//                 return false;
//
//             for (var i = 0; i < thing.comps.Count; i++)
//             {
//                 var compClass = thing.comps[i].compClass;
//                 if (def.specificType == compClass || (def.allowSubtypes && def.specificType.IsAssignableFrom(compClass)))
//                     return true;
//             }
//
//             return false;
//         }
//     }
//
//     private class CustomGroupWorker_SpecificDefModExtension(CustomListerThingGroupDef def) : CustomGroupWorker(def)
//     {
//         public override bool Contains(ThingDef thing)
//         {
//             if (def.specificType == null)
//                 return false;
//
//             for (var i = 0; i < thing.modExtensions.Count; i++)
//             {
//                 var extensionClass = thing.modExtensions[i].GetType();
//                 if (def.specificType == extensionClass || (def.allowSubtypes && def.specificType.IsAssignableFrom(extensionClass)))
//                     return true;
//             }
//
//             return false;
//         }
//     }
// }