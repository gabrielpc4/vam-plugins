using System;
using System.Collections.Generic;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Keeps CustomUnityAsset DillDoe cum (<c>Fluid.assetbundle</c> /
    /// <c>DillDoe_Cum</c>) from drawing until VaM finishes a scene load, then for
    /// an extra realtime interval (VaM skips
    /// <see cref="SuperController.SyncHiddenAtoms"/> while loading; atom Hidden is
    /// ineffective during that window). Disables <see cref="Renderer"/> and
    /// embedded <see cref="Canvas"/> under matched atoms until release time.
    /// </summary>
    internal static class EasyMateFluidCumHideDuringSceneLoad
    {
        private sealed class Entry
        {
            public Behaviour behaviour;
            public bool wasEnabled;
        }

        private static readonly List<Entry> Records = new List<Entry>();

        private static readonly HashSet<int> SeenIds = new HashSet<int>();

        private static bool _prevSuperLoading;

        private static float _releaseShowAtRealtime;

        public static void Tick(
            bool featureEnabled,
            bool superIsLoading,
            float delayRealtimeSecondsAfterLoadEnds)
        {
            if (!featureEnabled)
            {
                RestoreAll();
                _prevSuperLoading = superIsLoading;
                _releaseShowAtRealtime = 0f;
                return;
            }

            if (delayRealtimeSecondsAfterLoadEnds < 0f)
                delayRealtimeSecondsAfterLoadEnds = 0f;

            if (superIsLoading)
            {
                _prevSuperLoading = true;
                ApplyHide();
                return;
            }

            if (_prevSuperLoading)
            {
                _prevSuperLoading = false;
                _releaseShowAtRealtime =
                    Time.realtimeSinceStartup + delayRealtimeSecondsAfterLoadEnds;
            }

            if (Time.realtimeSinceStartup < _releaseShowAtRealtime)
                ApplyHide();
            else
                RestoreAll();
        }

        public static void OnPluginDestroy()
        {
            RestoreAll();
            _prevSuperLoading = false;
            _releaseShowAtRealtime = 0f;
        }

        private static void RestoreAll()
        {
            for (int i = 0; i < Records.Count; i++)
            {
                Entry e = Records[i];
                if (e.behaviour != null)
                    e.behaviour.enabled = e.wasEnabled;
            }
            Records.Clear();
            SeenIds.Clear();
        }

        private static bool IsFluidCumAtom(Atom atom)
        {
            if (atom == null || atom.type != "CustomUnityAsset")
                return false;
            JSONStorable asset = atom.GetStorableByID("asset");
            if (asset == null)
                return false;
            string assetName = asset.GetStringChooserParamValue("assetName");
            if (assetName != null && assetName.IndexOf(
                "DillDoe_Cum",
                StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            string assetUrl = asset.GetUrlParamValue("assetUrl");
            if (assetUrl != null && assetUrl.IndexOf(
                "Fluid.assetbundle",
                StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            return false;
        }

        private static void RememberAndDisableBehaviour(Behaviour b)
        {
            if (b == null)
                return;
            int id = b.GetInstanceID();
            if (!SeenIds.Contains(id))
            {
                SeenIds.Add(id);
                Entry e = new Entry();
                e.behaviour = b;
                e.wasEnabled = b.enabled;
                Records.Add(e);
            }
            b.enabled = false;
        }

        private static void ApplyHide()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null)
                return;
            try
            {
                foreach (Atom atom in sc.GetAtoms())
                {
                    if (atom == null || !atom.gameObject.activeInHierarchy)
                        continue;
                    if (!IsFluidCumAtom(atom))
                        continue;
                    Renderer[] rends =
                        atom.gameObject.GetComponentsInChildren<Renderer>(true);
                    for (int i = 0; i < rends.Length; i++)
                        RememberAndDisableBehaviour(rends[i]);
                    Canvas[] canvases =
                        atom.gameObject.GetComponentsInChildren<Canvas>(true);
                    for (int c = 0; c < canvases.Length; c++)
                        RememberAndDisableBehaviour(canvases[c]);
                }
            }
            catch
            {
            }
        }
    }
}
