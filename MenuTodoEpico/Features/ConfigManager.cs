using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;

namespace SidaMenu
{
    /// <summary>
    /// Saves and restores the whole menu state (toggles, floats, keybinds) to a
    /// plain text profile so a setup survives restarts.
    /// </summary>
    internal static class ConfigManager
    {
        public static string Path => System.IO.Path.Combine(Paths.ConfigPath, "SidaMenu_Profile.txt");

        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.Static;

        public static void Save()
        {
            try
            {
                var lines = new List<string>
                {
                    "# SidaMenu profile - key=value, one per line",
                    "# Toggles: bool.<field>=True|False",
                    "# Numbers: float.<field>=1.5 / int.<field>=3",
                    "# Keybinds: key.<field>=F1"
                };

                foreach (var f in Features.All)
                    lines.Add($"bool.{f.Field}={f.Value}");

                foreach (var fi in typeof(States).GetFields(Flags))
                {
                    if (fi.FieldType == typeof(float))
                        lines.Add($"float.{fi.Name}={((float)(fi.GetValue(null) ?? 0f)).ToString(CultureInfo.InvariantCulture)}");
                    else if (fi.FieldType == typeof(int))
                        lines.Add($"int.{fi.Name}={fi.GetValue(null)}");
                    else if (fi.FieldType == typeof(string))
                        lines.Add($"str.{fi.Name}={fi.GetValue(null)}");
                }

                foreach (var kv in Keybinds.Map)
                    lines.Add($"key.{kv.Key}={kv.Value}");

                File.WriteAllLines(Path, lines);
                Notification.Push("Profile saved", Theme.Good);
                EventLog.Add($"Saved profile to {Path}", LogKind.Action);
            }
            catch (System.Exception e)
            {
                Notification.Push("Save failed: " + e.Message, Theme.Danger);
                EventLog.Add("Save failed: " + e, LogKind.Danger);
            }
        }

        public static void Load()
        {
            try
            {
                if (!File.Exists(Path))
                {
                    Notification.Push("No profile saved yet", Theme.Warn);
                    return;
                }

                foreach (var raw in File.ReadAllLines(Path))
                {
                    var line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith("#")) continue;

                    var dot = line.IndexOf('.');
                    var eq = line.IndexOf('=');
                    if (dot < 0 || eq < 0) continue;

                    var kind = line.Substring(0, dot);
                    var name = line.Substring(dot + 1, eq - dot - 1);
                    var value = line.Substring(eq + 1);

                    switch (kind)
                    {
                        case "bool":
                            var feature = Features.ByField(name);
                            if (feature != null) feature.Value = value.Trim().Equals("true", System.StringComparison.OrdinalIgnoreCase);
                            break;

                        case "float":
                            var ff = typeof(States).GetField(name, Flags);
                            if (ff != null && float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var fv))
                                ff.SetValue(null, fv);
                            break;

                        case "int":
                            var iff = typeof(States).GetField(name, Flags);
                            if (iff != null && int.TryParse(value, out var iv)) iff.SetValue(null, iv);
                            break;

                        case "str":
                            var sf = typeof(States).GetField(name, Flags);
                            if (sf != null && sf.FieldType == typeof(string)) sf.SetValue(null, value);
                            break;

                        case "key":
                            if (System.Enum.TryParse<UnityEngine.KeyCode>(value.Trim(), true, out var kc))
                                Keybinds.Set(name, kc);
                            break;
                    }
                }

                Notification.Push("Profile loaded", Theme.Good);
                EventLog.Add($"Loaded profile from {Path}", LogKind.Action);
            }
            catch (System.Exception e)
            {
                Notification.Push("Load failed: " + e.Message, Theme.Danger);
                EventLog.Add("Load failed: " + e, LogKind.Danger);
            }
        }

        private static bool _loadedOnStart;

        public static void LoadOnStart()
        {
            if (_loadedOnStart) return;
            _loadedOnStart = true;

            if (!File.Exists(Path)) return;
            Load();

            // Startup restore should not spam the player with toasts.
            Notification.Clear();
        }

        public static void ResetDefaults()
        {
            States.ResetAll();
            Keybinds.Map.Clear();
            Notification.Push("Settings reset", Theme.Warn);
        }
    }
}
