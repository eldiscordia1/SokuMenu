using HarmonyLib;

namespace SidaMenu
{
    internal static class HostPatches
    {
        /// <summary>Stops the round from ending while the toggle is on.</summary>
        [HarmonyPatch(typeof(GameManager), nameof(GameManager.RpcEndGame))]
        internal static class NoEndGamePatch
        {
            public static bool Prefix() => !States.noEndGame;
        }
    }
}
