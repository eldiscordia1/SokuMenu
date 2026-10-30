using System.Collections.Generic;
using UnityEngine;

namespace SidaMenu
{
    internal enum LogKind
    {
        Info,
        Action,
        Host,
        Warning,
        Danger
    }

    internal sealed class LogEntry
    {
        public string Time = "";
        public string Text = "";
        public LogKind Kind;
    }

    /// <summary>Timestamped event feed, mirroring the in-game console tab and toasts.</summary>
    internal static class EventLog
    {
        public const int Max = 500;

        public static readonly List<LogEntry> Entries = new List<LogEntry>();

        public static bool Paused;

        public static void Add(string text, LogKind kind = LogKind.Info)
        {
            if (string.IsNullOrEmpty(text)) return;
            Entries.Add(new LogEntry
            {
                Time = System.DateTime.Now.ToString("HH:mm:ss"),
                Text = text,
                Kind = kind
            });

            if (Entries.Count > Max) Entries.RemoveAt(0);
        }

        public static void Clear() => Entries.Clear();

        public static Color ColorFor(LogKind kind)
        {
            switch (kind)
            {
                case LogKind.Host: return Theme.Warn;
                case LogKind.Warning: return Theme.Warn;
                case LogKind.Danger: return Theme.Danger;
                case LogKind.Action: return Theme.Good;
                default: return Theme.Text;
            }
        }

        public static string TagFor(LogKind kind)
        {
            switch (kind)
            {
                case LogKind.Host: return "HOST";
                case LogKind.Warning: return "WARN";
                case LogKind.Danger: return "DANGER";
                case LogKind.Action: return "ACT ";
                default: return "INFO";
            }
        }
    }
}
