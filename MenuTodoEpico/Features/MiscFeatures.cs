using System;
using Hazel;
using InnerNet;
using UnityEngine;

namespace SidaMenu
{
    /// <summary>Chat tricks, exploits, spoofing and other odds and ends.</summary>
    internal static class MiscFeatures
    {
        // Sanction messages used to make a quick-chat bubble render as a red warning.
        private static readonly ushort[] SanctionMessages =
        {
            (ushort)StringNames.SanctionCheatingHacking,
            (ushort)StringNames.SanctionSpamming,
            (ushort)StringNames.SanctionBullying,
            (ushort)StringNames.SanctionHarassment,
            (ushort)StringNames.SanctionUnsportsmanlikeConduct,
            (ushort)StringNames.SanctionInappropriateChatDating,
            (ushort)StringNames.SanctionSelfHarmPromotion,
            (ushort)StringNames.SanctionDoxing,
            (ushort)StringNames.SanctionSexualMisconduct,
            (ushort)StringNames.SanctionIllegalActivity,
            (ushort)StringNames.SanctionImpersonationCeleb,
            (ushort)StringNames.SanctionImpersonationDevelopers,
            (ushort)StringNames.SanctionInappropriateNameDerogatory,
            (ushort)StringNames.SanctionInappropriateNameNsfw,
            (ushort)StringNames.SanctionInappropriateNameUnsportsmanlike,
            (ushort)StringNames.SanctionInappropriateChatPersonalInfo,
            (ushort)StringNames.SanctionRepeatOffender3,
            (ushort)StringNames.SanctionRepeatOffender10,
            (ushort)StringNames.SanctionWeaponizingRules,
            (ushort)StringNames.SanctionUnknown,
            (ushort)StringNames.SanctionDuration,
            (ushort)StringNames.SanctionPermanent
        };

        public static void Tick()
        {
            AntiBan();
            HostFeatures.Tick();
        }

        /// <summary>Keeps our own ban meter at zero so a report cannot escalate.</summary>
        private static void AntiBan()
        {
            if (!States.antiBan) return;
            if (PlayerControl.LocalPlayer == null) return;

            try
            {
                AmongUs.Data.DataManager.Player.Ban.BanPoints = 0f;
            }
            catch { /* data manager not ready yet */ }
        }

        // ------------------------------------------------------------------
        // Chat
        // ------------------------------------------------------------------
        public static void SendChat(string message)
        {
            var lp = PlayerControl.LocalPlayer;
            if (lp == null || string.IsNullOrEmpty(message)) return;

            try
            {
                lp.RpcSendChat(message);
                EventLog.Add("Sent chat: " + message, LogKind.Action);
            }
            catch (Exception e) { EventLog.Add("Chat failed: " + e.Message, LogKind.Danger); }
        }

        public static void SpamChat(string message, int times)
        {
            var lp = PlayerControl.LocalPlayer;
            if (lp == null) return;
            if (string.IsNullOrWhiteSpace(message)) { Notification.Push("Spam text is empty", Theme.Warn); return; }

            try
            {
                for (var i = 0; i < times; i++) lp.RpcSendChat(message);
                Notification.Push($"Sent {times} message(s)", Theme.Good);
                EventLog.Add($"Spammed chat x{times}", LogKind.Action);
            }
            catch (Exception e) { EventLog.Add("Spam failed: " + e.Message, LogKind.Danger); }
        }

        /// <summary>
        /// Sends a quick-chat packet that carries a sanction string, which renders a
        /// red warning bubble for everyone in the lobby.
        /// </summary>
        public static void RedChat()
        {
            var client = AmongUsClient.Instance;
            var lp = PlayerControl.LocalPlayer;
            if (client == null || lp == null) return;

            try
            {
                var randomSanction = SanctionMessages[UnityEngine.Random.Range(0, SanctionMessages.Length)];
                var mw = MessageWriter.Get(SendOption.Reliable);

                mw.StartMessage(5);
                mw.Write(client.GameId);
                mw.StartMessage(2);
                mw.WritePacked(lp.NetId);
                mw.Write((byte)RpcCalls.SendQuickChat);
                mw.Write((byte)3);
                mw.Write((ushort)StringNames.NumImpostorsP);
                mw.Write((byte)1);
                mw.Write((byte)2);
                mw.Write(randomSanction);
                mw.EndMessage();
                mw.EndMessage();

                client.SendOrDisconnect(mw);
                mw.Recycle();

                Notification.Push("Sent red chat", Theme.Danger);
                EventLog.Add("Sent red quick chat", LogKind.Danger);
            }
            catch (Exception e)
            {
                Notification.Push("Red chat failed", Theme.Warn);
                EventLog.Add("Red chat failed: " + e, LogKind.Danger);
            }
        }

        // Only works against clients that log chat (Hydra and other mod menus).
        public static void WindowsNotificationExploit()
        {
            var lp = PlayerControl.LocalPlayer;
            if (lp == null) return;

            try
            {
                lp.RpcSendChat("\u0007\u0007\u0007");
                Notification.Push("Sent bell characters", Theme.Warn);
            }
            catch { }
        }

        public static void Promotion()
        {
            SendChat("SidaMenu is the best Mod Menu that you can get RN!\n GO get it at:\n " +
                     "ｈｔｔｐｓ：／／ｇｉｔｈｕｂ．ｃｏｍ／ｈａｕｎｔ０１／ＳｉｄａＭｅｎｕ");
        }

        // ------------------------------------------------------------------
        // Utility
        // ------------------------------------------------------------------
        public static void BecomeInvisible()
        {
            var client = AmongUsClient.Instance;
            var lp = PlayerControl.LocalPlayer;
            if (client == null || lp == null || lp.MyPhysics == null) return;

            try
            {
                client.Despawn(lp.MyPhysics);
                Notification.Push("Despawned - you are invisible", Theme.Warn);
                EventLog.Add("Despawned local player (invisible)", LogKind.Danger);
            }
            catch (Exception e) { EventLog.Add("Invisible failed: " + e.Message, LogKind.Danger); }
        }

        public static void CopyLobbyCode()
        {
            if (AmongUsClient.Instance == null) return;

            string code;
            try
            {
                code = GameCode.IntToGameName(AmongUsClient.Instance.GameId)
                       ?? AmongUsClient.Instance.GameId.ToString();
            }
            catch (Exception e)
            {
                Safe.Report("lobby code", e);
                code = AmongUsClient.Instance.GameId.ToString();
            }

            Utils.CopyToClipboard(code, "lobby code");
        }
    }
}
