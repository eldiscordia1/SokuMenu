using System;
using UnityEngine;

namespace SidaMenu
{
    /// <summary>Small IMGUI helpers so the tab code stays readable.</summary>
    internal static class W
    {
        public const float Row = 22f;

        public static string? Hover;

        // Widgets disable themselves the first time the engine side fails, so a
        // stripped method degrades one control instead of the whole menu.
        private static bool _toggleOk = true;
        private static bool _sliderOk = true;
        private static bool _buttonOk = true;
        private static bool _fieldOk = true;

        public static void Section(string title) =>
            GUILayout.Label(title.ToUpperInvariant(), Theme.Section);

        public static void Space(float px = 8f) => GUILayout.Space(px);

        public static void Hint(string text) => GUILayout.Label(text, Theme.Dim);

        public static void Text(string text) => GUILayout.Label(text, Theme.Label);

        private static void Measure(string tip)
        {
            if (string.IsNullOrEmpty(tip)) return;
            try
            {
                var r = GUILayoutUtility.GetLastRect();
                if (r.Contains(Event.current.mousePosition)) Hover = tip;
            }
            catch { }
        }

        /// <summary>Draws a checkbox. Pass a tooltip to show explanatory text on hover.</summary>
        public static bool Toggle(string label, bool value, string tip = "")
        {
            if (!_toggleOk)
            {
                GUILayout.Label(label, Theme.Label);
                return value;
            }

            var v = value;
            GUILayout.BeginHorizontal(GUILayout.Height(Row));
            try
            {
                v = GUILayout.Toggle(value, " " + label, Theme.Toggle, GUILayout.Height(Row));
                GUILayout.FlexibleSpace();
                Measure(tip);
            }
            catch (Exception e)
            {
                _toggleOk = false;
                Safe.Report("toggle widget", e);
            }
            finally
            {
                try { GUILayout.EndHorizontal(); } catch { }
            }

            return v;
        }

        /// <summary>A toggle row with a hotkey chip on the right.</summary>
        public static bool ToggleWithKey(string label, string field, bool value, string tip = "",
            bool enabled = true)
        {
            if (!_toggleOk)
            {
                GUILayout.Label(label, Theme.Label);
                return value;
            }

            var v = value;
            GUILayout.BeginHorizontal(GUILayout.Height(Row));
            try
            {
                var old = GUI.enabled;
                GUI.enabled = enabled;
                v = GUILayout.Toggle(value, " " + label, Theme.Toggle, GUILayout.Height(Row));
                GUI.enabled = old;

                GUILayout.FlexibleSpace();

                var key = Keybinds.Get(field);
                var chip = key == KeyCode.None ? Keybinds.Label(field) : Keybinds.Pretty(key);

                if (States.bindingField == field)
                {
                    if (Event.current.type == EventType.KeyDown)
                    {
                        var pressed = Event.current.keyCode;
                        if (pressed == KeyCode.Escape) Keybinds.Set(field, KeyCode.None);
                        else if (pressed != KeyCode.None) Keybinds.Set(field, pressed);
                        States.bindingField = null;
                        Event.current.Use();
                    }

                    chip = "press...";
                }

                if (GUILayout.Button(chip, key == KeyCode.None ? Theme.Chip : Theme.ChipBound,
                        GUILayout.Width(48f), GUILayout.Height(16f)))
                {
                    States.bindingField = States.bindingField == field ? null : field;
                }

                Measure(tip);
            }
            catch (Exception e)
            {
                _toggleOk = false;
                Safe.Report("toggle widget", e);
            }
            finally
            {
                try { GUILayout.EndHorizontal(); } catch { }
            }

            return v;
        }

        public static float Slider(string label, float value, float min, float max, string format = "0.00",
            string tip = "", float resetTo = float.NaN)
        {
            if (!_sliderOk)
            {
                GUILayout.Label($"{label}: {value.ToString(format)}", Theme.Dim);
                return value;
            }

            var v = value;
            GUILayout.BeginHorizontal(GUILayout.Height(18f));
            try
            {
                GUILayout.Label($"{label}  <color={Theme.Hex(Theme.Accent)}>{value.ToString(format)}</color>",
                    Theme.Label, GUILayout.Width(190f));

                v = GUILayout.HorizontalSlider(value, min, max, GUILayout.Height(14f));

                if (GUILayout.Button("R", Theme.Chip, GUILayout.Width(18f), GUILayout.Height(15f))
                    && !float.IsNaN(resetTo))
                {
                    v = resetTo;
                }

                Measure(tip);
            }
            catch (Exception e)
            {
                _sliderOk = false;
                Safe.Report("slider widget", e);
            }
            finally
            {
                try { GUILayout.EndHorizontal(); } catch { }
            }

            return v;
        }

        public static bool Button(string label, float width = 0f, float height = 26f, bool primary = false,
            bool danger = false)
        {
            if (!_buttonOk)
            {
                GUILayout.Label(label, Theme.Dim);
                return false;
            }

            try
            {
                var style = danger ? Theme.ButtonDanger : primary ? Theme.ButtonPrimary : Theme.Button;
                var options = new[]
                {
                    GUILayout.Height(height),
                    width > 0f ? GUILayout.Width(width) : GUILayout.ExpandWidth(true)
                };

                return GUILayout.Button(label, style, options);
            }
            catch (Exception e)
            {
                _buttonOk = false;
                Safe.Report("button widget", e);
                return false;
            }
        }

        /// <summary>A labelled single-line text field.</summary>
        public static string TextField(string label, string value, string tip = "")
        {
            if (!_fieldOk) return value;

            var v = value;
            GUILayout.BeginHorizontal(GUILayout.Height(Row));
            try
            {
                GUILayout.Label(label, Theme.Label, GUILayout.Width(80f));
                v = GUILayout.TextField(value ?? "", 120, Theme.Field, GUILayout.Height(20f));
                Measure(tip);
            }
            catch (Exception e)
            {
                _fieldOk = false;
                Safe.Report("text field widget", e);
            }
            finally
            {
                try { GUILayout.EndHorizontal(); } catch { }
            }

            return v;
        }

        public static void Divider()
        {
            GUILayout.Space(6f);
            try
            {
                var r = GUILayoutUtility.GetRect(1f, 1f, GUILayout.ExpandWidth(true));
                Theme.LineH(r);
            }
            catch (Exception e) { Safe.Report("divider", e); }

            GUILayout.Space(6f);
        }

        /// <summary>Draws a feature toggle straight from the catalog.</summary>
        public static void Feature(BoolFeature f)
        {
            var hostBlocked = f.HostOnly && !Utils.IsHost && !Utils.IsFreePlay;

            var old = GUI.enabled;
            try
            {
                GUI.enabled = !hostBlocked;

                var label = f.Dangerous
                    ? $"<color={Theme.Hex(Theme.Danger)}>{f.Label}</color>"
                    : f.Label;

                var tip = hostBlocked
                    ? f.Tip + "\n\n(Host only - you are not the host)"
                    : f.Tip;

                var value = ToggleWithKey(label, f.Field, f.Value, tip);
                if (value != f.Value) f.Value = value;
            }
            finally
            {
                GUI.enabled = old;
            }
        }
    }
}
