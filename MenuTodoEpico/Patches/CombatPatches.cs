using HarmonyLib;

namespace SidaMenu
{
    internal static class CombatPatches
    {
        /// <summary>Forces the local kill timer back to zero.</summary>
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
        internal static class NoKillCooldownPatch
        {
            public static void Prefix(PlayerControl __instance, ref float time)
            {
                if (!States.noKillCooldown) return;
                if (__instance == null || !__instance.AmOwner) return;
                time = 0f;
            }
        }

        /// <summary>Lets the local player use any vent, from any distance and role.</summary>
        [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
        internal static class VentAnywherePatch
        {
            public static void Postfix(NetworkedPlayerInfo pc, ref bool canUse, ref bool couldUse, ref float __result)
            {
                if (!States.unlockVents) return;

                var lp = PlayerControl.LocalPlayer;
                if (lp == null || pc == null) return;
                if (pc.PlayerId != lp.PlayerId) return;

                canUse = true;
                couldUse = true;
                __result = 0f;
            }
        }
    }
}
