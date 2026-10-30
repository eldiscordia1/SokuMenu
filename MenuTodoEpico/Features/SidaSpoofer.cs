using HarmonyLib;

namespace SidaMenu
{
    /// <summary>Makes the client report a different platform to the lobby.</summary>
    public static class SidaSpoofer
    {
        public static int selectedPlatform = 5;

        public static readonly Platforms[] platforms =
        {
            Platforms.StandaloneSteamPC,
            Platforms.StandaloneEpicPC,
            Platforms.Playstation,
            Platforms.Xbox,
            Platforms.Switch,
            Platforms.Android,
            Platforms.IPhone
        };

        public static string Current =>
            selectedPlatform >= 0 && selectedPlatform < platforms.Length
                ? platforms[selectedPlatform].ToString()
                : "Unknown";

        public static void Cycle()
        {
            selectedPlatform = (selectedPlatform + 1) % platforms.Length;
            Notification.Push($"Platform spoof: {Current}", Theme.Accent);
            EventLog.Add($"Platform spoof set to {Current}", LogKind.Action);
        }

        [HarmonyPatch(typeof(Constants), nameof(Constants.GetPlatformType))]
        internal static class SpoofPlatformTypePatch
        {
            public static bool Prefix(ref Platforms __result)
            {
                if (!States.spoofPlatform) return true;
                if (selectedPlatform < 0 || selectedPlatform >= platforms.Length) return true;

                __result = platforms[selectedPlatform];
                return false;
            }
        }

        [HarmonyPatch(typeof(Constants), nameof(Constants.GetPlatformData))]
        internal static class SpoofPlatformDataPatch
        {
            public static void Postfix(ref PlatformSpecificData __result)
            {
                if (!States.spoofPlatform) return;
                if (__result == null) return;
                if (selectedPlatform < 0 || selectedPlatform >= platforms.Length) return;

                __result.Platform = platforms[selectedPlatform];
            }
        }
    }
}
