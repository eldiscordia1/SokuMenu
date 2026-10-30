using System;
using UnityEngine;

namespace SidaMenu
{
    /// <summary>Colours, textures and lazily built GUIStyles for the whole menu.</summary>
    internal static class Theme
    {
        public static readonly Color BaseAccent = new Color(0.62f, 0.31f, 0.94f, 1f);

        public static readonly Color Bg = new Color(0.067f, 0.071f, 0.090f, 0.98f);
        public static readonly Color Panel = new Color(0.106f, 0.114f, 0.145f, 1f);
        public static readonly Color PanelAlt = new Color(0.137f, 0.147f, 0.188f, 1f);
        public static readonly Color Line = new Color(1f, 1f, 1f, 0.07f);
        public static readonly Color Text = new Color(0.906f, 0.918f, 0.949f, 1f);
        public static readonly Color Muted = new Color(0.545f, 0.573f, 0.639f, 1f);
        public static readonly Color Danger = new Color(0.945f, 0.325f, 0.361f, 1f);
        public static readonly Color Good = new Color(0.337f, 0.847f, 0.545f, 1f);
        public static readonly Color Warn = new Color(0.976f, 0.741f, 0.294f, 1f);

        public static GUIStyle Title = null!, Section = null!, Label = null!, Dim = null!,
            Button = null!, ButtonPrimary = null!, ButtonDanger = null!, Tab = null!,
            TabActive = null!, Toggle = null!, Field = null!, Chip = null!, ChipBound = null!,
            Badge = null!, Big = null!;

        private static bool _built;
        private static bool _fillOk = true;

        public static Color Accent
        {
            get
            {
                if (!States.rgbMode) return BaseAccent;

                float t;
                try { t = Time.unscaledTime; }
                catch { return BaseAccent; }

                return Hsv(t * 0.12f, 0.62f, 1f);
            }
        }

        // ------------------------------------------------------------------
        // Pure managed colour helpers. ColorUtility and Color.HSVToRGB are engine
        // methods that may be stripped from the shipped build, and both sat directly
        // in the header draw path, so they are reimplemented here in plain C#.
        // ------------------------------------------------------------------
        public static string Hex(Color c) =>
            "#" + Channel(c.r) + Channel(c.g) + Channel(c.b);

        private static string Channel(float v)
        {
            var i = (int)Math.Round(v * 255f);
            if (i < 0) i = 0;
            if (i > 255) i = 255;
            return i.ToString("X2");
        }

        public static Color Hsv(float h, float s, float v)
        {
            h -= (float)Math.Floor(h);
            var sector = h * 6f;
            var i = (int)sector;
            var f = sector - i;

            var p = v * (1f - s);
            var q = v * (1f - f * s);
            var t = v * (1f - (1f - f) * s);

            switch (i % 6)
            {
                case 0: return new Color(v, t, p);
                case 1: return new Color(q, v, p);
                case 2: return new Color(p, v, t);
                case 3: return new Color(p, q, v);
                case 4: return new Color(t, p, v);
                default: return new Color(v, p, q);
            }
        }

        // The IL2CPP reference assembly only exposes a parameterless RectOffset
        // constructor, so every offset is built field by field.
        private static RectOffset RO(int left, int right, int top, int bottom)
        {
            var o = new RectOffset();
            o.left = left;
            o.right = right;
            o.top = top;
            o.bottom = bottom;
            return o;
        }

        public static void Ensure()
        {
            if (_built) return;
            _built = true;

            try
            {
                Build();
            }
            catch (Exception e)
            {
                Safe.Report("theme styles", e);
                Fallback();
            }
        }

        private static void Fallback()
        {
            Title = Big = Section = Label = Dim = Badge = GUI.skin.label;
            Button = ButtonPrimary = ButtonDanger = Tab = TabActive = Chip = ChipBound = GUI.skin.button;
            Toggle = GUI.skin.toggle;
            Field = GUI.skin.textField;
        }

        private static void Build()
        {
            Title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                richText = true,
                normal = { textColor = Text }
            };

            Big = new GUIStyle(Title) { fontSize = 19 };

            Section = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Muted },
                margin = RO(0, 0, 8, 3)
            };

            Label = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                richText = true,
                wordWrap = true,
                normal = { textColor = Text }
            };

            Dim = new GUIStyle(Label) { fontSize = 11, normal = { textColor = Muted } };

            Button = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                richText = true,
                padding = RO(8, 8, 4, 4),
                normal = { background = Tex(PanelAlt), textColor = Text },
                hover = { background = Tex(Light(PanelAlt, 0.07f)), textColor = Text },
                active = { background = Tex(Light(PanelAlt, 0.14f)), textColor = Text },
                onNormal = { background = Tex(BaseAccent), textColor = Color.white }
            };

            ButtonPrimary = new GUIStyle(Button)
            {
                normal = { background = Tex(Color.Lerp(BaseAccent, Color.black, 0.25f)), textColor = Color.white },
                hover = { background = Tex(BaseAccent), textColor = Color.white },
                active = { background = Tex(Color.Lerp(BaseAccent, Color.black, 0.4f)), textColor = Color.white }
            };

            ButtonDanger = new GUIStyle(Button)
            {
                normal = { background = Tex(Color.Lerp(Danger, Color.black, 0.45f)), textColor = Text },
                hover = { background = Tex(Danger), textColor = Color.white },
                active = { background = Tex(Color.Lerp(Danger, Color.black, 0.3f)), textColor = Color.white }
            };

            Tab = new GUIStyle(Button)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                padding = RO(12, 8, 4, 4),
                margin = RO(0, 0, 2, 2),
                normal = { background = null, textColor = Muted },
                hover = { background = Tex(PanelAlt), textColor = Text },
                active = { background = Tex(PanelAlt), textColor = Text }
            };

            TabActive = new GUIStyle(Tab)
            {
                fontStyle = FontStyle.Bold,
                normal = { background = Tex(PanelAlt), textColor = Color.white },
                hover = { background = Tex(PanelAlt), textColor = Color.white }
            };

            Toggle = new GUIStyle(GUI.skin.toggle)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                richText = true,
                fixedHeight = 21,
                normal = { textColor = Text },
                hover = { textColor = Color.white },
                onNormal = { textColor = Color.white }
            };

            Field = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 12,
                padding = RO(6, 6, 4, 4),
                normal = { background = Tex(PanelAlt), textColor = Text },
                focused = { background = Tex(Light(PanelAlt, 0.05f)), textColor = Text },
                hover = { background = Tex(Light(PanelAlt, 0.05f)), textColor = Text }
            };

            Chip = new GUIStyle(Button)
            {
                fontSize = 9,
                padding = RO(4, 4, 1, 1),
                margin = RO(2, 0, 0, 0),
                normal = { background = Tex(new Color(1f, 1f, 1f, 0.06f)), textColor = Muted },
                hover = { background = Tex(new Color(1f, 1f, 1f, 0.14f)), textColor = Text }
            };

            ChipBound = new GUIStyle(Chip)
            {
                normal = { background = Tex(Color.Lerp(BaseAccent, Color.black, 0.4f)), textColor = Color.white },
                hover = { background = Tex(BaseAccent), textColor = Color.white }
            };

            Badge = new GUIStyle(Label) { fontSize = 10 };
        }

        private static Texture2D? _white;

        /// <summary>A 1x1 texture of the given colour, used for every filled rect.</summary>
        public static Texture2D Tex(Color c)
        {
            var t = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            t.SetPixel(0, 0, c);
            t.Apply();
            t.hideFlags = HideFlags.HideAndDontSave;
            return t;
        }

        public static Texture2D? White
        {
            get
            {
                if (_white != null) return _white;
                try { _white = Tex(Color.white); }
                catch (Exception e) { Safe.Report("texture", e); }
                return _white;
            }
        }

        public static Color Light(Color c, float amount) => Color.Lerp(c, Color.white, amount);

        public static Color Alpha(Color c, float a) => new Color(c.r, c.g, c.b, a);

        /// <summary>
        /// Fill a rect with a flat colour. If the engine side is unavailable the fills
        /// silently stop instead of taking the whole GUI pass down with them.
        /// </summary>
        public static void Fill(Rect r, Color c)
        {
            if (!_fillOk) return;

            try
            {
                var tex = White;
                if (tex == null) { _fillOk = false; return; }

                var old = GUI.color;
                GUI.color = c;
                GUI.DrawTexture(r, tex);
                GUI.color = old;
            }
            catch (Exception e)
            {
                _fillOk = false;
                Safe.Report("rect fill", e);
            }
        }

        public static void LineH(Rect r) => Fill(new Rect(r.x, r.y, r.width, 1f), Line);
    }
}
