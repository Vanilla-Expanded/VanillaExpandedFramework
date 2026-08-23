using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using LudeonTK;
using RimWorld;
using Verse;

namespace VEF.Storyteller
{
    public static class PrefabReflection
    {
        private static readonly FieldInfo thingsField = AccessTools.Field(typeof(PrefabDef), "things");
        private static readonly FieldInfo terrainField = AccessTools.Field(typeof(PrefabDef), "terrain");
        private static readonly FieldInfo prefabsField = AccessTools.Field(typeof(PrefabDef), "prefabs");
        private static readonly FieldInfo bufferField = AccessTools.Field(typeof(DebugActionsPrefabs), "buffer");
        public static List<PrefabThingData> GetThingsList(this PrefabDef def) => (List<PrefabThingData>)thingsField.GetValue(def);
        public static List<PrefabTerrainData> GetTerrainList(this PrefabDef def) => (List<PrefabTerrainData>)terrainField.GetValue(def);
        public static List<SubPrefabData> GetSubPrefabsList(this PrefabDef def) => (List<SubPrefabData>)prefabsField.GetValue(def);
        public static void SetDebugBuffer(PrefabDef def) => bufferField.SetValue(null, def);
    }
}
