using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Lightweight facial motion: smoothly varies a small set of mouth / cheek / brow morphs only.
    /// Does not touch head, neck, eye gaze, or eye morphs (eyebrow morphs are allowed).
    /// </summary>
    public class EasyMotionLite : MVRScript
    {
        private const int MaxMorphsTracked = 32;

        private GenerateDAZMorphsControlUI morphUi;

        private readonly List<MorphChannel> channels = new List<MorphChannel>();

        private JSONStorableBool enabledDriving;

        private JSONStorableFloat globalIntensity;

        private JSONStorableFloat smoothSpeed;

        private JSONStorableFloat retargetInterval;

        private float retargetClock;

        public override void Init()
        {
            pluginLabelJSON.val = "EasyMotionLite";

            enabledDriving = new JSONStorableBool("Enable face motion", true);
            RegisterBool(enabledDriving);

            globalIntensity = new JSONStorableFloat("Expression intensity", 0.35f, 0f, 1f, true);
            RegisterFloat(globalIntensity);

            smoothSpeed = new JSONStorableFloat("Smooth speed", 2.5f, 0.5f, 8f, true);
            RegisterFloat(smoothSpeed);

            retargetInterval = new JSONStorableFloat("Retarget every (s)", 2.2f, 0.6f, 8f, true);
            RegisterFloat(retargetInterval);

            CreateToggle(enabledDriving, false);
            CreateSlider(globalIntensity, false);
            CreateSlider(smoothSpeed, false);
            CreateSlider(retargetInterval, false);

            StartCoroutine(CoBootstrapMorphs());
        }

        private IEnumerator CoBootstrapMorphs()
        {
            yield return new WaitForSeconds(0.35f);
            yield return null;
            RefreshMorphChannels();
        }

        private void RefreshMorphChannels()
        {
            channels.Clear();
            try
            {
                JSONStorable geo = containingAtom != null ? containingAtom.GetStorableByID("geometry") : null;
                DAZCharacterSelector dcs = geo as DAZCharacterSelector;
                if (dcs == null)
                    return;

                morphUi = dcs.morphsControlUI;
                if (morphUi == null)
                    return;

                List<string> uidList = new List<string>();
                foreach (string u in morphUi.GetMorphUids())
                    uidList.Add(u);

                List<DAZMorph> candidates = new List<DAZMorph>();
                for (int i = 0; i < uidList.Count; i++)
                {
                    DAZMorph m = morphUi.GetMorphByUid(uidList[i]);
                    if (m == null)
                        continue;
                    if (!m.gameObject.activeInHierarchy)
                        continue;
                    if (!AllowedFacialMorph(m))
                        continue;
                    candidates.Add(m);
                }

                if (candidates.Count == 0)
                    return;

                int pickCount = candidates.Count;
                if (pickCount > MaxMorphsTracked)
                    pickCount = MaxMorphsTracked;

                while (candidates.Count > pickCount)
                {
                    int r = UnityEngine.Random.Range(0, candidates.Count);
                    candidates.RemoveAt(r);
                }

                for (int c = 0; c < candidates.Count; c++)
                {
                    DAZMorph mm = candidates[c];
                    MorphChannel ch = new MorphChannel();
                    ch.morph = mm;
                    ch.current = mm.morphValue;
                    ch.target = ch.current;
                    channels.Add(ch);
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("EasyMotionLite: refresh morphs: " + e.Message);
            }
        }

        private static bool AllowedFacialMorph(DAZMorph m)
        {
            if (m == null)
                return false;

            string blob = ((m.displayName ?? "") + " " + (m.morphName ?? "") + " " + (m.uid ?? "")).ToLowerInvariant();

            if (IsLikelyBodyMorph(blob))
                return false;
            if (IsExcludedEyeMorph(blob))
                return false;
            if (BlobSuggestsFacial(blob))
                return true;

            return false;
        }

        private static bool BlobSuggestsFacial(string blob)
        {
            return blob.Contains("mouth") || blob.Contains("lip") || blob.Contains("cheek") || blob.Contains("smile") ||
                blob.Contains("frown") || blob.Contains("grimace") || blob.Contains("pucker") || blob.Contains("kiss") ||
                blob.Contains("jaw") || blob.Contains("chin ") || blob.Contains(" chin") || blob.Contains("dimple") ||
                blob.Contains("nasolab") || blob.Contains("marionette") || blob.Contains("philtrum") || blob.Contains("nose") ||
                blob.Contains("brow") || blob.Contains("forehead") || blob.Contains("temple") || blob.Contains("expression") ||
                blob.Contains("face") || blob.Contains("muzzle") || blob.Contains("wrinkle");
        }

        private static bool IsExcludedEyeMorph(string blob)
        {
            if (blob.Contains("eyebrow"))
                return false;
            if (blob.Contains("brow") && !blob.Contains("eye"))
                return false;
            if (blob.Contains("iris") || blob.Contains("pupil") || blob.Contains("lacrimal") || blob.Contains("tear") ||
                blob.Contains("blink") || blob.Contains("eyelid") || blob.Contains("lid close") || blob.Contains("lid_close") ||
                blob.Contains("squint") || blob.Contains("gaze") || blob.Contains("saccade") || blob.Contains("pup dil"))
                return true;
            if (blob.Contains("eye"))
                return true;
            return false;
        }

        private static bool IsLikelyBodyMorph(string blob)
        {
            return blob.Contains("breast") || blob.Contains("pectoral") || blob.Contains("glute") || blob.Contains("butt") ||
                blob.Contains("penis") || blob.Contains("vagina") || blob.Contains("labia") || blob.Contains("testicle") ||
                blob.Contains("areola") || blob.Contains("nipple") || blob.Contains("abdomen") || blob.Contains("belly") ||
                blob.Contains("navel") || blob.Contains("waist") || blob.Contains("shoulder") || blob.Contains("bicep") ||
                blob.Contains("forearm") || blob.Contains("thigh") || blob.Contains("calf") || blob.Contains("foot") ||
                blob.Contains("toe") || blob.Contains("finger") || blob.Contains("thumb") || blob.Contains("pelvis") ||
                blob.Contains("hip ") || blob.Contains(" hip") || blob.Contains("neck") || blob.Contains("head ") ||
                blob.Contains(" head") || blob.Contains("torso") || blob.Contains("scapula") || blob.Contains("trap");
        }

        private void PickNewTargets()
        {
            float intensity = globalIntensity != null ? globalIntensity.val : 0.35f;
            for (int i = 0; i < channels.Count; i++)
            {
                MorphChannel ch = channels[i];
                if (ch.morph == null)
                    continue;
                float span = Mathf.Max(0.02f, (ch.morph.max - ch.morph.min) * 0.45f * intensity);
                float center = (ch.morph.min + ch.morph.max) * 0.5f;
                float t = center + UnityEngine.Random.Range(-span, span);
                ch.target = Mathf.Clamp(t, ch.morph.min, ch.morph.max);
            }
        }

        public void Update()
        {
            if (enabledDriving == null || !enabledDriving.val)
                return;
            if (channels.Count == 0)
                return;

            retargetClock -= Time.deltaTime;
            if (retargetClock <= 0f)
            {
                float interval = retargetInterval != null ? retargetInterval.val : 2.2f;
                retargetClock = Mathf.Max(0.5f, interval);
                PickNewTargets();
            }

            float spd = smoothSpeed != null ? smoothSpeed.val : 2.5f;
            float k = Mathf.Clamp01(Time.deltaTime * spd);
            for (int i = 0; i < channels.Count; i++)
            {
                MorphChannel ch = channels[i];
                if (ch.morph == null)
                    continue;
                ch.current = Mathf.Lerp(ch.current, ch.target, k);
                ch.morph.morphValue = ch.current;
            }
        }

        private class MorphChannel
        {
            public DAZMorph morph;

            public float current;

            public float target;
        }
    }
}
