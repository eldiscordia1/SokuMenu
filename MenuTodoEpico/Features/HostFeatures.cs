using System;
using AmongUs.GameOptions;
using InnerNet;
using UnityEngine;

namespace SidaMenu
{
    /// <summary>Actions that only make sense (or only work) while hosting.</summary>
    internal static class HostFeatures
    {
        // ------------------------------------------------------------------
        // Guards and feedback
        // ------------------------------------------------------------------
        private static bool RequireHost(string what)
        {
            if (Utils.IsHost || Utils.IsFreePlay) return true;
            Notification.Push($"{what}: host only", Theme.Warn);
            return false;
        }

        private static void Ok(string what)
        {
            Notification.Push(what, Theme.Good);
            EventLog.Add(what, LogKind.Action);
        }

        private static void Fail(string what)
        {
            Notification.Push(what, Theme.Warn);
            EventLog.Add(what, LogKind.Warning);
        }

        // ------------------------------------------------------------------
        // Lobby / flow
        // ------------------------------------------------------------------
        public static void ResetTasks()
        {
            if (!RequireHost("Reset tasks")) return;
            if (ShipStatus.Instance == null) { Fail("Reset tasks: no ship"); return; }

            try
            {
                ShipStatus.Instance.Begin();
                Ok("Tasks reset");
            }
            catch (Exception e) { Fail("Reset tasks failed"); EventLog.Add(e.ToString(), LogKind.Danger); }
        }

        public static void ForceStart()
        {
            if (!RequireHost("Force start")) return;
            if (AmongUsClient.Instance == null) return;
            try
            {
                AmongUsClient.Instance.StartGame();
                Ok("Force started the game");
            }
            catch (Exception e) { Fail("Force start failed"); EventLog.Add(e.ToString(), LogKind.Danger); }
        }

        public static void ForceEnd()
        {
            if (!RequireHost("Force end")) return;
            if (GameManager.Instance == null) return;
            try
            {
                GameManager.Instance.RpcEndGame(GameOverReason.CrewmateDisconnect, true);
                Ok("Forced the round to end");
            }
            catch (Exception e) { Fail("Force end failed"); EventLog.Add(e.ToString(), LogKind.Danger); }
        }

        public static void ApplyMaxPlayers()
        {
            if (!States.morePlayers) return;
            if (!RequireHost("Max players")) return;
            if (GameManager.Instance == null || GameManager.Instance.LogicOptions == null) return;

            try
            {
                GameManager.Instance.LogicOptions.currentGameOptions
                    .SetInt(Int32OptionNames.MaxPlayers, States.maxPlayers);
            }
            catch (Exception e) { Debug.LogError($"[SidaMenu] max players: {e}"); }
        }

        // ------------------------------------------------------------------
        // Kills
        // ------------------------------------------------------------------
        public static void Murder(PlayerControl? target)
        {
            var lp = PlayerControl.LocalPlayer;
            if (lp == null || target == null || target.PlayerId == lp.PlayerId) return;

            try
            {
                if (Utils.IsHost) lp.RpcMurderPlayer(target, true);
                else lp.MurderPlayer(target, MurderResultFlags.Succeeded);

                EventLog.Add($"Killed {Utils.NameOf(target)}", LogKind.Danger);
            }
            catch (Exception e) { EventLog.Add("Kill failed: " + e.Message, LogKind.Danger); }
        }

        public static void KillAll(bool crew, bool impostors)
        {
            if (!RequireHost("Kill all")) return;

            var count = 0;
            foreach (var pc in Utils.Others())
            {
                var isImpostor = false;
                try { isImpostor = pc.Data != null && pc.Data.Role != null && pc.Data.Role.IsImpostor; }
                catch { /* role not assigned yet */ }

                if (isImpostor ? impostors : crew)
                {
                    Murder(pc);
                    count++;
                }
            }

            if (count > 0) Notification.Push($"Killed {count} player(s)", Theme.Danger);
        }

        // ------------------------------------------------------------------
        // Sabotage
        // ------------------------------------------------------------------
        public static void Sabotage(SystemTypes type, string label)
        {
            if (!RequireHost(label)) return;
            if (ShipStatus.Instance == null) { Fail(label + ": no ship"); return; }

            try
            {
                ShipStatus.Instance.RpcUpdateSystem(type, 128);
                Ok($"{label} sabotaged");
            }
            catch (Exception e) { Fail($"{label} failed"); EventLog.Add(e.ToString(), LogKind.Danger); }
        }

        public static void RepairAll()
        {
            if (ShipStatus.Instance == null) { Fail("Repair: no ship"); return; }
            try
            {
                ShipStatus.Instance.RepairCriticalSabotages();
                Ok("Repaired all sabotages");
            }
            catch (Exception e) { Fail("Repair failed"); EventLog.Add(e.ToString(), LogKind.Danger); }
        }

        // ------------------------------------------------------------------
        // Meetings
        // ------------------------------------------------------------------
        public static void CallMeeting()
        {
            var lp = PlayerControl.LocalPlayer;
            if (lp == null) { Fail("Call meeting: not in game"); return; }

            try
            {
                lp.CmdReportDeadBody(null);
                Ok("Called a meeting");
            }
            catch (Exception e) { Fail("Call meeting failed"); EventLog.Add(e.ToString(), LogKind.Danger); }
        }

        public static void SkipMeeting()
        {
            if (MeetingHud.Instance == null) { Fail("Skip meeting: no meeting"); return; }

            try
            {
                MeetingHud.Instance.RpcVotingComplete(new MeetingHud.VoterState[0], null, true);
                Ok("Skipped the meeting");
            }
            catch (Exception e) { Fail("Skip meeting failed"); EventLog.Add(e.ToString(), LogKind.Danger); }
        }

        // ------------------------------------------------------------------
        // Moderation
        // ------------------------------------------------------------------
        public static void Kick(PlayerControl? target, bool ban = false)
        {
            if (target == null || AmongUsClient.Instance == null) return;
            if (!RequireHost(ban ? "Ban" : "Kick")) return;

            try
            {
                var id = AmongUsClient.Instance.GetClientIdFromCharacter(target);
                if (id < 0) { Fail("Player not networked"); return; }

                AmongUsClient.Instance.KickPlayer(id, ban);
                Ok($"{(ban ? "Banned" : "Kicked")} {Utils.NameOf(target)}");
            }
            catch (Exception e) { Fail("Kick failed"); EventLog.Add(e.ToString(), LogKind.Danger); }
        }

        public static void KickEveryone()
        {
            if (!RequireHost("Kick everyone")) return;

            var count = 0;
            foreach (var pc in Utils.Others())
            {
                try
                {
                    var id = AmongUsClient.Instance.GetClientIdFromCharacter(pc);
                    if (id >= 0)
                    {
                        AmongUsClient.Instance.KickPlayer(id, false);
                        count++;
                    }
                }
                catch (Exception e) { EventLog.Add(e.ToString(), LogKind.Danger); }
            }

            Notification.Push($"Kicked {count} player(s)", Theme.Danger);
            EventLog.Add($"Kicked everyone ({count})", LogKind.Danger);
        }

        // ------------------------------------------------------------------
        // Vents
        // ------------------------------------------------------------------
        public static void ExitAllVents()
        {
            var lp = PlayerControl.LocalPlayer;
            if (lp == null || lp.MyPhysics == null) return;

            try
            {
                lp.MyPhysics.ExitAllVents();
                lp.moveable = true;
                Ok("Left the vent");
            }
            catch (Exception e) { EventLog.Add("Exit vent failed: " + e.Message, LogKind.Danger); }
        }

        /// <summary>Called every frame from the UI tick.</summary>
        public static void Tick()
        {
            if (States.morePlayers) ApplyMaxPlayers();
        }
    }
}
