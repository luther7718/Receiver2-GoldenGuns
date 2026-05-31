using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Receiver2;

namespace GoldenGuns
{
    [BepInPlugin("GoldenGuns", "GoldenGuns", "2.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        private static Dictionary<string, string> gunmap = new Dictionary<string, string>
        {
            { "wolfire.smith_and_wesson_model_10", "wolfire.smith_and_wesson_model_10_gold" },
            { "wolfire.colt_detective", "wolfire.colt_detective_gold" },
            { "wolfire.colt_saa", "wolfire.colt_saa_gold" },
            { "wolfire.hi_point_c9", "wolfire.hi_point_c9_gold" },
            { "wolfire.glock_17", "wolfire.glock_17_gold" },
            { "wolfire.beretta_m9", "wolfire.beretta_m9_gold" },
            { "wolfire.sig226", "wolfire.sig226_gold" },
            { "wolfire.desert_eagle", "wolfire.desert_eagle_gold" },
            { "wolfire.colt_m1911", "wolfire.colt_m1911_gold" }
        };

        private static Dictionary<string, string> challengemap = new Dictionary<string, string>
        {
            { "wolfire.smith_and_wesson_model_10", "wolfire.model_10_base_gold" },
            { "wolfire.colt_detective", "wolfire.colt_detective_base_gold" },
            { "wolfire.colt_saa", "wolfire.colt_saa_base_gold" },
            { "wolfire.hi_point_c9", "wolfire.hi_point_c9_base_gold" },
            { "wolfire.glock_17", "wolfire.glock17_base_gold" },
            { "wolfire.beretta_m9", "wolfire.beretta_m9_base_gold" },
            { "wolfire.sig226", "wolfire.sig226_base_gold" },
            { "wolfire.desert_eagle", "wolfire.desert_eagle_base_gold" },
            { "wolfire.colt_m1911", "wolfire.colt_m1911_base_gold" }
        };

        private static Dictionary<string, string> magmap = new Dictionary<string, string>
        {
             { "wolfire.hi_point_c9", "wolfire.hi_point_c9_std_cap_mag_gold" },
             { "wolfire.glock_17", "wolfire.glock_17_std_cap_mag_gold" },
             { "wolfire.beretta_m9", "wolfire.beretta_std_cap_mag_gold" },
             { "wolfire.sig226", "wolfire.p226_std_cap_mag_gold" },
             { "wolfire.desert_eagle", "wolfire.desert_eagle_std_cap_mag_gold" },
             { "wolfire.colt_m1911", "wolfire.colt_1911_std_cap_mag_gold" }
        };

        private static ConfigEntry<bool> enableRankedCampaign;
        private static ConfigEntry<bool> enableClassic;

        private void Awake()
        {
            Logger.LogInfo("Plugin GoldSpawner is loaded!");

            enableRankedCampaign = Config.Bind(
                "General",
                "Enable in RankedCampaign",
                true,
                "Replace guns with gold versions in The Dreaming."
            );

            enableClassic = Config.Bind(
                "General",
                "Enable in Classic",
                true,
                "Replace guns with gold versions in Classic Mode."
            );

            Harmony.CreateAndPatchAll(((object)this).GetType(), (string)null);
        }

        [HarmonyPatch(typeof(ReceiverCoreScript), "SpawnPlayer")]
        [HarmonyPrefix]
        public static void OnSpawnPlayer(ReceiverCoreScript __instance)
        {
            GameMode currentMode = __instance.game_mode.GetGameMode();

            bool isRankedEnabled = enableRankedCampaign.Value && currentMode == GameMode.RankingCampaign;
            bool isClassicEnabled = enableClassic.Value && currentMode == GameMode.Classic;

            if (!isRankedEnabled && !isClassicEnabled)
                return;

            PlayerLoadout currentLoadout = __instance.CurrentLoadout;
            string originalGunName = currentLoadout.gun_internal_name;

            if (gunmap.ContainsKey(originalGunName))
            {
                string challengeItem = challengemap[originalGunName];
                if (__instance.MallData.IsItemGroupUnlocked(challengeItem))
                {
                    currentLoadout.gun_internal_name = gunmap[originalGunName];

                    foreach (PlayerLoadoutEquipment equipment in currentLoadout.equipment)
                    {
                        if (equipment.equipment_type == EquipmentType.Magazine &&
                            magmap.ContainsKey(originalGunName) &&
                            equipment.magazine_class == MagazineClass.StandardCapacity)
                        {
                            equipment.internal_name = magmap[originalGunName];
                            equipment.magazine_class = MagazineClass.StandardCapacityGold;
                        }
                        else if (equipment.equipment_type == EquipmentType.Flashlight &&
                                 __instance.MallData.IsItemGroupUnlocked("wolfire.flashlight_gold"))
                        {
                            equipment.internal_name = "wolfire.flashlight_gold";
                        }
                    }
                }
            }
        }
    }
}
