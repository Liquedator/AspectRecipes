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
using UnityEngine.AddressableAssets;
using HG;



namespace AspectRecipes {
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(PrefabAPI.PluginGUID)]

    public class AspectRecipes : BaseUnityPlugin {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "liquedator";
        public const string PluginName = "AspectRecipes";
        public const string PluginVersion = "1.0.6";

        //number of new entries
        private const int numCraftables = 11;

        //The Awake() method is run at the very start when the game is initialized.
        public void Awake() {
            //load recipes
            createBlankCraftables();
            new myContentPack().Initialise();
            PickupCatalog.availability.CallWhenAvailable(DefineRecipes);

            //add logbook entries for aspects
            On.RoR2.UI.LogBook.LogBookController.BuildPickupEntries += addLogbookEntries;   
            //move aspects to the end
            On.RoR2.UI.LogBook.LogBookController.BuildPickupEntries += moveAspectsToEnd;
            //add descriptions for aspects
            PickupCatalog.availability.CallWhenAvailable(addLogbookDescriptions);
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
            string path = System.IO.Path.Combine(Paths.PluginPath, "Unknown-AspectRecipes.dll", "voidelitepickupicon.png");
            //string path = System.IO.Path.Combine(Paths.PluginPath, "liquedator-AspectRecipes", "AspectRecipes", "voidelitepickupicon.png");
            Debug.Log("Obtaining sprite file from: " + path);
            byte[] bytes = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(bytes);
            Sprite voidSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            getEquipDefFromName("EliteVoidEquipment").pickupIconSprite = voidSprite;

            //do the pickup def too, for chef crafting menu
            PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(getEquipDefFromName("EliteVoidEquipment").equipmentIndex);
            PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
            pickupDef.iconSprite = voidSprite;
            pickupDef.iconTexture = voidSprite.texture;

            //do the 2d prefab
            setVoidInfestorModel();

            updateText("EQUIPMENT_AFFIXVOID_NAME", "From One Thousand Miles Under");
            updateText("EQUIPMENT_AFFIXVOID_PICKUP", "Become an aspect of the Void.");

            //update descriptions
            //<style=cIsUtility></style>
            //<style=cIsHealing></style>
            //<style=cIsDamage></style>
            //<color=#FF7F7F></color>
            //<br>
            updateText("EQUIPMENT_AFFIXVOID_DESC",
                "Gain <style=cIsHealing>50% maximum health</style> but <style=cIsDamage>reduce base damage</style> by <style=cIsDamage>30%</style>.<br>" +
                "<style=cIsDamage>100%</style> chance on hit to apply a stack of <style=cIsDamage>collapse</style> on enemies for <style=cIsDamage>400%</style> TOTAL damage per stack after <style=cIsDamage>3</style> seconds.<br>" +
                "Gain a <style=cIsUtility>bubble shield</style> which <style=cIsUtility>blocks</style> any single instance of damage. Recharges after <style=cIsUtility>15</style> seconds.");

            updateText("EQUIPMENT_AFFIXSECRETSPEED_DESC",
                "Increase <style=cIsUtility>movement speed</style> and <style=cIsDamage>attack speed</style> by <style=cIsUtility>200%</style> and <style=cIsUtility>negate all fall damage</style>.<br>" +
                "Allies within <style=cIsUtility>40m</style> also gain these buffs, which last for a further <style=cIsUtility>10</style> seconds upon leaving the radius.<br>" +
                "Enemies within the same <style=cIsUtility>40m</style> radius have <style=cIsUtility>50%</style> reduced <style=cIsUtility>movement speed</style> " +
                "and <style=cIsDamage>1%</style> reduced <style=cIsDamage>attack speed</style> for <style=cIsUtility>every percentage of movement speed</style> you have.<br>" +
                "These debuffs last a further <style=cIsUtility>10</style> seconds upon leaving the radius.");

            updateText("EQUIPMENT_AFFIXAURELIONITE_DESC",
                "Enemies drop a treasure on hit worth <style=cIsUtility>8 gold</style>, which <style=cIsUtility>scales over time</style>.<br>" +
                "On activation, summon a golden <style=cIsDamage>spike attack</style> consisting of <style=cIsDamage>2 pulses</style>.<br>" +
                "The outer ring does <style=cIsDamage>15% damage</style>, followed by the inner ring which does <style=cIsDamage>150% damage</style>.");

            updateText("EQUIPMENT_AFFIXBEAD_DESC",
                "<style=cIsUtility>Tether</style> to <style=cIsUtility>5 nearby allies</style> in a <style=cIsUtility>35m</style> radius.<br>" +
                "Tethered allies gain <style=cIsHealing>300 armor</style> and when hit, begin to charge a <style=cIsDamage>lunar spike ball</style>.<br>" +
                "After being hit <style=cIsDamage>10</style> times, the ball is <style=cIsDamage>fired</style>, locking on to a target and dealing <style=cIsDamage>100%</style> damage. " +
                "Applies <style=cIsDamage>lunar ruin</style>.");

            updateText("EQUIPMENT_AFFIXRED_DESC",
                "Leave behind a <style=cIsDamage>trail of fire</style> which deals <style=cIsDamage>150%</style> damage <style=cIsDamage>per second</style>.<br>" +
                "<style=cIsDamage>100%</style> chance on hit to <style=cIsDamage>burn</style> enemies for <style=cIsDamage>50%</style> TOTAL damage over time.");

            updateText("EQUIPMENT_AFFIXBLUE_DESC",
                "<style=cIsHealing>Convert 50%</style> of your <style=cIsHealing>maximum health</style> into <style=cIsHealing>regenerating shields</style>.<br>" +
                "<style=cIsDamage>100%</style> chance on hit to attach <style=cIsDamage>lightning bombs</style> to enemies which <style=cIsDamage>explode</style> after a short delay, dealing <style=cIsDamage>50%</style> TOTAL damage.");

            updateText("EQUIPMENT_AFFIXWHITE_DESC",
                "<style=cIsUtility>100%</style> chance on hit to <style=cIsUtility>greatly slow</style> enemies, reducing their <style=cIsUtility>movement speed</style> by <style=cIsUtility>80%</style>.<br>" +
                "<color=#FF7F7F>On death</color>, <style=cIsDamage>explode</style> in a small area, dealing <style=cIsDamage>150%</style> damage and <style=cIsDamage>freezing</style> enemies in the explosion.");

            updateText("EQUIPMENT_AFFIXPOISON_DESC",
                "Disable <style=cIsHealing>healing</style> on hit for <style=cIsHealing>8</style> seconds.<br>" +
                "Periodically release <style=cIsDamage>spiky balls</style>, which deal <style=cIsDamage>100%</style> damage and sprout <style=cIsDamage>spike pits</style> which deal <style=cIsDamage>100%</style> contact damage.<br>" +
                "<color=#FF7F7F>On death</color>, spawn a <style=cIsDamage>Malachite Urchin</style> which shoots at nearby enemies.");

            updateText("EQUIPMENT_AFFIXHAUNTED_DESC",
                "<style=cIsUtility>100%</style> chance on hit to <style=cIsUtility>greatly slow</style> enemies, reducing their <style=cIsUtility>movement speed</style> by <style=cIsUtility>80%</style>.<br>" +
                "Gain a <style=cIsUtility>30m</style> aura which <style=cIsUtility>cloaks</style> allies within and makes them <style=cIsUtility>invisible</style>.");

            updateText("EQUIPMENT_AFFIXEARTH_DESC",
                "<style=cIsHealing>Heal</style> the nearest damaged ally within <style=cIsHealing>30m</style> for <style=cIsHealing>40%</style> base damage <style=cIsHealing>4 times</style> per second.<br>" +
                "<color=#FF7F7F>On death</color>, spawn a destructible <style=cIsHealing>healing core</style> which explodes after a few seconds, <style=cIsHealing>healing</style> all entities within for <style=cIsHealing>80 hp</style>.");

            updateText("EQUIPMENT_AFFIXLUNAR_DESC",
                "<style=cIsDamage>100%</style> chance on hit to <style=cIsDamage>cripple</style> enemies, reducing their <style=cIsDamage>armor</style> by <style=cIsDamage>20%</style> and their <style=cIsUtility>movement speed</style> by <style=cIsUtility>50%</style>.<br>" +
                "Gain <style=cIsHealing>25% maximum health</style>. <style=cIsHealing>Convert</style> all but <style=cIsHealing>1 health</style> into <style=cIsHealing>regenerating shields</style>.<br>" +
                "Gain <style=cIsUtility>30% movement speed</style>.");

            updateText("EQUIPMENT_AFFIXCOLLECTIVE_DESC",
                "Gain a <style=cIsUtility>30m</style> dome which <style=cIsUtility>blocks projectiles</style> and <style=cIsUtility>reduces</style> ally and self cooldowns by <style=cIsUtility>20%</style>.<br>" +
                "<color=#FF7F7F>On death</color>, <style=cIsDamage>explode</style> for <style=cIsDamage>100%</style> damage and <style=cIsUtility>disable</style> items for <style=cIsUtility>2.5</style> seconds.");
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

        private void setVoidInfestorModel() { //get the prefab from the bodycatalog
            //declare eqdef and pickupdef
            var eqDef = getEquipDefFromName("EliteVoidEquipment");
            PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(eqDef.equipmentIndex);
            PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);

            //pull the addressable prefab and clone onto the logbook model
            GameObject displayPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/EliteVoid/DisplayAffixVoid.prefab").WaitForCompletion();
            GameObject pickupDisplay = PrefabAPI.InstantiateClone(displayPrefab, "PickupAffixVoid_AspectRecipes", false);

            //scale it so it looks proper in game
            pickupDisplay.transform.GetChild(0).GetChild(2).SetAsFirstSibling();

            pickupDisplay.transform.GetChild(1).localPosition = new Vector3(0f, 0.7f, 0f);

            pickupDisplay.transform.GetChild(1).GetChild(0).localPosition = new Vector3(0f, -0.5f, -0.6f);
            pickupDisplay.transform.GetChild(1).GetChild(0).localScale = new Vector3(1.5f, 1.5f, 1.5f);

            ((Component)pickupDisplay.transform.GetChild(1).GetChild(1)).gameObject.SetActive(false);
            ((Component)pickupDisplay.transform.GetChild(1).GetChild(3)).gameObject.SetActive(false);

            pickupDisplay.transform.GetChild(0).eulerAngles = new Vector3(310f, 0f, 0f);
            pickupDisplay.transform.GetChild(0).localScale = new Vector3(0.75f, 0.75f, 0.75f);

            var itemDisplay = pickupDisplay.GetComponent<ItemDisplay>();
    
            ItemDisplay component = pickupDisplay.GetComponent<ItemDisplay>();
            ArrayUtils.ArrayRemoveAtAndResize<CharacterModel.RendererInfo>(ref component.rendererInfos, 4, 1);
            
            var mpp = pickupDisplay.AddComponent<ModelPanelParameters>(); //for logbook model
            mpp.modelRotation = Quaternion.Euler(-20f, -45f, 0f); //rotate it backwards a bit

            eqDef.pickupModelReference = new AssetReferenceT<GameObject>("");
            eqDef.pickupModelPrefab = pickupDisplay;

            pickupDef.displayPrefab = pickupDisplay;
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