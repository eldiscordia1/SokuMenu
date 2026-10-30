using UnityEngine;

namespace SidaMenu
{
    /// <summary>Polls the bound hotkeys once per frame.</summary>
    internal static class KeybindManager
    {
        public static void Tick()
        {
            // The widget that is capturing a key owns input until it finishes.
            if (States.bindingField != null) return;

            // Never fire cheats while the player is typing in chat.
            if (Utils.ChatOpen) return;

            foreach (var kv in Keybinds.Map)
            {
                if (kv.Value == KeyCode.None) continue;
                if (!Input.GetKeyDown(kv.Value)) continue;
                Features.ByField(kv.Key)?.Toggle();
            }
        }
    }
}
