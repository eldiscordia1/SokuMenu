using HarmonyLib;
using UnityEngine;

namespace SidaMenu
{
    /// <summary>Client sided features that only affect the local player.</summary>
    internal static class SelfFeatures
    {
        private static bool _freecamApplied;
        private static float _defaultCamSize = -1f;
        private static float _defaultUiSize = -1f;

        public static void Tick()
        {
            var lp = PlayerControl.LocalPlayer;
            if (lp == null) return;

            NoClip(lp);
            Teleport(lp);
            Freecam(lp);
            Zoom();
            SeeGhosts();
            NoShadows();
            AlwaysChat();
            MoveWhileVenting(lp);
        }

        private static void NoClip(PlayerControl lp)
        {
            if (lp.Collider != null) lp.Collider.enabled = !States.noClip;
        }

        private static void Teleport(PlayerControl lp)
        {
            if (!States.tpToCursor) return;
            if (!Input.GetMouseButtonDown(1)) return;
            if (Camera.main == null) return;

            var world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            lp.NetTransform.RpcSnapTo(new Vector2(world.x, world.y));
        }

        private static void Freecam(PlayerControl lp)
        {
            var cam = Camera.main;

            if (States.freecam)
            {
                if (cam != null)
                {
                    var follower = cam.GetComponent<FollowerCamera>();
                    if (follower != null)
                    {
                        follower.Target = null;
                        follower.Locked = false;
                    }

                    var dir = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0f);
                    if (dir.sqrMagnitude > 0.01f)
                        cam.transform.position += dir.normalized * States.freecamSpeed * Time.unscaledDeltaTime;
                }

                lp.moveable = false;
                _freecamApplied = true;
            }
            else if (_freecamApplied)
            {
                if (cam != null)
                {
                    var follower = cam.GetComponent<FollowerCamera>();
                    if (follower != null)
                    {
                        follower.Target = lp;
                        follower.Locked = true;
                    }
                }

                lp.moveable = true;
                _freecamApplied = false;
            }
        }

        private static void Zoom()
        {
            var cam = Camera.main;
            if (cam == null) return;
            var hud = HudManager.Instance;
            var ui = hud != null ? hud.UICamera : null;

            if (States.zoomOut)
            {
                if (_defaultCamSize < 0f && cam.orthographicSize > 0f) _defaultCamSize = cam.orthographicSize;
                if (_defaultUiSize < 0f && ui != null && ui.orthographicSize > 0f) _defaultUiSize = ui.orthographicSize;

                // Mouse wheel nudges the zoom level while the feature is enabled.
                var scroll = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scroll) > 0.001f && !Utils.ChatOpen)
                {
                    States.zoomLevel = Mathf.Clamp(States.zoomLevel + scroll * 3f, 3.5f, 18f);
                }

                cam.orthographicSize = States.zoomLevel;
                if (ui != null) ui.orthographicSize = Mathf.Max(States.zoomLevel, 3f);
            }
            else if (_defaultCamSize > 0f)
            {
                cam.orthographicSize = _defaultCamSize;
                if (ui != null && _defaultUiSize > 0f) ui.orthographicSize = _defaultUiSize;
                _defaultCamSize = -1f;
                _defaultUiSize = -1f;
            }
        }

        private static void SeeGhosts()
        {
            if (!States.seeGhosts) return;

            foreach (var pc in Utils.AllPlayers())
            {
                if (pc.AmOwner) continue;
                if (pc.Data != null && pc.Data.IsDead) pc.Visible = true;
            }
        }

        private static void NoShadows()
        {
            var hud = HudManager.Instance;
            if (hud == null || hud.ShadowQuad == null) return;
            hud.ShadowQuad.gameObject.SetActive(!States.noShadows);
        }

        private static void AlwaysChat()
        {
            if (!States.alwaysChat) return;
            var hud = HudManager.Instance;
            if (hud == null || hud.Chat == null) return;
            hud.Chat.gameObject.SetActive(true);
        }

        private static void MoveWhileVenting(PlayerControl lp)
        {
            if (!States.moveWhileVenting) return;
            if (!lp.inVent) return;
            lp.inVent = false;
            lp.moveable = true;
        }

        /// <summary>Applies the speed multiplier on top of the game's own calculation.</summary>
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.TrueSpeed), MethodType.Getter)]
        internal static class SpeedPatch
        {
            public static void Postfix(PlayerPhysics __instance, ref float __result)
            {
                if (!__instance.AmOwner) return;
                if (States.speed <= 0f || Mathf.Approximately(States.speed, 1f)) return;
                __result *= States.speed;
            }
        }
    }
}
