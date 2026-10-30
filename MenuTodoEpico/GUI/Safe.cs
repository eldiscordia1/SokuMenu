using System;
using System.Collections.Generic;
using UnityEngine;

namespace SidaMenu
{
    /// <summary>
    /// Isolates drawing steps from each other.
    ///
    /// IL2CPP can strip methods that the reference assembly still advertises, and
    /// Il2CppInterop then throws <see cref="NotSupportedException"/> ("Method unstripping
    /// failed") the moment such a method is touched. An exception escaping an IMGUI pass
    /// corrupts the GUI clip stack, so every risky call is funnelled through here: the
    /// failure is logged once and the rest of the menu keeps working.
    /// </summary>
    internal static class Safe
    {
        private static readonly HashSet<string> Reported = new HashSet<string>();

        public static void Report(string what, Exception e)
        {
            if (!Reported.Add(what)) return;

            try { Debug.LogError($"[SidaMenu] {what} unavailable -> {e.GetType().Name}: {e.Message}"); }
            catch { /* logging must never throw */ }

            try { EventLog.Add($"{what} unavailable: {e.Message}", LogKind.Danger); }
            catch { }
        }

        /// <summary>Runs an action, logging and swallowing the first failure for that name.</summary>
        public static bool Run(string what, Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception e)
            {
                Report(what, e);
                return false;
            }
        }
    }
}
