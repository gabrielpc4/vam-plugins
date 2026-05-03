using System;
using System.Collections.Generic;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// During <see cref="SuperController.isLoading"/>, turns off renderers on
    /// CustomUnityAsset atoms using DillDoe cum (<c>Fluid.assetbundle</c> /
    /// <c>DillDoe_Cum</c>) so the mesh does not appear while JSON is still
    /// applying (VaM skips <see cref="SuperController.SyncHiddenAtoms"/> while
    /// loading, so atom "hidden" is ineffective for this).
    /// </summary>
    internal static class EasyMateFluidCumHideDuringSceneLoad
    {
        private sealed class Entry
        {
            public Renderer renderer;
            public bool wasEnabled;
        }

        private static readonly List<Entry> Records = new List<Entry>();

        private static readonly HashSet<int> SeenIds = new HashSet<int>();

        public static void Tick(bool featureEnabled, bool superIsLoading)
        {
            if (!featureEnabled)
            {
                RestoreAll();
                return;
            }
            if (superIsLoading)
                ApplyHide();
            else
                RestoreAll();
        }

        public static void OnPluginDestroy()
        {
            RestoreAll();
        }

        private static void RestoreAll()
        {
            for (int i = 0; i < Records.Count; i++)
            {
                Entry e = Records[i];
                if (e.renderer != null)
                    e.renderer.enabled = e.wasEnabled;
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
                    Renderer[] rends = atom.gameObject.GetComponentsInChildren<Renderer>(
                        true);
                    for (int i = 0; i < rends.Length; i++)
                    {
                        Renderer r = rends[i];
                        if (r == null)
                            continue;
                        int id = r.GetInstanceID();
                        if (!SeenIds.Contains(id))
                        {
                            SeenIds.Add(id);
                            Entry e = new Entry();
                            e.renderer = r;
                            e.wasEnabled = r.enabled;
                            Records.Add(e);
                        }
                        r.enabled = false;
                    }
                }
            }
            catch
            {
            }
        }
    }
}
