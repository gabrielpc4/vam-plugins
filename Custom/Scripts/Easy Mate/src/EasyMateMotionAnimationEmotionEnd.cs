using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// When scene motion / mocap playback uses <see cref="SuperController.motionAnimationMaster"/> with loop off and
    /// at least one clip longer than a configurable minimum, merges E-Motion onto female Persons once when playback
    /// reaches the end (timeline counter enters the tail or resets from the tail toward zero).
    /// </summary>
    internal static class EasyMateMotionAnimationEmotionEnd
    {
        private static bool _mergeFiredThisScene;

        private static float _prevPlaybackCounter;

        private static bool _hasPrevPlaybackCounter;

        private static bool _seenPlaybackAdvance;

        public static void ResetForNewScene()
        {
            _mergeFiredThisScene = false;
            _prevPlaybackCounter = 0f;
            _hasPrevPlaybackCounter = false;
            _seenPlaybackAdvance = false;
        }

        public static void LateTick(bool featureEnabled, float minClipLengthSeconds, MainUIButtons mainUIButtons)
        {
            if (!featureEnabled)
                return;
            if (mainUIButtons == null)
                return;

            SuperController sc = SuperController.singleton;
            if (sc == null || sc.isLoading)
                return;

            if (_mergeFiredThisScene)
                return;

            MotionAnimationMaster mam = sc.motionAnimationMaster;
            if (mam == null)
                return;

            float maxClip = GetMaxSceneMotionClipLength(sc);
            if (maxClip < minClipLengthSeconds || maxClip < 0.01f)
                return;

            if (mam.loop)
                return;

            float pc = mam.playbackCounter;

            if (!_hasPrevPlaybackCounter)
            {
                _hasPrevPlaybackCounter = true;
                _prevPlaybackCounter = pc;
                return;
            }

            if (pc > _prevPlaybackCounter + 0.0005f)
                _seenPlaybackAdvance = true;

            float endEps = Mathf.Max(0.12f, 0.003f * maxClip);
            bool atEnd = pc >= maxClip - endEps;
            bool prevAtEnd = _prevPlaybackCounter >= maxClip - endEps;

            bool shouldMerge = false;
            if (_seenPlaybackAdvance)
            {
                if (atEnd && !prevAtEnd)
                    shouldMerge = true;
                else if (prevAtEnd && pc < Mathf.Max(0.04f * maxClip, 0.25f))
                    shouldMerge = true;
            }

            _prevPlaybackCounter = pc;

            if (!shouldMerge)
                return;

            _mergeFiredThisScene = true;
            try
            {
                mainUIButtons.MergeEmotionOnFemalePersonsOnly();
                mainUIButtons.RefreshPluginToggleLabels();
            }
            catch (System.Exception e)
            {
                SuperController.LogError("EasyMate: E-Motion merge after mocap end: " + e.Message);
            }
        }

        private static float GetMaxSceneMotionClipLength(SuperController sc)
        {
            float maxLen = 0f;
            if (sc == null)
                return 0f;

            foreach (Atom atom in sc.GetAtoms())
            {
                if (atom == null)
                    continue;
                MotionAnimationControl[] macs = atom.motionAnimationControls;
                if (macs == null || macs.Length == 0)
                    continue;
                for (int i = 0; i < macs.Length; i++)
                {
                    MotionAnimationControl mac = macs[i];
                    if (mac == null)
                        continue;
                    MotionAnimationClip clip = mac.clip;
                    if (clip == null)
                        continue;
                    if (clip.clipLength > maxLen)
                        maxLen = clip.clipLength;
                }
            }

            return maxLen;
        }
    }
}
