using System;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// When scene motion uses <see cref="SuperController.motionAnimationMaster"/> with loop off
    /// and at least one clip longer than a configurable minimum, detects end of
    /// playback and schedules loading <c>Saves/scene/Default.json</c> after
    /// <see cref="DeferredDefaultSceneRealtimeDelaySeconds"/> realtime seconds.
    /// Skips paths matching the exception bucket
    /// (Haystack-style, case-insensitive).
    /// </summary>
    internal static class AnimationNoLoopDetection
    {
        private const string DeferredDefaultScenePath = "Saves/scene/Default.json";

        internal const float DeferredDefaultSceneRealtimeDelaySeconds = 5f;

        private const string ExceptionPathToken = "booty shake";

        private const float CompletionRecheckSeconds = 3f;

        private const float PlaybackAdvanceEpsilon = 0.0005f;

        private static bool _deferredDefaultSceneLoadFiredThisScene;

        private static bool _sceneAnimationStateEvaluated;

        private static float _sceneAnimationStateMinClipLength = -1f;

        private static bool _sceneHasLongNonLoopAnimation;

        private static bool _sceneMatchesExceptionRule;

        private static float _sceneMaxClipLength;

        private static float _prevPlaybackCounter;

        private static bool _hasPrevPlaybackCounter;

        private static bool _seenPlaybackAdvance;

        private static float _nextPlaybackCompletionCheckTime = -1f;

        public static void ResetForNewScene()
        {
            _deferredDefaultSceneLoadFiredThisScene = false;
            _sceneAnimationStateEvaluated = false;
            _sceneAnimationStateMinClipLength = -1f;
            _sceneHasLongNonLoopAnimation = false;
            _sceneMatchesExceptionRule = false;
            _sceneMaxClipLength = 0f;
            _prevPlaybackCounter = 0f;
            _hasPrevPlaybackCounter = false;
            _seenPlaybackAdvance = false;
            _nextPlaybackCompletionCheckTime = -1f;
        }

        public static void LateTick(
            bool featureEnabled,
            float minClipLengthSeconds,
            SessionOrchestrator orchestrator)
        {
            SuperController sc;
            MotionAnimationMaster mam;
            float maxClip;
            bool exceptionRule;
            float pc;
            float endEps;
            float startEps;
            bool atEnd;
            bool wrappedOrResetNearStart;

            if (!featureEnabled)
                return;

            if (orchestrator == null)
                return;

            if (_deferredDefaultSceneLoadFiredThisScene)
                return;

            if (!TryGetSceneAnimationState(
                    minClipLengthSeconds,
                    out sc,
                    out mam,
                    out maxClip,
                    out exceptionRule))
            {
                return;
            }

            if (exceptionRule)
                return;

            pc = mam.playbackCounter;

            if (!_hasPrevPlaybackCounter)
            {
                _hasPrevPlaybackCounter = true;
                _prevPlaybackCounter = pc;
                return;
            }

            if (!_seenPlaybackAdvance)
            {
                if (pc > _prevPlaybackCounter + PlaybackAdvanceEpsilon)
                {
                    _seenPlaybackAdvance = true;
                    _nextPlaybackCompletionCheckTime =
                        Time.unscaledTime + Mathf.Max(0f, maxClip - pc);
                }

                _prevPlaybackCounter = pc;
                return;
            }

            if (_nextPlaybackCompletionCheckTime > 0f &&
                Time.unscaledTime < _nextPlaybackCompletionCheckTime)
            {
                _prevPlaybackCounter = pc;
                return;
            }

            endEps = Mathf.Max(0.12f, 0.003f * maxClip);
            startEps = Mathf.Max(0.04f * maxClip, 0.25f);
            atEnd = pc >= maxClip - endEps;
            wrappedOrResetNearStart = pc < startEps;
            _prevPlaybackCounter = pc;
            if (!atEnd && !wrappedOrResetNearStart)
            {
                _nextPlaybackCompletionCheckTime =
                    Time.unscaledTime + CompletionRecheckSeconds;
                return;
            }

            _deferredDefaultSceneLoadFiredThisScene = true;
            orchestrator.StartAnimationNoLoopDetectionDeferredDefaultCoroutine();
        }

        private static bool TryGetSceneAnimationState(
            float minClipLengthSeconds,
            out SuperController sc,
            out MotionAnimationMaster mam,
            out float maxClip,
            out bool exceptionRule)
        {
            sc = SuperController.singleton;
            mam = null;
            maxClip = 0f;
            exceptionRule = false;

            if (sc == null || sc.isLoading)
                return false;

            mam = sc.motionAnimationMaster;
            if (mam == null)
                return false;

            EnsureSceneAnimationState(
                sc,
                mam,
                minClipLengthSeconds);

            if (!_sceneHasLongNonLoopAnimation)
                return false;

            maxClip = _sceneMaxClipLength;
            exceptionRule = _sceneMatchesExceptionRule;
            return true;
        }

        private static void EnsureSceneAnimationState(
            SuperController sc,
            MotionAnimationMaster mam,
            float minClipLengthSeconds)
        {
            if (_sceneAnimationStateEvaluated &&
                Mathf.Abs(
                    _sceneAnimationStateMinClipLength - minClipLengthSeconds) <
                0.0001f)
            {
                return;
            }

            _sceneAnimationStateEvaluated = true;
            _sceneAnimationStateMinClipLength = minClipLengthSeconds;
            _sceneMaxClipLength = GetMaxSceneMotionClipLength(sc);
            _sceneHasLongNonLoopAnimation =
                !mam.loop &&
                _sceneMaxClipLength >= minClipLengthSeconds &&
                _sceneMaxClipLength >= 0.01f;
            _sceneMatchesExceptionRule =
                _sceneHasLongNonLoopAnimation &&
                CurrentScenePathIndicatesException(sc);
            _hasPrevPlaybackCounter = false;
            _prevPlaybackCounter = 0f;
            _seenPlaybackAdvance = false;
            _nextPlaybackCompletionCheckTime = -1f;

            SoftPhysicsScenePreference.ApplyFromLongNonLoopSceneFlags(
                _sceneHasLongNonLoopAnimation,
                _sceneMatchesExceptionRule);
        }

        /// <summary>
        /// Scene qualifies for deferred Default.json: not exception paths, motion
        /// on, long enough dominant clip, master not looping.
        /// </summary>
        internal static bool CurrentSceneUsesLongNonLoopAnimation(
            float minClipLengthSeconds)
        {
            SuperController sc;
            MotionAnimationMaster mam;
            float maxClip;
            bool exceptionRule;
            if (!TryGetSceneAnimationState(
                minClipLengthSeconds,
                out sc,
                out mam,
                out maxClip,
                out exceptionRule))
                return false;

            return !exceptionRule;
        }

        /// <summary>
        /// Grip-triggered Spankings merge rules: non-exception long non-loop
        /// always blocks early merge until rules say otherwise; exception scenes
        /// block only until playback nears clip end.
        /// </summary>
        internal static bool CurrentSceneBlocksGripSpankingsMerge(
            float minClipLengthSeconds)
        {
            SuperController sc;
            MotionAnimationMaster mam;
            float maxClip;
            bool exceptionRule;
            if (!TryGetSceneAnimationState(
                minClipLengthSeconds,
                out sc,
                out mam,
                out maxClip,
                out exceptionRule))
                return false;

            if (!exceptionRule)
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
                sc.Load(DeferredDefaultScenePath);
                SuperController.LogMessage(
                    "VaMScripts session orchestrator: Loaded " +
                    DeferredDefaultScenePath +
                    " after non-loop animation end (delayed).");
            }
            catch (System.Exception e)
            {
                SuperController.LogError(
                    "VaMScripts session orchestrator: scene load after " +
                    "non-loop animation end: " +
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

        private static bool CurrentScenePathIndicatesException(
            SuperController sc)
        {
            if (sc == null)
                return false;

            string loadDir = sc.currentLoadDir;
            string saveDir = sc.currentSaveDir;
            string hay =
                ((loadDir != null ? loadDir : "") +
                " " + (saveDir != null ? saveDir : ""))
                .Replace('\\', '/');
            return hay.IndexOf(
                ExceptionPathToken,
                System.StringComparison.OrdinalIgnoreCase)
                >= 0;
        }
    }
}
