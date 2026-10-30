using System.Collections.Generic;
using UnityEngine;

namespace SidaMenu
{
    /// <summary>
    /// Every mutable setting the menu owns.
    ///
    /// The config, keybind and search systems enumerate the <c>public static bool</c>
    /// fields declared here through reflection, so a toggle only has to be declared
    /// once and registered in <see cref="Features"/> to become searchable, savable
    /// and bindable. Keep names unique and stable - they are used as file keys.
    /// </summary>
    public static class States
    {
        // ------------------------------------------------------------------
        // Self -> Movement
        // ------------------------------------------------------------------
        public static bool noClip;
        public static bool tpToCursor;
        public static bool freecam;

        /// <summary>Multiplier applied to <c>PlayerPhysics.TrueSpeed</c>.</summary>
        public static float speed = 1.0f;

        /// <summary>Extra zoom applied to the main camera while <see cref="zoomOut"/> is on.</summary>
        public static bool zoomOut;
        public static float zoomLevel = 6.5f;

        public static float freecamSpeed = 10f;

        // ------------------------------------------------------------------
        // Self -> Vision
        // ------------------------------------------------------------------
        public static bool fullBright;
        public static bool seeGhosts;
        public static bool alwaysChat;
        public static bool noShadows;

        // ------------------------------------------------------------------
        // Self -> Abilities
        // ------------------------------------------------------------------
        public static bool noKillCooldown;
        public static bool unlockVents;
        public static bool moveWhileVenting;

        // ------------------------------------------------------------------
        // Host
        // ------------------------------------------------------------------
        public static bool morePlayers;
        public static bool noEndGame;

        /// <summary>Player count used by <see cref="morePlayers"/>.</summary>
        public static int maxPlayers = 255;

        // ------------------------------------------------------------------
        // Misc / cosmetic
        // ------------------------------------------------------------------
        public static bool rgbMode;
        public static bool antiBan = true;
        public static bool noTelemetry = true;

        // ------------------------------------------------------------------
        // Spoofing (kept as fields so the registry can toggle them)
        // ------------------------------------------------------------------
        public static bool spoofPlatform;
        public static bool spoofName;

        // ------------------------------------------------------------------
        // Plain values (not toggles)
        // ------------------------------------------------------------------
        public static string spamMessage = "SidaMenu on top!";
        public static string fakeName = "Sida";

        /// <summary>KeyCode currently waiting to be bound, or null when idle.</summary>
        public static string? bindingField;

        /// <summary>True while the user is typing in any text field.</summary>
        public static bool typing;

        public static readonly List<string> Warnings = new List<string>();

        public static void ResetAll()
        {
            foreach (var f in typeof(States).GetFields(
                         System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
            {
                if (f.FieldType == typeof(bool)) f.SetValue(null, false);
            }

            // Sensible defaults restored after the blanket reset.
            speed = 1.0f;
            zoomLevel = 6.5f;
            freecamSpeed = 10f;
            maxPlayers = 255;
            antiBan = true;
            noTelemetry = true;
            spoofName = false;
            spoofPlatform = false;
        }
    }

    /// <summary>Keyboard shortcut storage, keyed by the States field name.</summary>
    public static class Keybinds
    {
        public static readonly Dictionary<string, KeyCode> Map = new Dictionary<string, KeyCode>();

        public static KeyCode Get(string field) => Map.TryGetValue(field, out var k) ? k : KeyCode.None;

        public static void Set(string field, KeyCode key)
        {
            if (key == KeyCode.None) Map.Remove(field);
            else Map[field] = key;
        }

        public static string Label(string field)
        {
            var k = Get(field);
            return k == KeyCode.None ? "bind" : Pretty(k);
        }

        /// <summary>Compact key name for the small chips next to each toggle.</summary>
        public static string Pretty(KeyCode key)
        {
            var s = key.ToString();
            if (s.StartsWith("Alpha")) s = s.Substring(5);
            if (s.StartsWith("Keypad")) s = "Num" + s.Substring(6);
            switch (s)
            {
                case "LeftControl": return "LCTRL";
                case "RightControl": return "RCTRL";
                case "LeftShift": return "LSHFT";
                case "RightShift": return "RSHFT";
                case "LeftAlt": return "LALT";
                case "RightAlt": return "RALT";
                case "Escape": return "ESC";
                case "Delete": return "DEL";
                case "Insert": return "INS";
                case "Return": return "ENTER";
                case "BackQuote": return "`";
                case "Mouse0": return "LMB";
                case "Mouse1": return "RMB";
                case "Mouse2": return "MMB";
                default: return s.ToUpperInvariant();
            }
        }
    }
}
