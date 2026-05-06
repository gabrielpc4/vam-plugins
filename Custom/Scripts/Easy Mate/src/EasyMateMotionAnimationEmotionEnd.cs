using System;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// When scene motion uses <see cref="SuperController.motionAnimationMaster"/> with loop off
    /// and at least one clip longer than a configurable minimum, detects mocap end (timeline
    /// counter enters the tail or resets from the tail toward zero). Then schedules loading
    /// <c>Saves/scene/Default.json</c> after <see cref="MocapEndToDefaultSceneRealtimeDelaySeconds"/>
    /// realtime seconds. Skips when load/save dir paths contain booty shake (case-insensitive).
    /// </summary>
    internal static class EasyMateMotionAnimationEmotionEnd
    {
        /// <summary>Relative to VaM install; same pattern as <c>ResetVROrientation</c>.
        /// </summary>
        private const string MocapEndLoadScenePath = "Saves/scene/Default.json";

        /// <summary>Realtime wait after mocap end before <c>Load(Default.json)</c>.</summary>
        internal const float MocapEndToDefaultSceneRealtimeDelaySeconds = 5f;

        /// <summary>Substring on load/save dir haystack for exception from default load.</summary>
        private const string BootyShakePathToken = "booty shake";

        private static bool _mocapEndLoadFiredThisScene;

        private static float _prevPlaybackCounter;

        private static bool _hasPrevPlaybackCounter;

        private static bool _seenPlaybackAdvance;

        public static void ResetForNewScene()
        {
            _mocapEndLoadFiredThisScene = false;
            _prevPlaybackCounter = 0f;
            _hasPrevPlaybackCounter = false;
            _seenPlaybackAdvance = false;
        }

        public static void LateTick(
            bool featureEnabled,
            float minClipLengthSeconds,
            EasyMate coroutineHost)
        {
            if (!featureEnabled)
                return;

            if (coroutineHost == null)
                return;

            if (_mocapEndLoadFiredThisScene)
                return;

            if (!CurrentSceneUsesLongNonLoopMocap(minClipLengthSeconds))
                return;

            SuperController sc = SuperController.singleton;
            MotionAnimationMaster mam = sc.motionAnimationMaster;
            float maxClip = GetMaxSceneMotionClipLength(sc);

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

            bool shouldLoad = false;
            if (_seenPlaybackAdvance)
            {
                if (atEnd && !prevAtEnd)
                    shouldLoad = true;
                else if (prevAtEnd && pc < Mathf.Max(0.04f * maxClip, 0.25f))
                    shouldLoad = true;
            }

            _prevPlaybackCounter = pc;

            if (!shouldLoad)
                return;

            _mocapEndLoadFiredThisScene = true;
            coroutineHost.StartDelayedMocapEndDefaultScene();
        }

        /// <summary>
        /// Same scene qualification used by the delayed Default.json load:
        /// current scene is not a booty-shake exception, has scene motion,
        /// at least one clip is long enough, and the motion master is not
        /// looping.
        /// </summary>
        internal static bool CurrentSceneUsesLongNonLoopMocap(
            float minClipLengthSeconds)
        {
            SuperController sc = SuperController.singleton;
            if (sc == null || sc.isLoading)
                return false;

            if (CurrentScenePathIndicatesBootyShake(sc))
                return false;

            MotionAnimationMaster mam = sc.motionAnimationMaster;
            if (mam == null)
                return false;

            float maxClip = GetMaxSceneMotionClipLength(sc);
            if (maxClip < minClipLengthSeconds || maxClip < 0.01f)
                return false;

            return !mam.loop;
        }

        /// <summary>Called from <see cref="EasyMate"/> coroutine after delay.</summary>
        internal static void ExecuteDeferredDefaultSceneLoad()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null || sc.isLoading)
                return;

            try
            {
                sc.Load(MocapEndLoadScenePath);
                SuperController.LogMessage(
                    "EasyMate: Loaded " + MocapEndLoadScenePath +
                    " after non-loop mocap end (delayed).");
            }
            catch (System.Exception e)
            {
                SuperController.LogError(
                    "EasyMate: scene load after mocap end: " + e.Message);
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

        /// <summary>
        /// Exclude certain folder-named scenes from post-mocap default load (Haystack-style
        /// match on <see cref="SuperController.currentLoadDir"/> and
        /// <see cref="SuperController.currentSaveDir"/>.
        /// </summary>
        private static bool CurrentScenePathIndicatesBootyShake(SuperController sc)
        {
            if (sc == null)
                return false;

            string loadDir = sc.currentLoadDir;
            string saveDir = sc.currentSaveDir;
            string hay = ((loadDir != null ? loadDir : "") + " " + (saveDir != null ? saveDir : "")).Replace('\\', '/');
            return hay.IndexOf(BootyShakePathToken, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
