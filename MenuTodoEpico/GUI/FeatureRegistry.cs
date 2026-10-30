using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SidaMenu
{
    /// <summary>A single on/off feature, described once and consumed everywhere.</summary>
    public sealed class BoolFeature
    {
        public string Tab = "";
        public string Section = "";
        public string Label = "";
        public string Tip = "";
        public string Field = "";

        /// <summary>Only meaningful while the local client is the host.</summary>
        public bool HostOnly;

        /// <summary>Rendered in red because it can ruin a lobby or get us kicked.</summary>
        public bool Dangerous;

        private FieldInfo _info = null!;

        public bool Value
        {
            get => (bool)(_info.GetValue(null) ?? false);
            set => _info.SetValue(null, value);
        }

        public void Bind()
        {
            _info = typeof(States).GetField(Field, BindingFlags.Public | BindingFlags.Static)
                    ?? throw new MissingFieldException(nameof(States), Field);
        }

        public void Toggle()
        {
            Value = !Value;
            Notification.Push($"{Label}: {(Value ? "ON" : "OFF")}", Value ? Theme.Accent : Theme.Muted);
            EventLog.Add($"{Label} -> {(Value ? "on" : "off")}", LogKind.Action);
        }
    }

    /// <summary>
    /// Central catalog of every toggle. The UI renders straight from this list, and the
    /// search, keybind and profile systems reuse it, so adding a feature is one line here
    /// plus one field in <see cref="States"/>.
    /// </summary>
    public static class Features
    {
        public static readonly List<BoolFeature> All = new List<BoolFeature>();
        public static readonly List<string> Tabs = new List<string>();

        private static readonly Dictionary<string, BoolFeature> ByFieldName =
            new Dictionary<string, BoolFeature>();

        public static void Register(string tab, string section, string label, string field,
            string tip = "", bool hostOnly = false, bool dangerous = false)
        {
            var f = new BoolFeature
            {
                Tab = tab, Section = section, Label = label, Field = field,
                Tip = tip, HostOnly = hostOnly, Dangerous = dangerous
            };
            f.Bind();
            All.Add(f);
            ByFieldName[field] = f;
            if (!Tabs.Contains(tab)) Tabs.Add(tab);
        }

        public static BoolFeature? ByField(string field) =>
            ByFieldName.TryGetValue(field, out var f) ? f : null;

        public static IEnumerable<BoolFeature> InSection(string tab, string section) =>
            All.Where(f => f.Tab == tab && f.Section == section);

        public static IEnumerable<BoolFeature> InTab(string tab) =>
            All.Where(f => f.Tab == tab);

        /// <summary>Case-insensitive match against label, section and tooltip.</summary>
        public static IEnumerable<BoolFeature> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<BoolFeature>();
            var q = query.Trim();
            return All.Where(f =>
                f.Label.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                f.Section.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                f.Tip.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary>Populated once at plugin load.</summary>
        public static void BuildCatalog()
        {
            if (All.Count > 0) return;

            // ---------------- Self ----------------
            Register("Self", "Movement", "No Clip", nameof(States.noClip),
                "Walk through walls and objects. Client sided.");
            Register("Self", "Movement", "Teleport To Cursor", nameof(States.tpToCursor),
                "Right click anywhere to instantly snap to that position.");
            Register("Self", "Movement", "Free Camera", nameof(States.freecam),
                "Detach the camera from your body and fly around with WASD.");
            Register("Self", "Movement", "Zoom Out", nameof(States.zoomOut),
                "Increase camera size. Mouse wheel while enabled changes the zoom.");

            Register("Self", "Vision", "Full Bright", nameof(States.fullBright),
                "Ignores the light radius, so the whole map is always lit.");
            Register("Self", "Vision", "See Ghosts", nameof(States.seeGhosts),
                "Renders dead players instead of hiding them.");
            Register("Self", "Vision", "No Shadows", nameof(States.noShadows),
                "Removes the darkness overlay the game draws over the map.");
            Register("Self", "Chat", "Always Chat", nameof(States.alwaysChat),
                "Keeps the chat input available even when the game hides it.");

            Register("Self", "Abilities", "No Kill Cooldown", nameof(States.noKillCooldown),
                "Kill timer is forced to zero. Most reliable while hosting.");
            Register("Self", "Abilities", "Unlock Vents", nameof(States.unlockVents),
                "Use any vent from any distance, as any role.");
            Register("Self", "Abilities", "Move While Venting", nameof(States.moveWhileVenting),
                "Keeps you mobile after entering a vent.");

            // ---------------- Host ----------------
            Register("Host", "Lobby", "Max Players", nameof(States.morePlayers),
                "Push the lobby player cap past the vanilla maximum.", hostOnly: true);
            Register("Host", "Lobby", "Block Game End", nameof(States.noEndGame),
                "Stops the game from ending the round.", hostOnly: true, dangerous: true);

            // ---------------- Misc ----------------
            Register("Misc", "Interface", "RGB Menu", nameof(States.rgbMode),
                "Cycles the menu accent colour through the rainbow.");
            Register("Misc", "Safety", "Anti Ban", nameof(States.antiBan),
                "Keeps your ban points at zero and blocks the report penalty building up.");
            Register("Misc", "Safety", "No Telemetry", nameof(States.noTelemetry),
                "Disables Unity analytics and performance reporting.");

            Register("Misc", "Spoofing", "Spoof Platform", nameof(States.spoofPlatform),
                "Other players see you as a different platform.");
        }
    }
}
