using BepInEx;
using BepInEx.Bootstrap;
using R2API;
using RoR2;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using RoR2.ContentManagement;
using RoR2.ExpansionManagement;
using System.IO;



namespace AspectRecipes {
    [BepInDependency(R2API.R2API.PluginGUID)]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    
    public class AspectRecipes : BaseUnityPlugin {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "liquedator";
        public const string PluginName = "AspectRecipes";
        public const string PluginVersion = "1.0.4";

        //number of new entries
        private const int numCraftables = 11;

        //The Awake() method is run at the very start when the game is initialized.
        public void Awake() {
            //load recipes
            createBlankCraftables();
            new myContentPack().Initialise();
            PickupCatalog.availability.CallWhenAvailable(DefineRecipes);

            //add descriptions for aspects
            PickupCatalog.availability.CallWhenAvailable(addLogbookDescriptions);

            //add logbook entries for aspects
            On.RoR2.UI.LogBook.LogBookController.BuildPickupEntries += addLogbookEntries;   
            //move aspects to the end
            On.RoR2.UI.LogBook.LogBookController.BuildPickupEntries += moveAspectsToEnd;
 
            Debug.Log("Adding unused aspects to logbook");
        }
        
        private void DefineRecipes() {
            //ifrits distinction
            fillCraftable(myContentPack.Craftables[0], "EliteFireEquipment", 1, "HeadHunter", "FireballsOnHit");
 
            //aurelionites blessing
            fillCraftable(myContentPack.Craftables[1], "EliteAurelioniteEquipment", 1, "HeadHunter", "BoostAllStats");
         
            //her biting embrace
            fillCraftable(myContentPack.Craftables[2], "EliteIceEquipment", 1, "HeadHunter", "Icicle");
         
            //his reassurance
            fillCraftable(myContentPack.Craftables[3], "EliteEarthEquipment", 1, "HeadHunter", "Plant");
         
            //his spiteful boon
            fillCraftable(myContentPack.Craftables[4], "EliteBeadEquipment", 1, "HeadHunter", "LunarTrinket");
         
            //nkuhana's retort
            fillCraftable(myContentPack.Craftables[5], "ElitePoisonEquipment", 1, "HeadHunter", "NovaOnHeal");
         
            //shared design
            fillCraftable(myContentPack.Craftables[6], "EliteLunarEquipment", 1, "HeadHunter", "ShinyPearl");
         
            //silence between 2 strikes
            fillCraftable(myContentPack.Craftables[7], "EliteLightningEquipment", 1, "HeadHunter", "ShockNearby");
           
            //spectral circlet
            fillCraftable(myContentPack.Craftables[8], "EliteHauntedEquipment", 1, "HeadHunter", "GhostOnKill");

            //void aspect
            fillCraftable(myContentPack.Craftables[9], "EliteVoidEquipment", 1, "HeadHunter", "ExtraLifeVoid");

            //speed aspect
            fillCraftable(myContentPack.Craftables[10], "EliteSecretSpeedEquipment", 1, "HeadHunter", "UtilitySkillMagazine");
        }

        private void fillCraftable(CraftableDef craftable, string aspect, int amount, string vultures, string itemB) {
            //null check
            var itemDefVultures = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(vultures));
            var itemDefItemB = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(itemB));
            var equipDefAspect = EquipmentCatalog.GetEquipmentDef(EquipmentCatalog.FindEquipmentIndex(aspect));

            if (!itemDefVultures || !itemDefItemB || !equipDefAspect)
            {
                Debug.LogError(
                    $"[AspectRecipes] Missing def: equip={aspect}, a={vultures}, b={itemB}");
                craftable.recipes = System.Array.Empty<Recipe>();
                craftable.pickup = null;
                return;
            }

            Recipe[] array = new Recipe[1];
            var recipe = new Recipe() { //make the recipe
                amountToDrop = amount,
                ingredients = new[] {
                    new RecipeIngredient {
                        pickup = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(vultures))
                    },
                    new RecipeIngredient {
                        pickup = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(itemB))
                    }
                }
            };

            //assign the recipe to the craftable def
            craftable.pickup = EquipmentCatalog.GetEquipmentDef(EquipmentCatalog.FindEquipmentIndex(aspect));
            array[0] = recipe;
            craftable.recipes = new[] { 
                recipe 
            };

            Debug.Log("Added recipe " + vultures + " + " + itemB + " = " + aspect);
        }

        private void createBlankCraftables() { //make a list of blank craftables
            for (int i = 0; i < numCraftables; i++) {
                var c = ScriptableObject.CreateInstance<CraftableDef>();
                c.name = "AspectRecipe #" + (i + 1);
                myContentPack.Craftables.Add(c);
                Debug.Log("Added blank recipe " + c.name);
            }
        }

        private RoR2.UI.LogBook.Entry[] addLogbookEntries(On.RoR2.UI.LogBook.LogBookController.orig_BuildPickupEntries orig, Dictionary<ExpansionDef, bool> expansionAvailability)
        {
            List<EquipmentDef> list = new List<EquipmentDef>();
            getEquipDefFromName("EliteGoldEquipment").dropOnDeathChance = 0f;
            getEquipDefFromName("EliteVoidEquipment").dropOnDeathChance = 0f;
            if (!Chainloader.PluginInfos.ContainsKey("com.TPDespair.ZetAspects") && !Chainloader.PluginInfos.ContainsKey("Wolfo.WolfoQoL")) { //dont do this if zet or wolfo are installed
                for (int i = 0; i < EliteCatalog.eliteDefs.Length; i++) { //add all elite defs to a list
                    EquipmentDef eliteEquipDef = EliteCatalog.eliteDefs[i].eliteEquipmentDef;
                    if (eliteEquipDef.dropOnDeathChance != 0f)
                    {
                        list.Add(eliteEquipDef);
                    }
                }
                Debug.Log("Done adding default aspects to list");
            }
            

            list.Add(getEquipDefFromName("EliteSecretSpeedEquipment")); //manually add speed equip
            list.Add(getEquipDefFromName("EliteVoidEquipment")); //manually add void equip

            for (int j = 0; j < list.Count; j++) { //make all equips in the list droppable temporarily so the game adds it to logbook
                list[j].canDrop = true;
            }
            Debug.Log("Done enabling canDrop on aspects");

            getEquipDefFromName("EliteLunarEquipment").isLunar = false; //dont make the lunar aspect a lunar so its grouped with the other aspects
            var result = orig.Invoke(expansionAvailability);

            for (int k = 0; k < list.Count; k++) { //change them back
                list[k].canDrop = false;
            }
            Debug.Log("Done disabling canDrop on aspects");

            getEquipDefFromName("EliteLunarEquipment").isLunar = true;
            Debug.Log("Done adding default aspects to logbook");

            return result;
        }

        private RoR2.UI.LogBook.Entry[] moveAspectsToEnd(On.RoR2.UI.LogBook.LogBookController.orig_BuildPickupEntries orig, Dictionary<ExpansionDef, bool> expansionAvailability) {
            var entries = orig.Invoke(expansionAvailability);

            //make 2 lists, 1 for non aspects and 1 for aspects 
            var normal = new List<RoR2.UI.LogBook.Entry>(entries.Length);
            var moved = new List<RoR2.UI.LogBook.Entry>();

            for (int i = 0; i < entries.Length; i++) { //for every logbook entry
                var currentEntry = entries[i]; //current logbook entry
                PickupIndex val = (PickupIndex)entries[i].extraData;
                PickupDef currentPickup = val.pickupDef;
                EquipmentIndex equipmentIndex = currentPickup.equipmentIndex; //get current logbook entry as an equipmentindex

                if (equipmentIndex != EquipmentIndex.None) { //check if its an equipment and get the equipment def
                    EquipmentDef currentEquip = EquipmentCatalog.GetEquipmentDef(equipmentIndex);

                    if (currentEquip != null && (currentEquip.passiveBuffDef != null || currentEquip.isBoss)) { //check if its boss or aspect equip
                        moved.Add(currentEntry); //if so add to the moved list to be added last
                    }
                    else {
                        normal.Add(currentEntry); //else add it to the normal list to be added first
                    }
                }
                else {
                    normal.Add(currentEntry);
                }
            }

            normal.AddRange(moved); //concatenate the moved list to the end of the normal one
            Debug.Log("Done moving aspects to the end of the logbook");
            return normal.ToArray();
        }

        private void addLogbookDescriptions() {
            //load void infestor icon
            string path = System.IO.Path.Combine(Paths.PluginPath, "Unknown-AspectRecipes.dll", "voidelitepickupicon.png"); //liquedator-AspectRecipes
            Debug.Log("Obtaining sprite file from: " + path);
            byte[] bytes = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(bytes);
            Sprite voidSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            getEquipDefFromName("EliteVoidEquipment").pickupIconSprite = voidSprite;
            updateText("EQUIPMENT_AFFIXVOID_NAME", "From One Thousand Miles Under");
            updateText("EQUIPMENT_AFFIXSECRETSPEED_DESC",
                "Increases <style=cIsUtility>movement speed</style> and <style=cIsDamage>attack speed</style> by <style=cIsUtility>200%</style> and negates all fall damage.<br>" +
                "Allies within <style=cIsUtility>40m</style> also gain these buffs which last for <style=cIsUtility>10 more seconds</style> upon leaving the radius.<br>" +
                "Enemies within the same <style=cIsUtility>40m</style> radius are <style=cIsUtility>slowed</style> for <style=cIsUtility>-50% movement speed</style> " +
                "and <style=cIsDamage>-1% attack speed</style> for <style=cIsUtility>every percentage of movement speed</style> you have.<br>" +
                "These debuffs last a further <style=cIsUtility>10 seconds</style> upon leaving the radius.");
        }

        private void updateText(string token, string desc) {
            LanguageAPI.Add(token, desc);
        }

        private EquipmentDef getEquipDefFromName(string name) {
            var idx = EquipmentCatalog.FindEquipmentIndex(name);
            var def = EquipmentCatalog.GetEquipmentDef(idx);
            Debug.Log("Returning " + name + " as an equipmentDef");
            return def;
        }

        public class myContentPack : IContentPackProvider {
            internal ContentPack contentPack = new ContentPack();

            public static List<CraftableDef> Craftables = new List<CraftableDef>();

            public string identifier => "liquedator.AspectRecipes";

            public void Initialise() {
                ContentManager.collectContentPackProviders += AddSelf;
            }

            private void AddSelf(ContentManager.AddContentPackProviderDelegate add) {
                add(this);
            }

            public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args) {
                contentPack.identifier = identifier;
                contentPack.craftableDefs.Add(Craftables.ToArray());
                args.ReportProgress(1f);
                yield break;
            }

            public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args) {
                ContentPack.Copy(contentPack, args.output);
                args.ReportProgress(1f);
                yield break;
            }

            public IEnumerator FinalizeAsync(FinalizeAsyncArgs args) {
                args.ReportProgress(1f);
                yield break;
            }
        }
    }
}   