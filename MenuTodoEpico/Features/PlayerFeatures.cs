using System;
using UnityEngine;

namespace SidaMenu
{
    /// <summary>Per-player actions used by the Players tab.</summary>
    internal static class PlayerFeatures
    {
        public static void TeleportTo(PlayerControl? target)
        {
            var lp = PlayerControl.LocalPlayer;
            if (lp == null || target == null) return;

            try
            {
                var pos = target.transform.position;
                lp.NetTransform.RpcSnapTo(new Vector2(pos.x, pos.y));
                Notification.Push($"Teleported to {Utils.NameOf(target)}", Theme.Good);
            }
            catch (Exception e) { EventLog.Add("Teleport failed: " + e.Message, LogKind.Danger); }
        }

        public static void Report(PlayerControl? target)
        {
            var lp = PlayerControl.LocalPlayer;
            if (lp == null || target == null) return;

            try
            {
                lp.CmdReportDeadBody(target.Data);
                Notification.Push($"Reported {Utils.NameOf(target)}", Theme.Warn);
                EventLog.Add($"Reported {Utils.NameOf(target)}", LogKind.Action);
            }
            catch (Exception e) { EventLog.Add("Report failed: " + e.Message, LogKind.Danger); }
        }
    }
}
