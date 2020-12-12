using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace VanillaFurnitureExpanded
{
    public class ConfigurableSpawnerDef : Def
    {

        //ConfigurableSpawnerDef defines a list of items that can be selected in a configurable spawner building.

        public List<string> items;

        public string listName;

        public string building;

        public string GizmoIcon = "";

        public string GizmoLabel = "";

        public string GizmoDescription = "";

        public int timeInTicks = 1000;

    }
}
