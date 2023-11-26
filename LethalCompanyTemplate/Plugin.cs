using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CirnoFumoScrap
{
   [BepInPlugin(modGUID, modName, modVersion)]
   [BepInDependency(LC_API.MyPluginInfo.PLUGIN_GUID)]
   public class FumoCirnoMod : BaseUnityPlugin {
      private const string modGUID = "badham.lc.cirnoscrap";
      private const string modName = "Cirno Fumo Scrap";
      private const string modVersion = "1.0.0";

      private readonly Harmony harmony = new Harmony(modGUID);

      private static FumoCirnoMod Instance;

      private ConfigEntry<int> configFumoRarity;
      private ConfigEntry<bool> configOopsAllFumo;

      internal ManualLogSource mls;
   
      private void Awake() {
         if (Instance == null) {
            Instance = this;
         }

         configFumoRarity = Config.Bind<int>("General", "FumoRarity", 30, new ConfigDescription("How rare Fumo Cirnos are to find. Lower values mean Fumos spawn less often", new AcceptableValueRange<int>(0, 100)));
         configOopsAllFumo = Config.Bind("Fun", "OopsAllFumo", false, "If enabled, all scrap spawns are replaced with Fumos. May not work as intended with other mods that tweak currentLevel.spawnableScrap");

         mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
         mls.LogInfo("Cirno has infiltrated your facilities...");

         harmony.PatchAll(typeof(FumoCirnoMod));
         harmony.PatchAll(typeof(RoundManagerPatch));
      }

      [HarmonyPatch(typeof(RoundManager))]
      internal class RoundManagerPatch {
         // Replace all spawnables with cirno fumos
         [HarmonyPatch("SpawnScrapInLevel")]
         [HarmonyPrefix]
         static void FumoInjection(ref SelectableLevel ___currentLevel) {
            SpawnableItemWithRarity itemWithRarity = new SpawnableItemWithRarity();
            itemWithRarity.rarity = Instance.configFumoRarity.Value;
            Item Cirno = LC_API.BundleAPI.BundleLoader.GetLoadedAsset<Item>("assets/fumocirno.asset");
            if (Cirno == null) {
               Instance.mls.LogInfo("Failed to find Cirno asset.");
               return;
            }
            itemWithRarity.spawnableItem = Cirno;
            if (Instance.configOopsAllFumo.Value) {
               OopsAllFumos(ref ___currentLevel, ref itemWithRarity);
            } else {
               AddFumoScrap(ref ___currentLevel, ref itemWithRarity);
            }
         }
      }

      static void AddFumoScrap(ref SelectableLevel ___currentLevel, ref SpawnableItemWithRarity itemWithRarity) {
         Instance.mls.LogInfo("Adding Cirno Fumo as scrap to current level...");

         ___currentLevel.spawnableScrap.Add(itemWithRarity);
         Instance.mls.LogInfo($"Added Cirno as spawnable scrap with rarity {itemWithRarity.rarity}");
      }

      static void OopsAllFumos(ref SelectableLevel ___currentLevel, ref SpawnableItemWithRarity itemWithRarity) {
         Instance.mls.LogInfo("Replacing spawnable items with Cirno fumos...");

         ___currentLevel.spawnableScrap.Clear();
         ___currentLevel.spawnableScrap.Add(itemWithRarity);
         Instance.mls.LogInfo("Successfully set Cirno as only spawnable:");
         Instance.mls.LogInfo(___currentLevel.spawnableScrap);
      }
   }
}