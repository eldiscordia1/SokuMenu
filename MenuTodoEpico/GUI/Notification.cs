using System.Collections.Generic;
using UnityEngine;

namespace SidaMenu
{
    /// <summary>Stacking toast notifications drawn over the game HUD.</summary>
    internal static class Notification
    {
        private sealed class Toast
        {
            public string Text = "";
            public Color Color;
            public float Born;
            public float Life = 3.2f;
        }

        private static readonly List<Toast> List = new List<Toast>();
        private static GUIStyle? _style;

        public static void Push(string text, Color? color = null)
        {
            if (string.IsNullOrEmpty(text)) return;
            List.Add(new Toast
            {
                Text = text,
                Color = color ?? Theme.Text,
                Born = Time.unscaledTime
            });

            while (List.Count > 6) List.RemoveAt(0);
            EventLog.Add(text, LogKind.Info);
        }

        public static void Clear() => List.Clear();

        public static int Count => List.Count;

        public static void Draw()
        {
            if (List.Count == 0) return;

            Theme.Ensure();
            var now = Time.unscaledTime;
            List.RemoveAll(t => now - t.Born > t.Life);
            if (List.Count == 0) return;

            _style ??= new GUIStyle(Theme.Label) { fontSize = 11, wordWrap = false };

            const float width = 250f;
            const float height = 30f;
            const float pad = 6f;
            const float right = 18f;
            const float top = 18f;

            var index = 0;
            foreach (var t in List)
            {
                var age = now - t.Born;
                var fade = Mathf.Clamp01((t.Life - age) / 0.7f);
                var slide = Mathf.Clamp01(age / 0.18f);
                var x = Screen.width - width - right + (1f - slide) * 24f;
                var y = top + index * (height + pad);

                var bg = Theme.Alpha(Theme.Panel, 0.96f * fade);
                Theme.Fill(new Rect(x, y, width, height), bg);
                Theme.Fill(new Rect(x, y, 3f, height), Theme.Alpha(t.Color, fade));

                var old = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, fade);
                GUI.Label(new Rect(x + 10f, y + 4f, width - 16f, height - 8f), t.Text, _style);
                GUI.color = old;

                index++;
            }
        }
    }
}
