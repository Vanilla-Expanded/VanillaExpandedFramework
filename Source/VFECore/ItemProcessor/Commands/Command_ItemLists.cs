using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse.AI;
using Verse;
using System.Linq;


namespace ItemProcessor
{
    [StaticConstructorOnStartup]
    public class Command_SetOutputList : Command
    {

        public Map map;
        public Building_ItemProcessor building;
        public List<Thing> things;



        public Command_SetOutputList()
        {
            //Loops through all things selected right now to see if it finds an item processor building
            //Just in case, item processors disable its Gizmos (buttons) if there are more things selected
            foreach (object obj in Find.Selector.SelectedObjects)
            {
                building = obj as Building_ItemProcessor;
                if (building != null)
                {
                    if (building.firstItem == "")
                    {
                        //If a first ingredient hasn't been chosen yet, show an empty "choose ingredient" button. This is configurable in each building
                        icon = ContentFinder<Texture2D>.Get(building.compItemProcessor.Props.chooseIngredientsIcon, true);
                        defaultLabel = "IP_ChooseOutput".Translate();

                    }
                    foreach (CombinationDef element in DefDatabase<CombinationDef>.AllDefs.Where(element => (element.building == building.def.defName && building.firstItem == element.items[0])))
                    {
                        //If an output has been chosen, then show the graphic of that ingredient 

                        if (building.productToTurnInto == element.result)
                        {
                            if (element.resultUsesSpecialIcon)
                            {
                                icon = ContentFinder<Texture2D>.Get(element.resultSpecialIcon, false);
                            }
                            else if (ThingDef.Named(element.result).graphicData.graphicClass == typeof(Graphic_StackCount))
                            {
                                icon = ContentFinder<Texture2D>.Get(ThingDef.Named(element.result).graphic.path + "/" + ThingDef.Named(element.result).defName + "_b", false);
                                if (icon == null)
                                {
                                    icon = ContentFinder<Texture2D>.Get(ThingDef.Named(element.result).graphic.path + "/" + ThingDef.Named(element.result).defName, false);
                                }
                                if (icon == null)
                                {
                                    icon = ContentFinder<Texture2D>.Get(ThingDef.Named(element.result).graphic.path, false);
                                }
                            }
                            else
                            {
                               if (ThingDef.Named(element.result).graphic.path != null)
                                {
                                    icon = ContentFinder<Texture2D>.Get(ThingDef.Named(element.result).graphic.path, false);
                                } else icon = ContentFinder<Texture2D>.Get(ThingDef.Named(element.result).graphicData.texPath, false);


                            }

                            defaultLabel = "IP_Output".Translate(ThingDef.Named(element.result).LabelCap);
                            if (element.isCategoryRecipe)
                            {
                                string categorytwoLabel = "";
                                string categorythreeLabel = "";
                                string categoryfourLabel = "";
                                if (element.secondItems != null)
                                {
                                    ThingCategoryDef categorytwo = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(element.secondItems[0]);
                                    if (categorytwo != null)
                                    {
                                        categorytwoLabel = ", "+categorytwo.LabelCap;
                                    }
                                }
                                if (element.thirdItems != null)
                                {
                                    ThingCategoryDef categorythree = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(element.thirdItems[0]);
                                    if (categorythree != null)
                                    {
                                        categorythreeLabel = ", " + categorythree.LabelCap;
                                    }
                                }
                                if (element.fourthItems != null)
                                {
                                    ThingCategoryDef categoryfour = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(element.fourthItems[0]);
                                    if (categoryfour != null)
                                    {
                                        categoryfourLabel = ", " + categoryfour.LabelCap;
                                    }
                                }
                                defaultDesc = "IP_OutputDesc".Translate(ThingDef.Named(element.result).LabelCap, DefDatabase<ThingCategoryDef>.GetNamedSilentFail(building.firstItem).LabelCap
                                    +  categorytwoLabel + categorythreeLabel +  categoryfourLabel);
                            }
                            else
                            {
                                string itemtwoLabel = "";
                                string itemthreeLabel = "";
                                string itemfourLabel = "";
                                if (element.secondItems != null)
                                {
                                    ThingDef itemtwo = DefDatabase<ThingDef>.GetNamedSilentFail(element.secondItems[0]);
                                    if (itemtwo != null)
                                    {
                                        itemtwoLabel = ", " + itemtwo.LabelCap;
                                    }
                                }
                                if (element.thirdItems != null)
                                {
                                    ThingDef itemthree = DefDatabase<ThingDef>.GetNamedSilentFail(element.thirdItems[0]);
                                    if (itemthree != null)
                                    {
                                        itemthreeLabel = ", " + itemthree.LabelCap;
                                    }
                                }
                                if (element.fourthItems != null)
                                {
                                    ThingDef itemfour = DefDatabase<ThingDef>.GetNamedSilentFail(element.fourthItems[0]);
                                    if (itemfour != null)
                                    {
                                        itemfourLabel = ", " + itemfour.LabelCap;
                                    }
                                }
                                defaultDesc = "IP_OutputDesc".Translate(ThingDef.Named(element.result).LabelCap, DefDatabase<ThingDef>.GetNamedSilentFail(building.firstItem).LabelCap
                                    +  itemtwoLabel + itemthreeLabel + itemfourLabel);
                            }

                        }
                    }
                }
            }
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            List<FloatMenuOption> list = new List<FloatMenuOption>();


            //For the rest, 
            foreach (CombinationDef element in DefDatabase<CombinationDef>.AllDefs.Where(element => (element.building == building.def.defName)))
            {

                if (element.isCategoryRecipe)
                {
                    string categorytwoLabel = "";
                    string categorythreeLabel = "";
                    string categoryfourLabel = "";
                    if (element.secondItems != null)
                    {
                        ThingCategoryDef categorytwo = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(element.secondItems[0]);
                        if (categorytwo != null)
                        {
                            categorytwoLabel = ", " + categorytwo.LabelCap;
                        }
                    }
                    if (element.thirdItems != null)
                    {
                        ThingCategoryDef categorythree = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(element.thirdItems[0]);
                        if (categorythree != null)
                        {
                            categorythreeLabel = ", " + categorythree.LabelCap;
                        }
                    }
                    if (element.fourthItems != null)
                    {
                        ThingCategoryDef categoryfour = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(element.fourthItems[0]);
                        if (categoryfour != null)
                        {
                            categoryfourLabel = ", " + categoryfour.LabelCap;
                        }
                    }
                    list.Add(new FloatMenuOption("IP_OutputVariable".Translate(ThingDef.Named(element.result).LabelCap, DefDatabase<ThingCategoryDef>.GetNamedSilentFail(element.items[0]).LabelCap +
                             categorytwoLabel  + categorythreeLabel + categoryfourLabel), delegate
                            {
                                if (building.processorStage <= ProcessorStage.ExpectingIngredients) { this.TryConfigureIngredientsByOutput(element); }
                            }, MenuOptionPriority.Default, null, null, 29f, null, null));
                }
                else
                {
                    string itemtwoLabel = "";
                    string itemthreeLabel = "";
                    string itemfourLabel = "";
                    if (element.secondItems != null)
                    {
                        ThingDef itemtwo = DefDatabase<ThingDef>.GetNamedSilentFail(element.secondItems[0]);
                        if (itemtwo != null)
                        {
                            itemtwoLabel = ", " + itemtwo.LabelCap;
                        }
                    }
                    if (element.thirdItems != null)
                    {
                        ThingDef itemthree = DefDatabase<ThingDef>.GetNamedSilentFail(element.thirdItems[0]);
                        if (itemthree != null)
                        {
                            itemthreeLabel = ", " + itemthree.LabelCap;
                        }
                    }
                    if (element.fourthItems != null)
                    {
                        ThingDef itemfour = DefDatabase<ThingDef>.GetNamedSilentFail(element.fourthItems[0]);
                        if (itemfour != null)
                        {
                            itemfourLabel = ", " + itemfour.LabelCap;
                        }
                    }
                    list.Add(new FloatMenuOption("IP_OutputVariable".Translate(ThingDef.Named(element.result).LabelCap, DefDatabase<ThingDef>.GetNamedSilentFail(element.items[0]).LabelCap +
                          itemtwoLabel + itemthreeLabel + itemfourLabel), delegate
                         {
                             if (building.processorStage <= ProcessorStage.ExpectingIngredients) { this.TryConfigureIngredientsByOutput(element); }
                         }, MenuOptionPriority.Default, null, null, 29f, null, null));
                }

            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        private void TryConfigureIngredientsByOutput(CombinationDef element)
        {
            building.productToTurnInto = element.result;
            building.thisRecipe = element.defName;
            
            if (element.isCategoryRecipe)
            {
                switch (building.compItemProcessor.Props.numberOfInputs)
                {
                    case 1:
                        building.firstCategory = ThingCategoryDef.Named(element.items[0]).defName;
                        building.firstItem = building.firstCategory;
                        building.ExpectedAmountFirstIngredient = element.amount[0];
                        break;
                    case 2:
                        building.firstCategory = ThingCategoryDef.Named(element.items[0]).defName;
                        building.firstItem = building.firstCategory;
                        building.secondCategory = ThingCategoryDef.Named(element.secondItems[0]).defName;
                        building.secondItem = building.secondCategory;
                        building.ExpectedAmountFirstIngredient = element.amount[0];
                        building.ExpectedAmountSecondIngredient = element.amount[1];
                        break;
                    case 3:
                        building.firstCategory = ThingCategoryDef.Named(element.items[0]).defName;
                        building.firstItem = building.firstCategory;
                        building.secondCategory = ThingCategoryDef.Named(element.secondItems[0]).defName;
                        building.secondItem = building.secondCategory;
                        building.thirdCategory = ThingCategoryDef.Named(element.thirdItems[0]).defName;
                        building.thirdItem = building.thirdCategory;
                        building.ExpectedAmountFirstIngredient = element.amount[0];
                        building.ExpectedAmountSecondIngredient = element.amount[1];
                        building.ExpectedAmountThirdIngredient = element.amount[2];
                        break;
                    case 4:
                        building.firstCategory = ThingCategoryDef.Named(element.items[0]).defName;
                        building.firstItem = building.firstCategory;
                        building.secondCategory = ThingCategoryDef.Named(element.secondItems[0]).defName;
                        building.secondItem = building.secondCategory;
                        building.thirdCategory = ThingCategoryDef.Named(element.thirdItems[0]).defName;
                        building.thirdItem = building.thirdCategory;
                        building.fourthCategory = ThingCategoryDef.Named(element.fourthItems[0]).defName;
                        building.fourthItem = building.fourthCategory;
                        building.ExpectedAmountFirstIngredient = element.amount[0];
                        building.ExpectedAmountSecondIngredient = element.amount[1];
                        building.ExpectedAmountThirdIngredient = element.amount[2];
                        building.ExpectedAmountFourthIngredient = element.amount[3];
                        break;
                    default:
                        building.firstCategory = ThingCategoryDef.Named(element.items[0]).defName;
                        building.firstItem = building.firstCategory;
                        building.ExpectedAmountFirstIngredient = element.amount[0];
                        break;
                }

            }
            else
            {
                switch (building.compItemProcessor.Props.numberOfInputs)
                {
                    case 1:
                        building.firstItem = ThingDef.Named(element.items[0]).defName;
                        building.ExpectedAmountFirstIngredient = element.amount[0];
                        break;
                    case 2:
                        building.firstItem = ThingDef.Named(element.items[0]).defName;
                        building.secondItem = ThingDef.Named(element.secondItems[0]).defName;
                        building.ExpectedAmountFirstIngredient = element.amount[0];
                        building.ExpectedAmountSecondIngredient = element.amount[1];
                        break;
                    case 3:
                        building.firstItem = ThingDef.Named(element.items[0]).defName;
                        building.secondItem = ThingDef.Named(element.secondItems[0]).defName;
                        building.thirdItem = ThingDef.Named(element.thirdItems[0]).defName;
                        building.ExpectedAmountFirstIngredient = element.amount[0];
                        building.ExpectedAmountSecondIngredient = element.amount[1];
                        building.ExpectedAmountThirdIngredient = element.amount[2];                        
                        break;
                    case 4:
                        building.firstItem = ThingDef.Named(element.items[0]).defName;
                        building.secondItem = ThingDef.Named(element.secondItems[0]).defName;
                        building.thirdItem = ThingDef.Named(element.thirdItems[0]).defName;
                        building.fourthItem = ThingDef.Named(element.fourthItems[0]).defName;
                        building.ExpectedAmountFirstIngredient = element.amount[0];
                        building.ExpectedAmountSecondIngredient = element.amount[1];
                        building.ExpectedAmountThirdIngredient = element.amount[2];
                        building.ExpectedAmountFourthIngredient = element.amount[3];
                        break;
                    default:
                        building.firstItem = ThingDef.Named(element.items[0]).defName;
                        building.ExpectedAmountFirstIngredient = element.amount[0];

                        break;
                }
            }

            if (building.compItemProcessor.Props.isSemiAutomaticMachine)
            {
                if (building.compPowerTrader != null && !building.compPowerTrader.PowerOn && building.compItemProcessor.Props.noPowerDestroysProgress)
                {
                    Messages.Message("IP_NoPowerDestroysWarning".Translate(building.def.LabelCap), building, MessageTypeDefOf.NegativeEvent, true);
                }
                else if (building.compFuelable != null && !building.compFuelable.HasFuel && building.compItemProcessor.Props.noPowerDestroysProgress)
                {
                    Messages.Message("IP_NoFuelDestroysWarning".Translate(building.def.LabelCap), building, MessageTypeDefOf.NegativeEvent, true);
                }
                else
                {
                    building.IngredientsChosenBringThemIn();
                }

            }
            else
            {
                building.processorStage = ProcessorStage.IngredientsChosen;
            }
        }
    }


    [StaticConstructorOnStartup]
    public class Command_SetFirstItemList : Command
    {

        public Map map;
        public Building_ItemProcessor building;
        public List<Thing> things;



        public Command_SetFirstItemList()
        {
            //Loops through all things selected right now to see if it finds an item processor building
            //Just in case, item processors disable its Gizmos (buttons) if there are more things selected
            foreach (object obj in Find.Selector.SelectedObjects)
            {
                building = obj as Building_ItemProcessor;
                if (building != null)
                {
                    if (building.firstItem == "")
                    {
                        //If a first ingredient hasn't been chosen yet, show an empty "choose ingredient" button. This is configurable in each building
                        icon = ContentFinder<Texture2D>.Get(building.compItemProcessor.Props.chooseIngredientsIcon, true);
                        defaultLabel = "IP_ChooseIngredient".Translate();
                    }
                    foreach (ItemAcceptedDef element in DefDatabase<ItemAcceptedDef>.AllDefs.Where(element => (element.building == building.def.defName) && (element.slot == 1)))
                    {
                        foreach (string item in element.items)
                        {
                            //If a first ingredient has been chosen, then show the graphic of that ingredient 

                            if (building.firstItem == item)
                            {
                                if (building.compItemProcessor.Props.isCategoryBuilding)
                                {
                                    icon = ContentFinder<Texture2D>.Get(ThingCategoryDef.Named(item).iconPath, true);
                                    defaultLabel = "IP_InsertVariable".Translate(ThingCategoryDef.Named(item).LabelCap);
                                }
                                else
                                {
                                    if (ThingDef.Named(item).graphicData.graphicClass == typeof(Graphic_StackCount))
                                    {
                                        icon = ContentFinder<Texture2D>.Get(ThingDef.Named(item).graphic.path + "/" + ThingDef.Named(item).defName + "_b", false);
                                        if (icon == null)
                                        {
                                            icon = ContentFinder<Texture2D>.Get(ThingDef.Named(item).graphic.path + "/" + ThingDef.Named(item).defName, false);
                                        }

                                    }
                                    else
                                    {
                                        icon = ContentFinder<Texture2D>.Get(ThingDef.Named(item).graphic.path, false);
                                    }

                                    defaultLabel = "IP_InsertVariable".Translate(ThingDef.Named(item).LabelCap);
                                }


                            }
                        }
                    }
                }
            }
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            //This is for 2-3 slot recipes that allow one of the ingredients to be empty
            if (building.GetComp<CompItemProcessor>().Props.acceptsNoneAsInput)
            {
                list.Add(new FloatMenuOption("IP_None".Translate(), delegate
                {
                    building.firstItem = "None";
                }, MenuOptionPriority.Default, null, null, 29f, null, null));
            }

            //For the rest, search for every recipe that is from this building, and this slot
            foreach (ItemAcceptedDef element in DefDatabase<ItemAcceptedDef>.AllDefs.Where(element => (element.building == building.def.defName) && (element.slot == 1)))
            {
                foreach (string item in element.items.Where(item => item != "None"))
                {
                    if (building.compItemProcessor.Props.isCategoryBuilding)
                    {
                        list.Add(new FloatMenuOption("IP_InsertVariable".Translate(ThingCategoryDef.Named(item).LabelCap), delegate
                        {
                            if (building.processorStage <= ProcessorStage.ExpectingIngredients) { this.TryInsertFirstThing(item); }
                        }, MenuOptionPriority.Default, null, null, 29f, null, null));

                    }
                    else
                    {
                        list.Add(new FloatMenuOption("IP_InsertVariable".Translate(ThingDef.Named(item).LabelCap), delegate
                        {
                            things = map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed(item, true));
                            if (things.Count > 0)
                            {
                                if (building.processorStage <= ProcessorStage.ExpectingIngredients) { this.TryInsertFirstThing(); }

                            }
                            else
                            {
                                Messages.Message("IP_CantFindThing".Translate(ThingDef.Named(item).LabelCap), null, MessageTypeDefOf.NegativeEvent, true);
                            }

                        }, MenuOptionPriority.Default, null, null, 29f, null, null));
                    }



                }
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        private void TryInsertFirstThing(string item = "")
        {
            if (item != "")
            {
                building.firstCategory = ThingCategoryDef.Named(item).defName;
                building.firstItem = building.firstCategory;
            }
            else
            {
                building.firstItem = things.RandomElement().def.defName;
            }
            if (building.compItemProcessor.Props.isSemiAutomaticMachine)
            {
                if (building.compPowerTrader != null && !building.compPowerTrader.PowerOn && building.compItemProcessor.Props.noPowerDestroysProgress)
                {
                    Messages.Message("IP_NoPowerDestroysWarning".Translate(building.def.LabelCap), building, MessageTypeDefOf.NegativeEvent, true);
                }
                else if (building.compFuelable != null && !building.compFuelable.HasFuel && building.compItemProcessor.Props.noPowerDestroysProgress)
                {
                    Messages.Message("IP_NoFuelDestroysWarning".Translate(building.def.LabelCap), building, MessageTypeDefOf.NegativeEvent, true);
                }
                else
                {
                    building.IngredientsChosenBringThemIn();
                }

            }
            else
            {
                building.processorStage = ProcessorStage.IngredientsChosen;

            }



        }




    }

    [StaticConstructorOnStartup]
    public class Command_SetSecondItemList : Command
    {

        public Map map;
        public Building_ItemProcessor building;
        public List<Thing> things;



        public Command_SetSecondItemList()
        {
            foreach (object obj in Find.Selector.SelectedObjects)
            {
                building = obj as Building_ItemProcessor;
                if (building != null)
                {
                    if (building.secondItem == "")
                    {
                        icon = ContentFinder<Texture2D>.Get(building.compItemProcessor.Props.chooseIngredientsIcon, true);
                        defaultLabel = "IP_ChooseIngredientSecond".Translate();
                    }
                    foreach (ItemAcceptedDef element in DefDatabase<ItemAcceptedDef>.AllDefs.Where(element => (element.building == building.def.defName) && (element.slot == 2)))
                    {
                        foreach (string item in element.items)
                        {
                            if (building.secondItem == item)
                            {
                                if (building.compItemProcessor.Props.isCategoryBuilding)
                                {
                                    icon = ContentFinder<Texture2D>.Get(ThingCategoryDef.Named(item).iconPath, true);
                                    defaultLabel = "IP_InsertVariable".Translate(ThingCategoryDef.Named(item).LabelCap);
                                }
                                else
                                {

                                    if (ThingDef.Named(item).graphicData.graphicClass == typeof(Graphic_StackCount))
                                    {
                                        icon = ContentFinder<Texture2D>.Get(ThingDef.Named(item).graphic.path + "/" + ThingDef.Named(item).defName + "_b", false);
                                        if (icon == null)
                                        {
                                            icon = ContentFinder<Texture2D>.Get(ThingDef.Named(item).graphic.path + "/" + ThingDef.Named(item).defName, false);
                                        }

                                    }
                                    else
                                    {
                                        icon = ContentFinder<Texture2D>.Get(ThingDef.Named(item).graphic.path, false);
                                    }
                                }


                            }
                        }
                    }
                }
            }
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            if (building.GetComp<CompItemProcessor>().Props.acceptsNoneAsInput)
            {
                list.Add(new FloatMenuOption("IP_None".Translate(), delegate
                {
                    building.secondItem = "None";
                }, MenuOptionPriority.Default, null, null, 29f, null, null));
            }

            foreach (ItemAcceptedDef element in DefDatabase<ItemAcceptedDef>.AllDefs.Where(element => (element.building == building.def.defName) && (element.slot == 2)))
            {
                foreach (string item in element.items.Where(item => item != "None"))
                {
                    if (building.compItemProcessor.Props.isCategoryBuilding)
                    {
                        list.Add(new FloatMenuOption("IP_InsertVariable".Translate(ThingCategoryDef.Named(item).LabelCap), delegate
                        {
                            if (building.processorStage <= ProcessorStage.ExpectingIngredients) { this.TryInsertSecondThing(item); }
                        }, MenuOptionPriority.Default, null, null, 29f, null, null));

                    }
                    else
                    {
                        list.Add(new FloatMenuOption("IP_InsertVariable".Translate(ThingDef.Named(item).LabelCap), delegate
                        {
                            things = map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed(item, true));
                            if (things.Count > 0)
                            {

                                if (building.processorStage <= ProcessorStage.ExpectingIngredients) { this.TryInsertSecondThing(); }
                            }
                            else
                            {
                                Messages.Message("IP_CantFindThing".Translate(ThingDef.Named(item).LabelCap), null, MessageTypeDefOf.NegativeEvent, true);
                            }

                        }, MenuOptionPriority.Default, null, null, 29f, null, null));
                    }

                }
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        private void TryInsertSecondThing(string item = "")
        {
            building.processorStage = ProcessorStage.IngredientsChosen;
            if (item != "")
            {
                building.secondCategory = ThingCategoryDef.Named(item).defName;
                building.secondItem = building.secondCategory;
            }
            else
            {
                building.secondItem = things.RandomElement().def.defName;
            }

        }




    }

    [StaticConstructorOnStartup]
    public class Command_SetThirdItemList : Command
    {

        public Map map;
        public Building_ItemProcessor building;
        public List<Thing> things;



        public Command_SetThirdItemList()
        {
            foreach (object obj in Find.Selector.SelectedObjects)
            {
                building = obj as Building_ItemProcessor;
                if (building != null)
                {
                    if (building.thirdItem == "")
                    {
                        icon = ContentFinder<Texture2D>.Get(building.compItemProcessor.Props.chooseIngredientsIcon, true);
                        defaultLabel = "IP_ChooseIngredientThird".Translate();
                    }
                    foreach (ItemAcceptedDef element in DefDatabase<ItemAcceptedDef>.AllDefs.Where(element => (element.building == building.def.defName) && (element.slot == 3)))
                    {
                        foreach (string item in element.items)
                        {
                            if (building.thirdItem == item)
                            {
                                if (building.compItemProcessor.Props.isCategoryBuilding)
                                {
                                    icon = ContentFinder<Texture2D>.Get(ThingCategoryDef.Named(item).iconPath, true);
                                    defaultLabel = "IP_InsertVariable".Translate(ThingCategoryDef.Named(item).LabelCap);
                                }
                                else
                                {
                                    if (ThingDef.Named(item).graphicData.graphicClass == typeof(Graphic_StackCount))
                                    {
                                        icon = ContentFinder<Texture2D>.Get(ThingDef.Named(item).graphic.path + "/" + ThingDef.Named(item).defName + "_b", false);
                                        if (icon == null)
                                        {
                                            icon = ContentFinder<Texture2D>.Get(ThingDef.Named(item).graphic.path + "/" + ThingDef.Named(item).defName, false);
                                        }

                                    }
                                    else
                                    {
                                        icon = ContentFinder<Texture2D>.Get(ThingDef.Named(item).graphic.path, false);
                                    }
                                }

                            }
                        }
                    }
                }
            }
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            if (building.GetComp<CompItemProcessor>().Props.acceptsNoneAsInput)
            {
                list.Add(new FloatMenuOption("IP_None".Translate(), delegate
                {
                    building.thirdItem = "None";
                }, MenuOptionPriority.Default, null, null, 29f, null, null));
            }

            foreach (ItemAcceptedDef element in DefDatabase<ItemAcceptedDef>.AllDefs.Where(element => (element.building == building.def.defName) && (element.slot == 3)))
            {
                foreach (string item in element.items.Where(item => item != "None"))
                {
                    if (building.compItemProcessor.Props.isCategoryBuilding)
                    {
                        list.Add(new FloatMenuOption("IP_InsertVariable".Translate(ThingCategoryDef.Named(item).LabelCap), delegate
                        {
                            if (building.processorStage <= ProcessorStage.ExpectingIngredients) { this.TryInsertThirdThing(item); }
                        }, MenuOptionPriority.Default, null, null, 29f, null, null));

                    }
                    else
                    {
                        list.Add(new FloatMenuOption("IP_InsertVariable".Translate(ThingDef.Named(item).LabelCap), delegate
                        {
                            things = map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed(item, true));
                            if (things.Count > 0)
                            {

                                if (building.processorStage <= ProcessorStage.ExpectingIngredients) { this.TryInsertThirdThing(); }
                            }
                            else
                            {
                                Messages.Message("IP_CantFindThing".Translate(ThingDef.Named(item).LabelCap), null, MessageTypeDefOf.NegativeEvent, true);
                            }

                        }, MenuOptionPriority.Default, null, null, 29f, null, null));
                    }
                }
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        private void TryInsertThirdThing(string item = "")
        {
            building.processorStage = ProcessorStage.IngredientsChosen;
            if (item != "")
            {
                building.thirdCategory = ThingCategoryDef.Named(item).defName;
                building.thirdItem = building.thirdCategory;
            }
            else
            {
                building.thirdItem = things.RandomElement().def.defName;
            }


        }




    }

    [StaticConstructorOnStartup]
    public class Command_SetFourthItemList : Command
    {

        public Map map;
        public Building_ItemProcessor building;
        public List<Thing> things;



        public Command_SetFourthItemList()
        {
            foreach (object obj in Find.Selector.SelectedObjects)
            {
                building = obj as Building_ItemProcessor;
                if (building != null)
                {
                    if (building.fourthItem == "")
                    {
                        icon = ContentFinder<Texture2D>.Get(building.compItemProcessor.Props.chooseIngredientsIcon, true);
                        defaultLabel = "IP_ChooseIngredientFourth".Translate();
                    }
                    foreach (ItemAcceptedDef element in DefDatabase<ItemAcceptedDef>.AllDefs.Where(element => (element.building == building.def.defName) && (element.slot == 4)))
                    {
                        foreach (string item in element.items)
                        {
                            if (building.fourthItem == item)
                            {
                                if (building.compItemProcessor.Props.isCategoryBuilding)
                                {
                                    icon = ContentFinder<Texture2D>.Get(ThingCategoryDef.Named(item).iconPath, true);
                                    defaultLabel = "IP_InsertVariable".Translate(ThingCategoryDef.Named(item).LabelCap);
                                }
                                else
                                {
                                    if (ThingDef.Named(item).graphicData.graphicClass == typeof(Graphic_StackCount))
                                    {
                                        icon = ContentFinder<Texture2D>.Get(ThingDef.Named(item).graphic.path + "/" + ThingDef.Named(item).defName + "_b", false);
                                        if (icon == null)
                                        {
                                            icon = ContentFinder<Texture2D>.Get(ThingDef.Named(item).graphic.path + "/" + ThingDef.Named(item).defName, false);
                                        }

                                    }
                                    else
                                    {
                                        icon = ContentFinder<Texture2D>.Get(ThingDef.Named(item).graphic.path, false);
                                    }
                                }

                            }
                        }
                    }
                }
            }
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            if (building.GetComp<CompItemProcessor>().Props.acceptsNoneAsInput)
            {
                list.Add(new FloatMenuOption("IP_None".Translate(), delegate
                {
                    building.fourthItem = "None";
                }, MenuOptionPriority.Default, null, null, 29f, null, null));
            }

            foreach (ItemAcceptedDef element in DefDatabase<ItemAcceptedDef>.AllDefs.Where(element => (element.building == building.def.defName) && (element.slot == 4)))
            {
                foreach (string item in element.items.Where(item => item != "None"))
                {
                    if (building.compItemProcessor.Props.isCategoryBuilding)
                    {
                        list.Add(new FloatMenuOption("IP_InsertVariable".Translate(ThingCategoryDef.Named(item).LabelCap), delegate
                        {
                            if (building.processorStage <= ProcessorStage.ExpectingIngredients) { this.TryInsertFourthThing(item); }
                        }, MenuOptionPriority.Default, null, null, 29f, null, null));

                    }
                    else
                    {
                        list.Add(new FloatMenuOption("IP_InsertVariable".Translate(ThingDef.Named(item).LabelCap), delegate
                        {
                            things = map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed(item, true));
                            if (things.Count > 0)
                            {

                                if (building.processorStage <= ProcessorStage.ExpectingIngredients) { this.TryInsertFourthThing(); }
                            }
                            else
                            {
                                Messages.Message("IP_CantFindThing".Translate(ThingDef.Named(item).LabelCap), null, MessageTypeDefOf.NegativeEvent, true);
                            }

                        }, MenuOptionPriority.Default, null, null, 29f, null, null));
                    }
                }
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        private void TryInsertFourthThing(string item = "")
        {
            building.processorStage = ProcessorStage.IngredientsChosen;
            if (item != "")
            {
                building.fourthCategory = ThingCategoryDef.Named(item).defName;
                building.fourthItem = building.fourthCategory;
            }
            else
            {
                building.fourthItem = things.RandomElement().def.defName;
            }


        }




    }

    [StaticConstructorOnStartup]
    public class Command_SetQualityList : Command
    {

        public Map map;
        public Building_ItemProcessor building;

        public Command_SetQualityList()
        {
            foreach (object obj in Find.Selector.SelectedObjects)
            {
                building = obj as Building_ItemProcessor;
                if (building != null)
                {
                    if (!building.qualityEstablished)
                    {
                        icon = ContentFinder<Texture2D>.Get("UI/QualitySelectors/IP_QualityAuto", true);
                        defaultLabel = "IP_ChooseQualityAuto".Translate();
                    }


                    else
                    {

                        switch (building.qualityRequested)
                        {
                            case QualityCategory.Awful:
                                icon = ContentFinder<Texture2D>.Get("UI/QualitySelectors/IP_QualityAwful", true);
                                defaultLabel = "IP_QualityAutoIs".Translate(QualityCategory.Awful.ToString());
                                break;
                            case QualityCategory.Poor:
                                icon = ContentFinder<Texture2D>.Get("UI/QualitySelectors/IP_QualityPoor", true);
                                defaultLabel = "IP_QualityAutoIs".Translate(QualityCategory.Poor.ToString());
                                break;
                            case QualityCategory.Normal:
                                icon = ContentFinder<Texture2D>.Get("UI/QualitySelectors/IP_QualityNormal", true);
                                defaultLabel = "IP_QualityAutoIs".Translate(QualityCategory.Normal.ToString());
                                break;
                            case QualityCategory.Good:
                                icon = ContentFinder<Texture2D>.Get("UI/QualitySelectors/IP_QualityGood", true);
                                defaultLabel = "IP_QualityAutoIs".Translate(QualityCategory.Good.ToString());
                                break;
                            case QualityCategory.Excellent:
                                icon = ContentFinder<Texture2D>.Get("UI/QualitySelectors/IP_QualityExcellent", true);
                                defaultLabel = "IP_QualityAutoIs".Translate(QualityCategory.Excellent.ToString());
                                break;
                            case QualityCategory.Masterwork:
                                icon = ContentFinder<Texture2D>.Get("UI/QualitySelectors/IP_QualityMasterwork", true);
                                defaultLabel = "IP_QualityAutoIs".Translate(QualityCategory.Masterwork.ToString());
                                break;
                            case QualityCategory.Legendary:
                                icon = ContentFinder<Texture2D>.Get("UI/QualitySelectors/IP_QualityLegendary", true);
                                defaultLabel = "IP_QualityAutoIs".Translate(QualityCategory.Legendary.ToString());
                                break;
                        }

                    }

                }
            }
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            List<FloatMenuOption> list = new List<FloatMenuOption>();




            list.Add(new FloatMenuOption("IP_QualityAutoIs".Translate(QualityCategory.Awful.ToString()), delegate
            {
                building.qualityRequested = QualityCategory.Awful;
                building.qualityEstablished = true;
            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            list.Add(new FloatMenuOption("IP_QualityAutoIs".Translate(QualityCategory.Poor.ToString()), delegate
            {
                building.qualityRequested = QualityCategory.Poor;
                building.qualityEstablished = true;
            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            list.Add(new FloatMenuOption("IP_QualityAutoIs".Translate(QualityCategory.Normal.ToString()), delegate
            {
                building.qualityRequested = QualityCategory.Normal;
                building.qualityEstablished = true;
            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            list.Add(new FloatMenuOption("IP_QualityAutoIs".Translate(QualityCategory.Good.ToString()), delegate
            {
                building.qualityRequested = QualityCategory.Good;
                building.qualityEstablished = true;
            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            list.Add(new FloatMenuOption("IP_QualityAutoIs".Translate(QualityCategory.Excellent.ToString()), delegate
            {
                building.qualityRequested = QualityCategory.Excellent;
                building.qualityEstablished = true;
            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            list.Add(new FloatMenuOption("IP_QualityAutoIs".Translate(QualityCategory.Masterwork.ToString()), delegate
            {
                building.qualityRequested = QualityCategory.Masterwork;
                building.qualityEstablished = true;
            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            list.Add(new FloatMenuOption("IP_QualityAutoIs".Translate(QualityCategory.Legendary.ToString()), delegate
            {
                building.qualityRequested = QualityCategory.Legendary;
                building.qualityEstablished = true;
            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            list.Add(new FloatMenuOption("IP_QualityAutoNot".Translate(), delegate
            {
                building.qualityEstablished = false;
            }, MenuOptionPriority.Default, null, null, 29f, null, null));



            Find.WindowStack.Add(new FloatMenu(list));
        }






    }




}

