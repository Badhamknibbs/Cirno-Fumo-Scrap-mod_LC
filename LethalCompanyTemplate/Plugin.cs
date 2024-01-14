using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CirnoFumoScrap
{
   [BepInPlugin(modGUID, modName, modVersion)]
   [BepInDependency(LethalLib.Plugin.ModGUID)]
   public class FumoCirnoMod : BaseUnityPlugin {
      private const string modGUID = "badham.lc.cirnoscrap";
      private const string modName = "Cirno Fumo Scrap";
      private const string modVersion = "2.0.1";

      private readonly Harmony harmony = new Harmony(modGUID);

      private static FumoCirnoMod Instance;

      private ConfigEntry<int> configFumoRarity;

      internal ManualLogSource mls;

      public static AssetBundle CirnoAssets;

      private void Awake() {
         if (Instance == null) {
            Instance = this;
         }

         CirnoAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "fumocirno"));

         mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
         mls.LogInfo($"Cirno has infiltrated your facilities [Version {modVersion}]...");

         harmony.PatchAll(typeof(FumoCirnoMod));

         configFumoRarity = Config.Bind("General", "FumoRarity", 30, new ConfigDescription("How rare Fumo Cirnos are to find. Lower values mean Fumos spawn less often", new AcceptableValueRange<int>(0, 100)));

         Item Cirno = CirnoAssets.LoadAsset<Item>("assets/Mods/CirnoFumo/FumoCirno.asset");
         if (Cirno == null) {
            mls.LogError("Failed to load Cirno prefab.");
         } else {
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(Cirno.spawnPrefab);
            LethalLib.Modules.Items.RegisterScrap(Cirno, configFumoRarity.Value, LethalLib.Modules.Levels.LevelTypes.All);
         }
      }
   }
}