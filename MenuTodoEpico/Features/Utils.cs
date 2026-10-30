using System;
using System.Collections.Generic;
using UnityEngine;

namespace SidaMenu
{
    /// <summary>Shared state probes and small helpers used all over the menu.</summary>
    internal static class Utils
    {
        public static bool IsHost
        {
            get
            {
                try { return AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost; }
                catch (Exception e) { Safe.Report("host probe", e); return false; }
            }
        }

        public static bool IsPlayer
        {
            get
            {
                try { return PlayerControl.LocalPlayer != null; }
                catch (Exception e) { Safe.Report("local player probe", e); return false; }
            }
        }

        public static bool IsShip
        {
            get
            {
                try { return ShipStatus.Instance != null; }
                catch (Exception e) { Safe.Report("ship probe", e); return false; }
            }
        }

        public static bool IsMeeting
        {
            get
            {
                try { return MeetingHud.Instance != null; }
                catch (Exception e) { Safe.Report("meeting probe", e); return false; }
            }
        }

        public static bool InLobby
        {
            get
            {
                try { return LobbyBehaviour.Instance != null; }
                catch (Exception e) { Safe.Report("lobby probe", e); return false; }
            }
        }

        /// <summary>
        /// Freeplay and local games have no real game id, and there is nothing to
        /// protect there, so host-only tools are allowed.
        /// </summary>
        public static bool IsFreePlay
        {
            get
            {
                try
                {
                    return IsPlayer && (AmongUsClient.Instance == null || AmongUsClient.Instance.GameId == 0);
                }
                catch { return false; }
            }
        }

        public static bool IsAlive
        {
            get
            {
                try
                {
                    var lp = PlayerControl.LocalPlayer;
                    return lp != null && lp.Data != null && !lp.Data.IsDead;
                }
                catch (Exception e) { Safe.Report("alive probe", e); return false; }
            }
        }

        public static bool ChatOpen
        {
            get
            {
                try
                {
                    var hud = HudManager.Instance;
                    var chat = hud != null ? hud.Chat : null;
                    return chat != null && chat.IsOpenOrOpening;
                }
                catch { return false; }
            }
        }

        public static string NameOf(PlayerControl? pc)
        {
            try
            {
                if (pc == null || pc.Data == null) return "?";
                return string.IsNullOrWhiteSpace(pc.Data.PlayerName) ? "Player" : pc.Data.PlayerName!;
            }
            catch { return "?"; }
        }

        /// <summary>
        /// A snapshot of every player. Returned as a list so callers never enumerate a
        /// live Il2Cpp collection while the game mutates it.
        /// </summary>
        public static List<PlayerControl> AllPlayers()
        {
            var list = new List<PlayerControl>();

            try
            {
                var all = PlayerControl.AllPlayerControls;
                if (all == null) return list;

                foreach (var pc in all)
                {
                    if (pc != null) list.Add(pc);
                }
            }
            catch (Exception e) { Safe.Report("player list", e); }

            return list;
        }

        public static IEnumerable<PlayerControl> Others()
        {
            var lp = PlayerControl.LocalPlayer;
            foreach (var pc in AllPlayers())
            {
                if (lp == null || pc.PlayerId != lp.PlayerId) yield return pc;
            }
        }

        public static string RoleOf(PlayerControl pc)
        {
            try
            {
                var role = pc.Data != null ? pc.Data.Role : null;
                return role == null ? "?" : role.NiceName;
            }
            catch (Exception e) { Safe.Report("role name", e); return "?"; }
        }

        public static Color RoleColorOf(PlayerControl pc)
        {
            try
            {
                var role = pc.Data != null ? pc.Data.Role : null;
                return role == null ? Theme.Text : role.TeamColor;
            }
            catch { return Theme.Text; }
        }

        public static float DistanceTo(PlayerControl target)
        {
            try
            {
                var lp = PlayerControl.LocalPlayer;
                if (lp == null || target == null) return -1f;
                return Vector2.Distance(lp.transform.position, target.transform.position);
            }
            catch { return -1f; }
        }

        public static float Ping
        {
            get
            {
                try { return AmongUsClient.Instance != null ? AmongUsClient.Instance.Ping : 0f; }
                catch (Exception e) { Safe.Report("ping probe", e); return 0f; }
            }
        }

        /// <summary>Clipboard access can itself be unavailable, so it is always guarded.</summary>
        public static void CopyToClipboard(string text, string what)
        {
            if (string.IsNullOrEmpty(text))
            {
                Notification.Push($"Nothing to copy for {what}", Theme.Warn);
                return;
            }

            try
            {
                GUIUtility.systemCopyBuffer = text;
                Notification.Push($"Copied {what}: {text}", Theme.Good);
                EventLog.Add($"Copied {what} to clipboard", LogKind.Action);
            }
            catch (Exception e)
            {
                Safe.Report("clipboard", e);
                Notification.Push($"Could not copy {what}", Theme.Warn);
            }
        }
    }
}
