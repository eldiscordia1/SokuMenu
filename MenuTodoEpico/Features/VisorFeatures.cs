using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SidaMenu
{
    /// <summary>
    /// Visor cosmetic tools.
    ///
    /// Among Us validates the player name and colour through server commands, but
    /// cosmetics have no such check: a client sends its own visor id and every other
    /// client renders it. That means any visor the game registers can be worn without
    /// being the host. Hidden visors ship in the same catalogue as the store ones, so
    /// they replicate too. Ids missing from the catalogue cannot be resolved by the
    /// other clients, so arbitrary textures stay local.
    /// </summary>
    internal static class VisorFeatures
    {
        public readonly struct KnownVisor
        {
            public readonly string Id;
            public readonly string Label;

            public KnownVisor(string id, string label)
            {
                Id = id;
                Label = label;
            }
        }

        /// <summary>Unused visors present in the game files but absent from the wardrobe.</summary>
        public static readonly KnownVisor[] KnownHidden =
        {
            new KnownVisor("visor_impostorShine", "Totally Innocent (unused)")
        };

        private static readonly List<VisorData> Cache = new List<VisorData>();
        private static readonly Dictionary<string, string> Names = new Dictionary<string, string>();
        private static readonly HashSet<string> BlacklistedIds =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private static bool _metaBuilt;

        /// <summary>Every visor the game registers, including hidden, locked and blacklisted ones.</summary>
        public static List<VisorData> All()
        {
            if (!_metaBuilt)
            {
                BuildBlacklist(GetReferenceData());
                _metaBuilt = true;
            }

            if (Cache.Count > 0) return Cache;

            // The authoritative array every client resolves RpcSetVisor against. Hidden
            // visors live in this array too, which is what makes them visible to others.
            try
            {
                var manager = HatManager.Instance;
                var field = manager != null ? AccessTools.Field(typeof(HatManager), "allVisors") : null;
                var value = field != null ? field.GetValue(manager) : null;

                if (value is VisorData[] array)
                {
                    foreach (var visor in array)
                    {
                        if (visor != null) Cache.Add(visor);
                    }
                }
                else if (value is System.Collections.IEnumerable list)
                {
                    foreach (var item in list)
                    {
                        if (item is VisorData visor) Cache.Add(visor);
                    }
                }
            }
            catch (Exception e) { Safe.Report("visor array", e); }

            // Fallback: the cosmetic manifest.
            var data = GetReferenceData();
            if (Cache.Count == 0 && data != null && data.visors != null)
            {
                foreach (var visor in data.visors)
                {
                    if (visor != null) Cache.Add(visor);
                }
            }

            // Last resort while the manifest is still loading.
            if (Cache.Count == 0)
            {
                try
                {
                    var manager = HatManager.Instance;
                    var unlocked = manager != null ? manager.GetUnlockedVisors() : null;
                    if (unlocked != null)
                    {
                        foreach (var visor in unlocked)
                        {
                            if (visor != null) Cache.Add(visor);
                        }
                    }
                }
                catch (Exception e) { Safe.Report("visor unlocks", e); }
            }

            return Cache;
        }

        /// <summary>Drops the cache so the next draw rebuilds it (after a game update).</summary>
        public static void Invalidate()
        {
            Cache.Clear();
            Names.Clear();
            BlacklistedIds.Clear();
            _metaBuilt = false;
        }

        private static ReferenceData? GetReferenceData()
        {
            try
            {
                var manager = ReferenceDataManager.Instance;
                return manager != null ? manager.Refdata : null;
            }
            catch (Exception e) { Safe.Report("reference data", e); return null; }
        }

        private static void BuildBlacklist(ReferenceData? data)
        {
            BlacklistedIds.Clear();
            if (data == null) return;

            // These are IL2CPP lists, so each one is walked directly instead of
            // going through a System.List helper parameter.
            try
            {
                if (data.CosmeticBlacklist != null)
                {
                    foreach (var cosmetic in data.CosmeticBlacklist) AddBlacklisted(cosmetic);
                }
            }
            catch (Exception e) { Safe.Report("visor blacklist", e); }

            try
            {
                if (data.LongModeBlacklist != null)
                {
                    foreach (var cosmetic in data.LongModeBlacklist) AddBlacklisted(cosmetic);
                }
            }
            catch (Exception e) { Safe.Report("visor blacklist", e); }
        }

        private static void AddBlacklisted(CosmeticData? cosmetic)
        {
            if (cosmetic == null) return;

            try
            {
                if (!string.IsNullOrEmpty(cosmetic.ProdId)) BlacklistedIds.Add(cosmetic.ProdId);
            }
            catch (Exception e) { Safe.Report("visor blacklist", e); }
        }

        public static bool IsBlacklisted(VisorData? visor) =>
            visor != null && BlacklistedIds.Contains(IdOf(visor));

        /// <summary>Hidden from the wardrobe, blacklisted, or a known unused visor.</summary>
        public static bool IsHidden(VisorData? visor)
        {
            if (visor == null) return false;

            try { if (visor.NotInStore) return true; }
            catch (Exception e) { Safe.Report("visor hidden flag", e); }

            if (IsBlacklisted(visor)) return true;

            var id = IdOf(visor);
            foreach (var known in KnownHidden)
            {
                if (string.Equals(known.Id, id, StringComparison.OrdinalIgnoreCase)) return true;
            }

            return false;
        }

        /// <summary>True when this build's HatManager can resolve the id for everyone.</summary>
        public static bool IsRegistered(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;

            try
            {
                var manager = HatManager.Instance;
                return manager != null && manager.GetVisorById(id) != null;
            }
            catch (Exception e) { Safe.Report("visor lookup", e); return Find(id) != null; }
        }

        public static string IdOf(VisorData? visor)
        {
            if (visor == null) return "";

            try { return visor.ProdId ?? ""; }
            catch (Exception e) { Safe.Report("visor id", e); return ""; }
        }

        public static string NameOf(VisorData? visor)
        {
            if (visor == null) return "?";

            var id = IdOf(visor);
            if (Names.TryGetValue(id, out var cached)) return cached;

            var name = "";

            try
            {
                name = visor.GetItemName();

                // Untranslated cosmetics come back as their translation key.
                if (name != null && name.StartsWith("Cosmetic.", StringComparison.Ordinal)) name = "";
            }
            catch (Exception e) { Safe.Report("visor name", e); }

            if (string.IsNullOrWhiteSpace(name)) name = Pretty(id);

            Names[id] = name;
            return name;
        }

        /// <summary>Turns "visor_pk01_Angry" into a readable fallback label.</summary>
        private static string Pretty(string id)
        {
            if (string.IsNullOrEmpty(id)) return "Visor";
            if (id.StartsWith("visor_", StringComparison.OrdinalIgnoreCase)) id = id.Substring(6);
            return id.Replace('_', ' ').Trim();
        }

        public static string CurrentId()
        {
            try
            {
                var lp = PlayerControl.LocalPlayer;
                var data = lp != null ? lp.Data : null;
                var outfit = data != null ? data.DefaultOutfit : null;
                return outfit != null ? outfit.VisorId ?? "" : "";
            }
            catch (Exception e) { Safe.Report("current visor", e); return ""; }
        }

        public static VisorData? Find(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;

            foreach (var visor in All())
            {
                if (string.Equals(IdOf(visor), id, StringComparison.OrdinalIgnoreCase)) return visor;
            }

            return null;
        }

        /// <summary>
        /// Applies the visor locally and then broadcasts it through the game's own
        /// cosmetic RPC. No host check is involved, so this works in any lobby.
        /// </summary>
        public static void Apply(string? visorId)
        {
            var lp = PlayerControl.LocalPlayer;
            if (lp == null)
            {
                Notification.Push("Join a lobby or game first", Theme.Warn);
                return;
            }

            var id = string.IsNullOrWhiteSpace(visorId) ? VisorData.EmptyId : visorId.Trim();

            var color = 0;
            try
            {
                if (lp.Data != null) color = lp.Data.DefaultOutfit.ColorId;
            }
            catch (Exception e) { Safe.Report("visor colour", e); }

            // Apply locally first: this still works in freeplay, where no RPC is sent.
            try
            {
                lp.SetVisor(id, color);
                if (lp.Data != null) lp.Data.UpdateVisor(id);
            }
            catch (Exception e) { Safe.Report("visor local apply", e); }

            try
            {
                lp.RpcSetVisor(id);

                var known = Find(id) != null || IsRegistered(id);
                Notification.Push(known ? $"Visor: {Pretty(id)}" : $"Visor id sent: {id}",
                    known ? Theme.Good : Theme.Warn);
                EventLog.Add($"Set visor to {id}", LogKind.Action);
            }
            catch (Exception e)
            {
                Notification.Push("Visor change failed", Theme.Warn);
                EventLog.Add("Visor change failed: " + e, LogKind.Danger);
            }
        }

        public static void Reset() => Apply(VisorData.EmptyId);

        public static void ApplyRandom()
        {
            var all = All();
            if (all.Count == 0)
            {
                Notification.Push("Visor catalogue is still loading", Theme.Warn);
                return;
            }

            var pick = all[UnityEngine.Random.Range(0, all.Count)];

            // Do not "randomise" to the visor that is already on, if possible.
            if (all.Count > 1 && string.Equals(IdOf(pick), CurrentId(), StringComparison.OrdinalIgnoreCase))
            {
                pick = all[(all.IndexOf(pick) + 1) % all.Count];
            }

            Apply(IdOf(pick));
        }
    }
}
