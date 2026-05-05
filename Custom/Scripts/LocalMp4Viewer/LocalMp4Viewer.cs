using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

namespace geesp0t
{
    /// <summary>
    /// One-shot values consumed by the next <see cref="LocalMp4Viewer"/> instance during <c>Init</c>,
    /// used by <see cref="IntroSceneLocalMp4Bootstrap"/> so parenting and presets apply before the quad is built.
    /// </summary>
    public static class LocalMp4ViewerBootstrapHints
    {
        public static string ParentFreeControllerId;

        public static string VideoRelativePath;

        public static bool ApplyTelevisionQuadPreset;

        public static bool ApplyLaptopQuadPreset;

        public static bool ShouldAutoPlayAfterLoad;

        public static void ClearAll()
        {
            ParentFreeControllerId = null;
            VideoRelativePath = null;
            ApplyTelevisionQuadPreset = false;
            ApplyLaptopQuadPreset = false;
            ShouldAutoPlayAfterLoad = false;
        }
    }

    /// <summary>
    /// Plays a local MP4 (or other formats Unity&apos;s <see cref="VideoPlayer"/> supports on your build)
    /// from a path <b>relative to the VaM install folder</b>, e.g. <c>Custom/MyVideos/clip.mp4</c>.
    /// Renders to a quad parented under this atom&apos;s main controller (default), or under a named <see cref="FreeControllerV3"/> when set.
    /// Uses <c>file:///</c> URLs — keep paths ASCII or spaces escaped.
    /// </summary>
    public class LocalMp4Viewer : MVRScript
    {
        private const int PrepareWaitFramesMax = 900;

        private GameObject videoQuadRoot;

        private MeshRenderer videoMeshRenderer;

        private VideoPlayer videoPlayerComponent;

        private RenderTexture videoRenderTexture;

        private Material videoScreenMaterial;

        private Coroutine preparePlayCoroutine;

        public JSONStorableString videoRelativePath;

        public JSONStorableBool loopPlayback;

        public JSONStorableBool playWhenPluginStarts;

        public JSONStorableBool quadVisible;

        public JSONStorableFloat quadWidthMeters;

        public JSONStorableFloat quadHeightMeters;

        public JSONStorableFloat quadLocalPositionX;

        public JSONStorableFloat quadLocalPositionY;

        public JSONStorableFloat quadLocalPositionZ;

        public JSONStorableFloat quadLocalEulerPitch;

        public JSONStorableString parentFreeControllerId;

        public JSONStorableAction playVideoAction;

        public JSONStorableAction stopVideoAction;

        public override void Init()
        {
            videoRelativePath = new JSONStorableString(
                "Video path (relative to VaM folder)",
                "Custom/video/example.mp4");
            RegisterString(videoRelativePath);

            parentFreeControllerId = new JSONStorableString(
                "Parent FreeController id (empty = main)",
                "");
            RegisterString(parentFreeControllerId);
            parentFreeControllerId.setCallbackFunction += OnParentFreeControllerIdChanged;

            loopPlayback = new JSONStorableBool("Loop", false);
            RegisterBool(loopPlayback);

            playWhenPluginStarts = new JSONStorableBool("Play when plugin loads", false);
            RegisterBool(playWhenPluginStarts);

            quadVisible = new JSONStorableBool("Quad visible", true, OnQuadVisibleChanged);
            RegisterBool(quadVisible);

            quadWidthMeters = new JSONStorableFloat("Quad width (m)", 2f, 0.1f, 20f);
            RegisterFloat(quadWidthMeters);

            quadHeightMeters = new JSONStorableFloat("Quad height (m)", 1.125f, 0.1f, 20f);
            RegisterFloat(quadHeightMeters);

            quadLocalPositionX = new JSONStorableFloat("Quad local X", 0f, -10f, 10f);
            RegisterFloat(quadLocalPositionX);

            quadLocalPositionY = new JSONStorableFloat("Quad local Y", 1.6f, -10f, 10f);
            RegisterFloat(quadLocalPositionY);

            quadLocalPositionZ = new JSONStorableFloat("Quad local Z", 2f, -10f, 10f);
            RegisterFloat(quadLocalPositionZ);

            quadLocalEulerPitch = new JSONStorableFloat("Quad pitch (deg)", 0f, -180f, 180f);
            RegisterFloat(quadLocalEulerPitch);

            playVideoAction = new JSONStorableAction("Play / reload video", OnPlayVideoAction);
            RegisterAction(playVideoAction);

            stopVideoAction = new JSONStorableAction("Stop video", OnStopVideoAction);
            RegisterAction(stopVideoAction);

            quadWidthMeters.setCallbackFunction += OnQuadDimensionsChanged;
            quadHeightMeters.setCallbackFunction += OnQuadDimensionsChanged;
            quadLocalPositionX.setCallbackFunction += OnQuadTransformChanged;
            quadLocalPositionY.setCallbackFunction += OnQuadTransformChanged;
            quadLocalPositionZ.setCallbackFunction += OnQuadTransformChanged;
            quadLocalEulerPitch.setCallbackFunction += OnQuadTransformChanged;

            ApplyBootstrapHintsIfPresent();
        }

        private void ApplyBootstrapHintsIfPresent()
        {
            if (!string.IsNullOrEmpty(LocalMp4ViewerBootstrapHints.ParentFreeControllerId))
            {
                parentFreeControllerId.val = LocalMp4ViewerBootstrapHints.ParentFreeControllerId;
                LocalMp4ViewerBootstrapHints.ParentFreeControllerId = null;
            }

            if (!string.IsNullOrEmpty(LocalMp4ViewerBootstrapHints.VideoRelativePath))
            {
                videoRelativePath.val = LocalMp4ViewerBootstrapHints.VideoRelativePath;
                LocalMp4ViewerBootstrapHints.VideoRelativePath = null;
            }

            if (LocalMp4ViewerBootstrapHints.ApplyTelevisionQuadPreset)
            {
                quadWidthMeters.val = 0.95f;
                quadHeightMeters.val = 0.53f;
                quadLocalPositionX.val = 0f;
                quadLocalPositionY.val = 0f;
                quadLocalPositionZ.val = 0.02f;
                quadLocalEulerPitch.val = 0f;
                LocalMp4ViewerBootstrapHints.ApplyTelevisionQuadPreset = false;
            }

            if (LocalMp4ViewerBootstrapHints.ApplyLaptopQuadPreset)
            {
                quadWidthMeters.val = 0.42f;
                quadHeightMeters.val = 0.24f;
                quadLocalPositionX.val = 0f;
                quadLocalPositionY.val = 0f;
                quadLocalPositionZ.val = 0.015f;
                quadLocalEulerPitch.val = 0f;
                LocalMp4ViewerBootstrapHints.ApplyLaptopQuadPreset = false;
            }

            if (LocalMp4ViewerBootstrapHints.ShouldAutoPlayAfterLoad)
            {
                playWhenPluginStarts.val = true;
                LocalMp4ViewerBootstrapHints.ShouldAutoPlayAfterLoad = false;
            }
        }

        private void Start()
        {
            TryCreateVideoQuadHierarchy();

            if (playWhenPluginStarts != null && playWhenPluginStarts.val)
            {
                RequestPrepareAndPlay();
            }
        }

        private void OnDestroy()
        {
            StopPrepareCoroutine();

            if (quadWidthMeters != null)
            {
                quadWidthMeters.setCallbackFunction -= OnQuadDimensionsChanged;
            }

            if (quadHeightMeters != null)
            {
                quadHeightMeters.setCallbackFunction -= OnQuadDimensionsChanged;
            }

            if (quadLocalPositionX != null)
            {
                quadLocalPositionX.setCallbackFunction -= OnQuadTransformChanged;
            }

            if (quadLocalPositionY != null)
            {
                quadLocalPositionY.setCallbackFunction -= OnQuadTransformChanged;
            }

            if (quadLocalPositionZ != null)
            {
                quadLocalPositionZ.setCallbackFunction -= OnQuadTransformChanged;
            }

            if (quadLocalEulerPitch != null)
            {
                quadLocalEulerPitch.setCallbackFunction -= OnQuadTransformChanged;
            }

            if (parentFreeControllerId != null)
            {
                parentFreeControllerId.setCallbackFunction -= OnParentFreeControllerIdChanged;
            }

            TeardownVideoResources();
        }

        private static string GetVaMInstallRootDirectory()
        {
            string dataPath = Application.dataPath;
            dataPath = dataPath.Replace('\\', '/');
            string marker = "/VaM_Data";
            int markerIndex = dataPath.IndexOf(marker, StringComparison.OrdinalIgnoreCase);

            if (markerIndex >= 0)
            {
                return dataPath.Substring(0, markerIndex);
            }

            int slashIndex = dataPath.LastIndexOf('/');

            if (slashIndex > 0)
            {
                return dataPath.Substring(0, slashIndex);
            }

            return dataPath;
        }

        private static string BuildAbsoluteMediaPath(string relativeToVaMInstall)
        {
            string root = GetVaMInstallRootDirectory().Replace('\\', '/').TrimEnd('/');
            string tail = (relativeToVaMInstall ?? "").Trim().Replace('\\', '/').TrimStart('/');
            return root + "/" + tail;
        }

        private static string BuildFileUrlForVideoPlayer(string absolutePathWithForwardSlashes)
        {
            string escaped = absolutePathWithForwardSlashes.Replace(" ", "%20");
            return "file:///" + escaped;
        }

        private Transform ResolveQuadParentTransform()
        {
            if (containingAtom == null)
            {
                return null;
            }

            string fcId = parentFreeControllerId != null ? parentFreeControllerId.val.Trim() : "";

            if (string.IsNullOrEmpty(fcId))
            {
                if (containingAtom.mainController == null)
                {
                    SuperController.LogError("LocalMp4Viewer: mainController missing (empty Parent FreeController id).");
                    return null;
                }

                return containingAtom.mainController.transform;
            }

            FreeControllerV3 fc = containingAtom.GetStorableByID(fcId) as FreeControllerV3;

            if (fc == null || fc.control == null)
            {
                SuperController.LogError(
                    "LocalMp4Viewer: FreeController id \"" + fcId + "\" not found on atom \"" + containingAtom.uid +
                    "\" — quad not created. Fix \"Parent FreeController id\" or leave empty for main controller.");

                return null;
            }

            return fc.control;
        }

        private void OnParentFreeControllerIdChanged(string unusedNewValue)
        {
            if (videoQuadRoot == null)
            {
                TryCreateVideoQuadHierarchy();
                return;
            }

            Transform parentTransform = ResolveQuadParentTransform();

            if (parentTransform == null)
            {
                return;
            }

            videoQuadRoot.transform.SetParent(parentTransform, false);
            ApplyQuadTransformAndVisibilityFromStorables();
        }

        private void TryCreateVideoQuadHierarchy()
        {
            if (containingAtom == null)
            {
                SuperController.LogError("LocalMp4Viewer: containingAtom missing.");
                return;
            }

            if (videoQuadRoot != null)
            {
                return;
            }

            Transform parentTransform = ResolveQuadParentTransform();

            if (parentTransform == null)
            {
                return;
            }

            videoQuadRoot = GameObject.CreatePrimitive(PrimitiveType.Quad);
            videoQuadRoot.name = "LocalMp4Viewer_Quad";

            Collider quadCollider = videoQuadRoot.GetComponent<Collider>();

            if (quadCollider != null)
            {
                Destroy(quadCollider);
            }

            videoQuadRoot.transform.SetParent(parentTransform, false);

            videoMeshRenderer = videoQuadRoot.GetComponent<MeshRenderer>();

            Shader shaderUnlit = Shader.Find("Unlit/Texture");

            if (shaderUnlit == null)
            {
                shaderUnlit = Shader.Find("Unlit/Color");
            }

            if (shaderUnlit == null)
            {
                SuperController.LogError("LocalMp4Viewer: Unlit/Texture shader not found.");
                return;
            }

            videoScreenMaterial = new Material(shaderUnlit);
            videoMeshRenderer.material = videoScreenMaterial;

            videoRenderTexture = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
            videoRenderTexture.Create();

            videoScreenMaterial.mainTexture = videoRenderTexture;

            videoPlayerComponent = videoQuadRoot.AddComponent<VideoPlayer>();
            videoPlayerComponent.playOnAwake = false;
            videoPlayerComponent.skipOnDrop = true;
            videoPlayerComponent.source = VideoSource.Url;
            videoPlayerComponent.renderMode = VideoRenderMode.RenderTexture;
            videoPlayerComponent.targetTexture = videoRenderTexture;
            videoPlayerComponent.audioOutputMode = VideoAudioOutputMode.Direct;

            videoPlayerComponent.errorReceived += OnVideoPlayerErrorReceived;

            ApplyQuadTransformAndVisibilityFromStorables();
        }

        private void OnVideoPlayerErrorReceived(VideoPlayer source, string message)
        {
            SuperController.LogError("LocalMp4Viewer VideoPlayer: " + message);
        }

        private void ApplyQuadTransformAndVisibilityFromStorables()
        {
            if (videoQuadRoot == null)
            {
                return;
            }

            float width = quadWidthMeters != null ? quadWidthMeters.val : 2f;
            float height = quadHeightMeters != null ? quadHeightMeters.val : 1.125f;

            videoQuadRoot.transform.localScale = new Vector3(width, height, 1f);

            float localX = quadLocalPositionX != null ? quadLocalPositionX.val : 0f;
            float localY = quadLocalPositionY != null ? quadLocalPositionY.val : 1.6f;
            float localZ = quadLocalPositionZ != null ? quadLocalPositionZ.val : 2f;

            videoQuadRoot.transform.localPosition = new Vector3(localX, localY, localZ);

            float pitch = quadLocalEulerPitch != null ? quadLocalEulerPitch.val : 0f;

            videoQuadRoot.transform.localEulerAngles = new Vector3(pitch, 180f, 0f);

            bool visible = quadVisible == null || quadVisible.val;

            videoMeshRenderer.enabled = visible;
        }

        private void OnQuadVisibleChanged(bool newVisible)
        {
            if (videoMeshRenderer != null)
            {
                videoMeshRenderer.enabled = newVisible;
            }
        }

        private void OnQuadDimensionsChanged(float unused)
        {
            ApplyQuadTransformAndVisibilityFromStorables();
        }

        private void OnQuadTransformChanged(float unused)
        {
            ApplyQuadTransformAndVisibilityFromStorables();
        }

        private void OnPlayVideoAction()
        {
            RequestPrepareAndPlay();
        }

        private void OnStopVideoAction()
        {
            StopPrepareCoroutine();

            if (videoPlayerComponent != null)
            {
                videoPlayerComponent.Stop();
            }
        }

        private void RequestPrepareAndPlay()
        {
            StopPrepareCoroutine();

            if (this != null && containingAtom != null && containingAtom.gameObject.activeInHierarchy)
            {
                preparePlayCoroutine = StartCoroutine(CoPrepareAndPlay());
            }
        }

        private void StopPrepareCoroutine()
        {
            if (preparePlayCoroutine != null)
            {
                StopCoroutine(preparePlayCoroutine);
                preparePlayCoroutine = null;
            }
        }

        private IEnumerator CoPrepareAndPlay()
        {
            TryCreateVideoQuadHierarchy();

            if (videoPlayerComponent == null || videoRenderTexture == null)
            {
                SuperController.LogError("LocalMp4Viewer: video components not ready.");
                preparePlayCoroutine = null;
                yield break;
            }

            string relativePath = videoRelativePath != null ? videoRelativePath.val : "";

            if (string.IsNullOrEmpty(relativePath))
            {
                SuperController.LogError("LocalMp4Viewer: set \"Video path (relative to VaM folder)\" first.");
                preparePlayCoroutine = null;
                yield break;
            }

            string absolutePath = BuildAbsoluteMediaPath(relativePath);
            string url = BuildFileUrlForVideoPlayer(absolutePath.Replace('\\', '/'));

            videoPlayerComponent.Stop();
            videoPlayerComponent.url = url;
            videoPlayerComponent.isLooping = loopPlayback != null && loopPlayback.val;

            videoPlayerComponent.Prepare();

            int waitedFrames = 0;

            while (!videoPlayerComponent.isPrepared && waitedFrames < PrepareWaitFramesMax)
            {
                waitedFrames++;
                yield return null;
            }

            if (!videoPlayerComponent.isPrepared)
            {
                SuperController.LogError(
                    "LocalMp4Viewer: timed out preparing video. Path: " + absolutePath + " — check codec (H.264 MP4) and file exists.");
                preparePlayCoroutine = null;
                yield break;
            }

            videoPlayerComponent.Play();
            SuperController.LogMessage("LocalMp4Viewer: playing " + absolutePath);
            preparePlayCoroutine = null;
        }

        private void TeardownVideoResources()
        {
            if (videoPlayerComponent != null)
            {
                videoPlayerComponent.errorReceived -= OnVideoPlayerErrorReceived;
                videoPlayerComponent.Stop();
                videoPlayerComponent.targetTexture = null;
            }

            if (videoRenderTexture != null)
            {
                videoRenderTexture.Release();
                Destroy(videoRenderTexture);
                videoRenderTexture = null;
            }

            if (videoScreenMaterial != null)
            {
                Destroy(videoScreenMaterial);
                videoScreenMaterial = null;
            }

            if (videoQuadRoot != null)
            {
                Destroy(videoQuadRoot);
                videoQuadRoot = null;
            }

            videoMeshRenderer = null;
            videoPlayerComponent = null;
        }
    }
}
