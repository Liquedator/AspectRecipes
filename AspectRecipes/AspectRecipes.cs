using BepInEx;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System.Collections;
using RoR2.ContentManagement;

namespace AspectRecipes {
    [BepInDependency(R2API.R2API.PluginGUID)]

    //This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    //This is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    //BaseUnityPlugin itself inherits from MonoBehaviour, so you can use this as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class AspectRecipes : BaseUnityPlugin {
        //The Plugin GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config).
        //If we see this PluginGUID as it is on thunderstore, we will deprecate this mod. Change the PluginAuthor and the PluginName !
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "liquedator";
        public const string PluginName = "AspectRecipes";
        public const string PluginVersion = "1.0.2";

        //list of new craftables
        internal static readonly List<CraftableDef> Craftables = new();

        //number of new entries
        private const int numCraftables = 10;

        //The Awake() method is run at the very start when the game is initialized.
        public void Awake() {
            createBlankCraftables();
            new myContentPack().Initialise();
            PickupCatalog.availability.CallWhenAvailable(DefineRecipes);
        }

        internal static void DefineRecipes() {
            //ifrits distinction
            fillCraftable(Craftables[0], "EliteFireEquipment", 1, "HeadHunter", "FireballsOnHit");
 
            //aurelionites blessing
            fillCraftable(Craftables[1], "EliteAurelioniteEquipment", 1, "HeadHunter", "BoostAllStats");
         
            //her biting embrace
            fillCraftable(Craftables[2], "EliteIceEquipment", 1, "HeadHunter", "Icicle");
         
            //his reassurance
            fillCraftable(Craftables[3], "EliteEarthEquipment", 1, "HeadHunter", "Plant");
         
            //his spiteful boon
            fillCraftable(Craftables[4], "EliteBeadEquipment", 1, "HeadHunter", "LunarTrinket");
         
            //nkuhana's retort
            fillCraftable(Craftables[5], "ElitePoisonEquipment", 1, "HeadHunter", "NovaOnHeal");
         
            //shared design
            fillCraftable(Craftables[6], "EliteLunarEquipment", 1, "HeadHunter", "ShinyPearl");
         
            //silence between 2 strikes
            fillCraftable(Craftables[7], "EliteLightningEquipment", 1, "HeadHunter", "ShockNearby");
           
            //spectral circlet
            fillCraftable(Craftables[8], "EliteHauntedEquipment", 1, "HeadHunter", "GhostOnKill");

            //void aspect
            fillCraftable(Craftables[9], "EliteVoidEquipment", 1, "HeadHunter", "ExtraLifeVoid");
        }

        private static CraftableDef fillCraftable(CraftableDef craftable, string aspect, int amount, string vultures, string itemB) {
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
                return craftable;
            }

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
            craftable.recipes = new[] {
                recipe
            };

            return craftable;
        }

        private void createBlankCraftables() {
            for (int i = 0; i < numCraftables; i++) {
                Craftables.Add(ScriptableObject.CreateInstance<CraftableDef>());
            }
        }

        public class myContentPack : IContentPackProvider {
            internal ContentPack contentPack = new ContentPack();
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