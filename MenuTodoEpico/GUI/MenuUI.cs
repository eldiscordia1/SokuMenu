using System;
using System.Linq;
using UnityEngine;

namespace SidaMenu
{
    public partial class MenuUI : MonoBehaviour
    {
        public static bool showMenu = true;

        public Rect windowRect = new Rect(60, 60, 800, 560);

        public string version = "v2.0.1";

        private static readonly string[] TabNames =
            { "Self", "Host", "Players", "Misc", "Console", "Config", "About" };

        private int _tab;
        private string _search = "";
        private Vector2 _scroll;

        private float _fpsTimer;
        private int _fpsFrames;
        private int _fps;

        private static bool _fieldFocused;
        private bool _dragging;
        private Vector2 _dragOffset;
        private bool _searchOk = true;

        void Start()
        {
            Features.BuildCatalog();
            ConfigManager.LoadOnStart();
            EventLog.Add($"SidaMenu {version} loaded - {Features.All.Count} features", LogKind.Action);
        }

        void Update()
        {
            try { Tick(); }
            catch (Exception e) { Safe.Report("tick", e); }
        }

        // ------------------------------------------------------------------
        // Frame loop
        // ------------------------------------------------------------------
        private void Tick()
        {
            _fpsTimer += Time.unscaledDeltaTime;
            _fpsFrames++;
            if (_fpsTimer >= 0.5f)
            {
                _fps = (int)Math.Round(_fpsFrames / _fpsTimer);
                _fpsTimer = 0f;
                _fpsFrames = 0;
            }

            // A widget is capturing a key, so swallow input this frame.
            if (States.bindingField != null) return;

            if (!Utils.ChatOpen && !_fieldFocused)
            {
                if (Input.GetKeyDown(KeyCode.Delete)) showMenu = !showMenu;
                KeybindManager.Tick();
            }

            Safe.Run("self features", SelfFeatures.Tick);
            Safe.Run("misc features", MiscFeatures.Tick);
        }

        // ------------------------------------------------------------------
        // Rendering
        // ------------------------------------------------------------------
        void OnGUI()
        {
            try
            {
                Theme.Ensure();
                W.Hover = null;

                if (showMenu)
                {
                    HandleDrag();

                    // BeginGroup/EndGroup push and pop a GUI clip. If anything throws in
                    // between, EndGroup must still run or IMGUI's clip stack desyncs and
                    // floods the console with "pushing more GUIClips than popping".
                    var grouped = false;
                    try
                    {
                        GUI.BeginGroup(windowRect);
                        grouped = true;
                        DrawMenu();
                    }
                    catch (Exception e)
                    {
                        Safe.Report("menu draw", e);
                    }
                    finally
                    {
                        if (grouped)
                        {
                            try { GUI.EndGroup(); }
                            catch (Exception e) { Safe.Report("menu draw", e); }
                        }
                    }
                }

                Safe.Run("notifications", Notification.Draw);
                Safe.Run("tooltip", DrawTooltip);

                var focused = false;
                try { focused = GUIUtility.keyboardControl != 0; }
                catch (Exception e) { Safe.Report("focus probe", e); }

                // Only block hotkeys while the menu is actually open and a field has
                // focus, otherwise a closed menu could swallow DELETE forever.
                _fieldFocused = showMenu && focused;
            }
            catch (Exception e)
            {
                Safe.Report("gui", e);
            }
        }

        /// <summary>
        /// The menu is drawn into a GUI group instead of a GUI.Window so it has no title
        /// bar. Dragging is handled here, from the header strip only.
        /// </summary>
        private void HandleDrag()
        {
            try
            {
                var e = Event.current;
                var header = new Rect(windowRect.x, windowRect.y, windowRect.width, 48f);

                if (e.type == EventType.MouseDown && e.button == 0 && header.Contains(e.mousePosition))
                {
                    _dragging = true;
                    _dragOffset = e.mousePosition - windowRect.position;
                    e.Use();
                    return;
                }

                if (_dragging && e.type == EventType.MouseDrag)
                {
                    windowRect.position = e.mousePosition - _dragOffset;
                    e.Use();
                    return;
                }

                if (e.type == EventType.MouseUp) _dragging = false;
            }
            catch (Exception e)
            {
                Safe.Report("drag", e);
            }
        }

        /// <summary>Runs a draw step inside a GUILayout area that is always closed.</summary>
        private static void Area(Rect rect, string name, Action draw)
        {
            var open = false;
            try
            {
                GUILayout.BeginArea(rect);
                open = true;
            }
            catch (Exception e)
            {
                Safe.Report(name + " area", e);
            }

            if (!open) return;

            try
            {
                Safe.Run(name, draw);
            }
            finally
            {
                try { GUILayout.EndArea(); }
                catch (Exception e) { Safe.Report(name + " area", e); }
            }
        }

        private void DrawMenu()
        {
            var w = windowRect.width;
            var h = windowRect.height;
            var bodyHeight = Math.Max(h - 86f, 1f);

            Theme.Fill(new Rect(0f, 0f, w, h), Theme.Bg);
            Theme.Fill(new Rect(0f, 0f, w, 2f), Theme.Accent);

            Safe.Run("header", () =>
            {
                GUI.Label(new Rect(16f, 7f, 340f, 22f), "<b>SIDAMENU</b>", Theme.Big);
                GUI.Label(new Rect(16f, 27f, 460f, 16f), StatusLine(), Theme.Dim);
            });

            DrawSearchBox(w);

            Theme.LineH(new Rect(0f, 48f, w, 1f));

            Area(new Rect(8f, 56f, 140f, bodyHeight), "sidebar", DrawSidebar);
            Area(new Rect(152f, 56f, Math.Max(w - 160f, 1f), bodyHeight), "content", DrawContent);

            Theme.LineH(new Rect(0f, h - 27f, w, 1f));

            Safe.Run("footer", () =>
                GUI.Label(new Rect(12f, h - 24f, 600f, 20f), FooterLine(), Theme.Dim));

            Safe.Run("close button", () =>
            {
                if (GUI.Button(new Rect(w - 96f, h - 26f, 84f, 20f), "Close [DEL]", Theme.Chip))
                    showMenu = false;
            });
        }

        private void DrawSearchBox(float w)
        {
            if (!_searchOk) return;

            _searchOk = Safe.Run("search box", () =>
            {
                var next = GUI.TextField(new Rect(w - 266f, 14f, 204f, 22f), _search, 48, Theme.Field);
                if (next != _search)
                {
                    _search = next;
                    _scroll = Vector2.zero;
                }

                if (GUI.Button(new Rect(w - 58f, 14f, 42f, 22f), "clear", Theme.Chip))
                {
                    _search = "";
                    _scroll = Vector2.zero;
                }
            });
        }

        private void DrawSidebar()
        {
            for (var i = 0; i < TabNames.Length; i++)
            {
                var active = _tab == i && string.IsNullOrEmpty(_search);
                if (GUILayout.Button("  " + TabNames[i], active ? Theme.TabActive : Theme.Tab,
                        GUILayout.Height(30f)))
                {
                    _tab = i;
                    _search = "";
                    _scroll = Vector2.zero;
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label($"<color={Theme.Hex(Theme.Muted)}>{Features.All.Count} features</color>", Theme.Dim);
        }

        private void DrawContent()
        {
            var scrollOpen = false;
            try
            {
                _scroll = GUILayout.BeginScrollView(_scroll);
                scrollOpen = true;
            }
            catch (Exception e)
            {
                Safe.Report("scroll view", e);
            }

            try
            {
                if (!string.IsNullOrEmpty(_search))
                {
                    Safe.Run("search results", DrawSearch);
                    return;
                }

                switch (_tab)
                {
                    case 0: Safe.Run("Self tab", DrawSelf); break;
                    case 1: Safe.Run("Host tab", DrawHost); break;
                    case 2: Safe.Run("Players tab", DrawPlayers); break;
                    case 3: Safe.Run("Misc tab", DrawMisc); break;
                    case 4: Safe.Run("Console tab", DrawConsole); break;
                    case 5: Safe.Run("Config tab", DrawConfig); break;
                    default: Safe.Run("About tab", DrawAbout); break;
                }
            }
            finally
            {
                if (scrollOpen)
                {
                    try { GUILayout.EndScrollView(); }
                    catch (Exception e) { Safe.Report("scroll view", e); }
                }
            }
        }

        private static void Registry(string tab, string section)
        {
            foreach (var f in Features.InSection(tab, section)) W.Feature(f);
        }

        // ------------------------------------------------------------------
        // Tabs
        // ------------------------------------------------------------------
        private void DrawSelf()
        {
            W.Section("Movement");
            States.speed = W.Slider("Speed", States.speed, 1f, 15f, "0.00x",
                "Multiplies your movement speed.", 1f);
            Registry("Self", "Movement");
            States.zoomLevel = W.Slider("Zoom", States.zoomLevel, 3.5f, 18f, "0.0",
                "Camera size used while Zoom Out is on. Mouse wheel adjusts it in game.", 6.5f);
            States.freecamSpeed = W.Slider("Freecam Speed", States.freecamSpeed, 2f, 40f, "0.0",
                "How fast the detached camera flies.", 10f);

            W.Section("Vision");
            Registry("Self", "Vision");

            W.Section("Chat");
            Registry("Self", "Chat");

            W.Section("Abilities");
            Registry("Self", "Abilities");

            W.Divider();
            GUILayout.BeginHorizontal();
            if (W.Button("Exit All Vents")) HostFeatures.ExitAllVents();
            if (W.Button("Copy Lobby Code")) MiscFeatures.CopyLobbyCode();
            GUILayout.EndHorizontal();
        }

        private void DrawHost()
        {
            W.Section("Lobby");
            Registry("Host", "Lobby");
            States.maxPlayers = (int)Math.Round(W.Slider("Player cap", States.maxPlayers, 10f, 255f, "0",
                "Applied while Max Players is enabled.", 255f));

            GUILayout.BeginHorizontal();
            if (W.Button("Force Start", primary: true)) HostFeatures.ForceStart();
            if (W.Button("Force End", danger: true)) HostFeatures.ForceEnd();
            GUILayout.EndHorizontal();

            W.Section("Ship");
            GUILayout.BeginHorizontal();
            if (W.Button("Reset Tasks")) HostFeatures.ResetTasks();
            if (W.Button("Repair All Sabotages")) HostFeatures.RepairAll();
            GUILayout.EndHorizontal();

            W.Section("Sabotage");
            GUILayout.BeginHorizontal();
            if (W.Button("Lights")) HostFeatures.Sabotage(SystemTypes.Electrical, "Lights");
            if (W.Button("Reactor")) HostFeatures.Sabotage(SystemTypes.Reactor, "Reactor");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (W.Button("Oxygen")) HostFeatures.Sabotage(SystemTypes.LifeSupp, "Oxygen");
            if (W.Button("Comms")) HostFeatures.Sabotage(SystemTypes.Comms, "Comms");
            GUILayout.EndHorizontal();

            W.Section("Meetings");
            GUILayout.BeginHorizontal();
            if (W.Button("Call Meeting")) HostFeatures.CallMeeting();
            if (W.Button("Skip Meeting")) HostFeatures.SkipMeeting();
            GUILayout.EndHorizontal();

            W.Section("Lobby control");
            GUILayout.BeginHorizontal();
            if (W.Button("Kill Crewmates", danger: true)) HostFeatures.KillAll(true, false);
            if (W.Button("Kill Impostors", danger: true)) HostFeatures.KillAll(false, true);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (W.Button("Kill Everyone", danger: true)) HostFeatures.KillAll(true, true);
            if (W.Button("Kick Everyone", danger: true)) HostFeatures.KickEveryone();
            GUILayout.EndHorizontal();
        }

        private void DrawPlayers()
        {
            var players = Utils.AllPlayers();
            W.Section($"Players ({players.Count})");
            W.Hint("Actions apply instantly. Kill uses the host path when you are host.");
            W.Space(4f);

            var local = PlayerControl.LocalPlayer;

            foreach (var pc in players)
            {
                if (pc == null) continue;

                var isLocal = local != null && pc.PlayerId == local.PlayerId;
                var dead = pc.Data != null && pc.Data.IsDead;
                var color = Utils.RoleColorOf(pc);
                var dist = Utils.DistanceTo(pc);

                GUILayout.BeginHorizontal(GUILayout.Height(24f));

                var label = $"<b><color={Theme.Hex(color)}>{Utils.NameOf(pc)}</color></b>";
                if (isLocal) label += $"  <color={Theme.Hex(Theme.Muted)}>(you)</color>";
                if (dead) label += $"  <color={Theme.Hex(Theme.Muted)}>dead</color>";

                GUILayout.Label(label, Theme.Label, GUILayout.Width(180f));
                GUILayout.Label(Utils.RoleOf(pc), Theme.Dim, GUILayout.Width(88f));
                GUILayout.Label(isLocal || dist < 0f ? "-" : $"{dist:0.0}m", Theme.Dim,
                    GUILayout.Width(48f));

                if (!isLocal)
                {
                    if (W.Button("TP", 34f, 20f)) PlayerFeatures.TeleportTo(pc);
                    if (W.Button("Kill", 38f, 20f, danger: true)) HostFeatures.Murder(pc);
                    if (W.Button("Report", 48f, 20f)) PlayerFeatures.Report(pc);
                    if (W.Button("Kick", 40f, 20f)) HostFeatures.Kick(pc);
                }

                GUILayout.EndHorizontal();
            }
        }

        private void DrawMisc()
        {
            W.Section("Interface");
            Registry("Misc", "Interface");

            W.Section("Safety");
            Registry("Misc", "Safety");

            W.Section("Trolling");
            GUILayout.BeginHorizontal();
            if (W.Button("Invisible", danger: true)) MiscFeatures.BecomeInvisible();
            if (W.Button("Red Quick Chat", danger: true)) MiscFeatures.RedChat();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (W.Button("Windows Sound Exploit")) MiscFeatures.WindowsNotificationExploit();
            if (W.Button("Promote Menu")) MiscFeatures.Promotion();
            GUILayout.EndHorizontal();

            W.Section("Chat");
            States.spamMessage = W.TextField("Message", States.spamMessage,
                "Text sent by the buttons below.");
            GUILayout.BeginHorizontal();
            if (W.Button("Send Once")) MiscFeatures.SendChat(States.spamMessage);
            if (W.Button("Spam x6", danger: true)) MiscFeatures.SpamChat(States.spamMessage, 6);
            GUILayout.EndHorizontal();

            W.Section("Spoofing");
            Registry("Misc", "Spoofing");
            if (W.Button($"Platform: {SidaSpoofer.Current}")) SidaSpoofer.Cycle();
        }

        private void DrawConsole()
        {
            W.Section($"Event log ({EventLog.Entries.Count})");

            GUILayout.BeginHorizontal();
            if (W.Button("Clear")) EventLog.Clear();
            if (W.Button("Copy")) CopyLog();
            EventLog.Paused = GUILayout.Toggle(EventLog.Paused, " Pause", Theme.Toggle,
                GUILayout.Width(76f));
            GUILayout.EndHorizontal();

            W.Space(4f);

            for (var i = EventLog.Entries.Count - 1; i >= 0; i--)
            {
                var e = EventLog.Entries[i];
                var c = Theme.Hex(EventLog.ColorFor(e.Kind));
                GUILayout.Label(
                    $"<color={Theme.Hex(Theme.Muted)}>[{e.Time}]</color> <color={c}>{EventLog.TagFor(e.Kind)}</color> {e.Text}",
                    Theme.Dim);
            }
        }

        private static void CopyLog()
        {
            var text = string.Join("\n",
                EventLog.Entries.Select(e => $"[{e.Time}] {EventLog.TagFor(e.Kind)} {e.Text}"));

            Utils.CopyToClipboard(text, "log");
        }

        private void DrawConfig()
        {
            W.Section("Profile");
            W.Hint(ConfigManager.Path);
            GUILayout.BeginHorizontal();
            if (W.Button("Save", primary: true)) ConfigManager.Save();
            if (W.Button("Load")) ConfigManager.Load();
            if (W.Button("Reset defaults", danger: true)) ConfigManager.ResetDefaults();
            GUILayout.EndHorizontal();

            W.Section("Keybinds");
            W.Hint("Click the chip next to any toggle to bind it. ESC clears a bind. " +
                   "Hotkeys never fire while chat is open.");

            var bound = Features.All.Where(f => Keybinds.Get(f.Field) != KeyCode.None).ToList();
            if (bound.Count == 0) W.Hint("No hotkeys bound yet.");

            foreach (var f in bound)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(20f));
                GUILayout.Label(f.Label, Theme.Label, GUILayout.Width(220f));
                GUILayout.Label(Keybinds.Pretty(Keybinds.Get(f.Field)), Theme.Dim, GUILayout.Width(70f));
                if (W.Button("Clear", 56f, 18f)) Keybinds.Set(f.Field, KeyCode.None);
                GUILayout.EndHorizontal();
            }

            W.Section("Menu");
            W.Hint("DELETE shows and hides the menu.");
        }

        private void DrawAbout()
        {
            W.Section("SidaMenu");
            W.Text($"<b>Version</b> {version}");
            W.Text("<b>Base</b> BepInEx 6 IL2CPP + Harmony");
            W.Hint("Aims to be the best experience you can get out of an Among Us mod menu. " +
                   "Host tools, client tools, a player list, an event log and configurable hotkeys.");

            W.Space(6f);
            GUILayout.BeginHorizontal();
            if (W.Button("GitHub")) Open("https://github.com/haunt01/SidaMenu");
            if (W.Button("Discord")) Open("https://discord.gg/gaQvGxK5F8");
            GUILayout.EndHorizontal();

            W.Section("Session");
            W.Hint($"Features: {Features.All.Count}");
            W.Hint($"Players: {Utils.AllPlayers().Count}");
            W.Hint($"Host: {(Utils.IsHost ? "yes" : "no")}   In game: {(Utils.IsShip ? "yes" : "no")}");
            W.Hint($"Ping: {Utils.Ping:0} ms   FPS: {_fps}");
        }

        private static void Open(string url)
        {
            try { Application.OpenURL(url); }
            catch (Exception e) { Safe.Report("open url", e); }
        }

        private void DrawSearch()
        {
            var results = Features.Search(_search).ToList();
            W.Section($"Search \"{_search}\" ({results.Count})");

            if (results.Count == 0)
            {
                W.Hint("Nothing matched. Try 'speed', 'vent', 'spoof' or 'chat'.");
                return;
            }

            string? lastSection = null;
            foreach (var f in results)
            {
                var group = f.Tab + " / " + f.Section;
                if (group != lastSection)
                {
                    if (lastSection != null) W.Space(4f);
                    W.Hint(group);
                    lastSection = group;
                }

                W.Feature(f);
            }
        }

        // ------------------------------------------------------------------
        // Chrome
        // ------------------------------------------------------------------
        private string StatusLine()
        {
            var host = Utils.IsHost
                ? $"<color={Theme.Hex(Theme.Good)}>host</color>"
                : $"<color={Theme.Hex(Theme.Muted)}>client</color>";

            var state = Utils.IsShip
                ? "in game"
                : Utils.InLobby
                    ? "in lobby"
                    : "in menu";

            return $"<color={Theme.Hex(Theme.Muted)}>{version}</color>   {host}   " +
                   $"<color={Theme.Hex(Theme.Muted)}>{state}</color>";
        }

        private string FooterLine()
        {
            var players = Utils.AllPlayers().Count;
            return $"<color={Theme.Hex(Theme.Muted)}>{players} players   {_fps} fps" +
                   (Utils.IsHost ? "" : "   host features disabled") + "</color>";
        }

        private void DrawTooltip()
        {
            var tip = W.Hover;
            if (string.IsNullOrEmpty(tip)) return;

            Theme.Ensure();

            // CalcSize/CalcHeight are engine helpers; fall back to a fixed box if they
            // are unavailable rather than taking the GUI pass down.
            var width = 260f;
            var height = 40f;

            try
            {
                var content = new GUIContent(tip);
                width = Math.Min(Theme.Dim.CalcSize(content).x + 16f, 320f);
                height = Theme.Dim.CalcHeight(content, width - 16f) + 10f;
            }
            catch (Exception e)
            {
                Safe.Report("tooltip measure", e);
            }

            var pos = Event.current.mousePosition;
            var x = Math.Max(Math.Min(pos.x + 16f, Screen.width - width - 4f), 0f);
            var y = Math.Max(Math.Min(pos.y + 16f, Screen.height - height - 4f), 0f);

            var rect = new Rect(x, y, width, height);
            Theme.Fill(rect, Theme.Alpha(Theme.PanelAlt, 0.98f));
            GUI.Label(new Rect(rect.x + 8f, rect.y + 5f, rect.width - 16f, rect.height - 10f),
                tip, Theme.Dim);
        }
    }
}
