using System;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// When scene motion uses <see cref="SuperController.motionAnimationMaster"/> with loop off
    /// and at least one clip longer than a configurable minimum, detects end of playback (timeline
    /// counter enters the tail or resets from the tail toward zero). Then schedules loading
    /// <c>Saves/scene/Default.json</c> after <see cref="AnimationNoLoopToDefaultSceneRealtimeDelaySeconds"/>
    /// realtime seconds. Skips paths containing booty shake (Haystack-style, case-insensitive).
    /// </summary>
    internal static class AnimationNoLoopMainEnd
    {
        private const string AnimationNoLoopDefaultScenePath = "Saves/scene/Default.json";

        internal const float AnimationNoLoopToDefaultSceneRealtimeDelaySeconds =
            5f;

        private const string BootyShakePathToken = "booty shake";

        private static bool _animationNoLoopDefaultLoadFiredThisScene;

        private static float _prevPlaybackCounter;

        private static bool _hasPrevPlaybackCounter;

        private static bool _seenPlaybackAdvance;

        public static void ResetForNewScene()
        {
            _animationNoLoopDefaultLoadFiredThisScene = false;
            _prevPlaybackCounter = 0f;
            _hasPrevPlaybackCounter = false;
            _seenPlaybackAdvance = false;
        }

        public static void LateTick(
            bool featureEnabled,
            float minClipLengthSeconds,
            GabrielHud hud)
        {
            if (!featureEnabled)
                return;

            if (hud == null)
                return;

            if (_animationNoLoopDefaultLoadFiredThisScene)
                return;

            if (!CurrentSceneUsesLongNonLoopAnimation(minClipLengthSeconds))
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

            _animationNoLoopDefaultLoadFiredThisScene = true;
            hud.StartAnimationNoLoopDefaultSceneDelayCoroutine();
        }

        private static bool TryGetLongNonLoopAnimationState(
            float minClipLengthSeconds,
            out SuperController sc,
            out MotionAnimationMaster mam,
            out float maxClip)
        {
            sc = SuperController.singleton;
            mam = null;
            maxClip = 0f;

            if (sc == null || sc.isLoading)
                return false;

            mam = sc.motionAnimationMaster;
            if (mam == null)
                return false;

            maxClip = GetMaxSceneMotionClipLength(sc);
            if (maxClip < minClipLengthSeconds || maxClip < 0.01f)
                return false;

            return !mam.loop;
        }

        /// <summary>
        /// Scene qualifies for deferred Default.json: not booty-shake paths, motion on,
        /// long enough dominant clip, master not looping.
        /// </summary>
        internal static bool CurrentSceneUsesLongNonLoopAnimation(
            float minClipLengthSeconds)
        {
            SuperController sc;
            MotionAnimationMaster mam;
            float maxClip;
            if (!TryGetLongNonLoopAnimationState(
                minClipLengthSeconds,
                out sc,
                out mam,
                out maxClip))
                return false;

            return !CurrentScenePathIndicatesBootyShake(sc);
        }

        /// <summary>
        /// Grip-triggered Spankings merge rules: non-booty long non-loop always blocks early
        /// merge until rules say otherwise; booty-shake blocks only until playback nears clip
        /// end.
        /// </summary>
        internal static bool CurrentSceneBlocksGripSpankingsMerge(
            float minClipLengthSeconds)
        {
            SuperController sc;
            MotionAnimationMaster mam;
            float maxClip;
            if (!TryGetLongNonLoopAnimationState(
                minClipLengthSeconds,
                out sc,
                out mam,
                out maxClip))
                return false;

            if (!CurrentScenePathIndicatesBootyShake(sc))
                return true;

            float endEps = Mathf.Max(0.12f, 0.003f * maxClip);
            return mam.playbackCounter < maxClip - endEps;
        }

        internal static void ExecuteDeferredDefaultSceneLoad()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null || sc.isLoading)
                return;

            try
            {
                sc.Load(AnimationNoLoopDefaultScenePath);
                SuperController.LogMessage(
                    "GabrielHud: Loaded " + AnimationNoLoopDefaultScenePath +
                    " after non-loop animation end (delayed).");
            }
            catch (System.Exception e)
            {
                SuperController.LogError(
                    "GabrielHud: scene load after non-loop animation end: " +
                    e.Message);
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

        private static bool CurrentScenePathIndicatesBootyShake(SuperController sc)
        {
            if (sc == null)
                return false;

            string loadDir = sc.currentLoadDir;
            string saveDir = sc.currentSaveDir;
            string hay =
                ((loadDir != null ? loadDir : "") +
                " " + (saveDir != null ? saveDir : ""))
                .Replace('\\', '/');
            return hay.IndexOf(BootyShakePathToken, System.StringComparison.OrdinalIgnoreCase)
                >= 0;
        }
    }
}
