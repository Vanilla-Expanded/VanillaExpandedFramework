using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ItemProcessor
{
    public class ItemAcceptedDef : Def
    {


        //ItemAcceptedDef is a simple custom def that assigns products to buildings, and slots.
        //Ingredient insertion code reads from these defs, so if an ingredient isn't there, it won't appear on any item processor


        //defName of the building accepting these ingredients
        public string building;
        //Which slot of the building will these ingredients be inserted on?
        public int slot = 1;
        //A list of ingredient defNames
        public List<string> items;
    }
}
