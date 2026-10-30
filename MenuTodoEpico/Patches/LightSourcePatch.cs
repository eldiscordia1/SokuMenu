using HarmonyLib;

namespace SidaMenu
{
    /// <summary>Overrides the light radius so the whole map stays lit.</summary>
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
    internal static class FullBrightPatch
    {
        public static void Postfix(ref float __result)
        {
            if (!States.fullBright) return;
            __result = 50f;
        }
    }
}
