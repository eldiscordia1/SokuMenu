using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine.Analytics;

namespace SidaMenu
{
    [BepInAutoPlugin]
    [BepInProcess("Among Us.exe")]
    public partial class SidaMenu : BasePlugin
    {
        public override void Load()
        {
            Features.BuildCatalog();

            if (States.noTelemetry)
            {
                /* Disables Unity telemetry - mirrored from MalumMenu. */
                Analytics.enabled = false;
                Analytics.deviceStatsEnabled = false;
                PerformanceReporting.enabled = false;
            }

            AddComponent<MenuUI>();
            new Harmony("com.sida.menu").PatchAll();

            ConfigManager.LoadOnStart();

            Log.LogInfo($"SidaMenu loaded - {Features.All.Count} features registered");
        }
    }
}
