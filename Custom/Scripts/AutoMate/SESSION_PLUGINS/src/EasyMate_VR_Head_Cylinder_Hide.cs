using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace geesp0t
{
    /// <summary>
    /// File: <c>EasyMate_VR_Head_Cylinder_Hide.cs</c>. Easy Mate VR head zone: when the HMD eye is inside a **radial band** around a Person’s head (finite cylinder
    /// along possess **up** through <c>headControl.control</c>, 15 cm below to 50 cm above), temporarily hide face
    /// materials and active **Glasses** / **Hat** clothing. **Male** figures: hair is unequipped via
    /// <see cref="DAZCharacterSelector.SetActiveHairItem"/> (restored on leave). **Female** figures:
    /// active hair uses per-eye <c>_AlphaAdjust</c> and Sim-V2 <c>_StandWidth</c> like ImprovedPoV
    /// <c>HairHandler</c> (no unequip).
    /// With <b>VR head proximity hide</b> enabled (Easy Mate storables default on), any Person whose head zone contains the HMD is a hide target
    /// (closest Person along the cylinder test wins when multiple overlap). Zone tests use <see cref="SuperController.centerCameraTarget"/> when present
    /// so left/right eye cameras do not disagree inside a tight radial band (IPD).
    /// Same camera filters as before (VR eye only; not <c>MonitorRig</c> or mirror/reflection cameras).
    /// Skin opaque→transparent swaps and <c>BroadcastMessage</c> run only after all replacement shaders resolve via <c>Shader.Find</c> at configure time
    /// (returns <c>TryAgainLater</c> until VaM exposes those shaders — no static <c>Shader.Find</c> at type load).
    /// Adapted from ImprovedPoV 2.1.1 (Acidbubbles) — https://github.com/acidbubbles/vam-improved-pov
    /// Controlled by Easy Mate storables <b>VR head proximity hide</b> (default on; scene JSON may override).
    /// Skips persons under female Passenger VR hand possession (same step as palm <b>Despossuir</b>) so ImprovedPoV is not doubled.
    /// </summary>
    public static class EasyMateVrHeadCylinderHide
    {
        /// <summary>When true, head-zone material hide runs for any Person whose cylinder contains the HMD (Easy Mate storables default on).</summary>
        private static bool _headProximityHide = false;

        private static bool _shutdownInProgress;

        /// <summary>Person whose skin / hair handlers are configured for the current VO hide pass.</summary>
        private static Atom _hideHandlerPerson;
        private static DAZCharacterSelector _cachedSelector;
        private static SnapSkinHandler _skinHandler;
        private static SnapHairUnequipRestore _hairUnequipRestore;
        private static SnapAccessoryClothingMaterialsHandler _accessoryClothingHandler;
        private static ISnapMaterialHandler _femaleHairAlphaHandler;
        private static bool _hooksRegistered;
        private static bool _handlersConfigured;
        /// <summary>Radial distance from possess-up line through <c>headControl.control</c> (finite segment on that axis).</summary>
        private const float InsideHeadRadiusBaseMeters = 0.065f;
        /// <summary>Radial band radius from possess-up axis through <c>headControl</c> (tighter than legacy ~19 cm).</summary>
        private static readonly float InsideHeadRadiusMeters = InsideHeadRadiusBaseMeters * 1.58740105f;
        /// <summary>
        /// Wider cylinder only while hide handlers are already active — keeps L/R eye cameras agreeing near the tight radius (IPD straddles the band).
        /// </summary>
        private const float HeadZoneRelaxRadiusScaleWhileHiding = 1.28f;
        /// <summary>Along possess-up from <c>headControl.control</c>: toward feet (negative axis).</summary>
        private const float InsideHeadCylinderBelowHeadControlM = 0.15f;
        /// <summary>Along possess-up from <c>headControl.control</c>: toward crown (positive axis).</summary>
        private const float InsideHeadCylinderAboveHeadControlM = 0.50f;
        /// <summary>How often to retry skin/hair setup when skin is not ready yet or Configure failed.</summary>
        private const float TryConfigureHandlersIntervalSeconds = 0.5f;

        private static float _nextPollSkinNullTime = -1f;
        private static float _nextConfigureRetryTime = -1f;

        private static MVRScript _coroutineHost;

        private sealed class HeadZoneScratch
        {
            public Atom Person;
            public Transform LEye;
            public Transform REye;

            public void EnsureEyeCache(Atom person)
            {
                if (person == null)
                    return;
                if (Person == person && LEye != null && REye != null)
                    return;
                Person = person;
                LEye = null;
                REye = null;
                LookAtWithLimits[] eyes = person.GetComponentsInChildren<LookAtWithLimits>(true);
                for (int i = 0; i < eyes.Length; i++)
                {
                    LookAtWithLimits e = eyes[i];
                    if (e == null)
                        continue;
                    if (e.name == "lEye")
                        LEye = e.transform;
                    else if (e.name == "rEye")
                        REye = e.transform;
                }
            }
        }

        private static Dictionary<string, HeadZoneScratch> _headZoneScratchByUid;

        /// <summary>
        /// Restores skin/accessory materials, clears hide-target state, and shows possessor alignment meshes.
        /// Does not unregister camera hooks while VR head proximity hide remains enabled — call after possession clears or similar flows.
        /// </summary>
        public static void RestoreTransientHeadHideState()
        {
            RestoreHandlers();
            SetPossessorPreviewMeshesVisible(true);
            _hideHandlerPerson = null;
            _cachedSelector = null;
            _handlersConfigured = false;
            _nextPollSkinNullTime = -1f;
            _nextConfigureRetryTime = -1f;
        }

        /// <summary>Easy Mate plugin toggle: hide head materials when the HMD is inside any Person’s head cylinder.</summary>
        public static void SetHeadProximityHideEnabled(bool enabled, MVRScript host)
        {
            _headProximityHide = enabled;
            if (host != null)
                _coroutineHost = host;

            SuperController sc = SuperController.singleton;
            if (enabled && sc != null && (sc.isOVR || sc.isOpenVR || XRSettings.enabled))
            {
                RegisterHooks();
            }
            else if (!enabled)
            {
                UnregisterHooks();
            }
        }

        /// <summary>Full teardown (plugin unload).</summary>
        public static void Shutdown()
        {
            _shutdownInProgress = true;
            UnregisterHooks();
            RestoreHandlers();
            SetPossessorPreviewMeshesVisible(true);
            _hideHandlerPerson = null;
            _cachedSelector = null;
            _handlersConfigured = false;
            _nextPollSkinNullTime = -1f;
            _nextConfigureRetryTime = -1f;
            _headZoneScratchByUid = null;
            _coroutineHost = null;
            _shutdownInProgress = false;
        }

        /// <summary>Alias for <see cref="Shutdown"/>.</summary>
        public static void End()
        {
            Shutdown();
        }

        /// <summary>
        /// Call when VaM has finished its loading/settle phase (same moment session <see cref="OnSceneStartup"/> releases its hold).
        /// Re-attaches camera hooks if <see cref="SetHeadProximityHideEnabled"/> left them off because Easy Mate was not
        /// ready yet, or Easy Mate was destroyed on load while the static proximity flag stayed enabled.
        /// </summary>
        public static void AfterSuperControllerFinishedSceneSettle(MVRScript host)
        {
            if (host != null)
            {
                _coroutineHost = host;
            }

            RestoreTransientHeadHideState();

            if (!_headProximityHide)
            {
                return;
            }

            SuperController sc = SuperController.singleton;
            if (sc == null)
            {
                return;
            }

            if (!sc.isOVR && !sc.isOpenVR && !XRSettings.enabled)
            {
                return;
            }

            RegisterHooks();
        }

        private static HeadZoneScratch GetHeadZoneScratch(Atom person)
        {
            if (person == null)
                return null;
            if (_headZoneScratchByUid == null)
                _headZoneScratchByUid = new Dictionary<string, HeadZoneScratch>();
            HeadZoneScratch s;
            if (!_headZoneScratchByUid.TryGetValue(person.uid, out s))
            {
                s = new HeadZoneScratch();
                _headZoneScratchByUid[person.uid] = s;
            }

            s.EnsureEyeCache(person);
            return s;
        }

        /// <summary>
        /// Stereo L/R eye cameras are IPD apart; <see cref="SuperController.centerCameraTarget"/> is a single rig point so both eyes share one in/out test.
        /// </summary>
        private static Vector3 ResolveHeadZoneProbeWorldPosition(SuperController sc, Camera invokingEyeCamera)
        {
            if (sc != null && sc.centerCameraTarget != null)
            {
                return sc.centerCameraTarget.transform.position;
            }

            if (sc != null && sc.lookCamera != null)
            {
                return sc.lookCamera.transform.position;
            }

            if (invokingEyeCamera != null)
            {
                return invokingEyeCamera.transform.position;
            }

            return Vector3.zero;
        }

        private static bool TryGetRadialSqInHeadZone(HeadZoneScratch scratch, FreeControllerV3 head, Vector3 probeWorldPosition, float radiusScale, out float radialSq)
        {
            radialSq = float.MaxValue;
            if (scratch == null || head == null)
                return false;
            Atom person = scratch.Person;
            if (person == null || !person.gameObject.activeInHierarchy)
                return false;
            if (head.control == null)
                return false;

            Vector3 axis = head.GetUpPossessAxis();
            if (axis.sqrMagnitude < 1e-12f)
                axis = head.control.up;
            axis.Normalize();

            Vector3 origin = head.control.position;
            Vector3 w = probeWorldPosition - origin;
            float axial = Vector3.Dot(w, axis);
            if (axial < -InsideHeadCylinderBelowHeadControlM || axial > InsideHeadCylinderAboveHeadControlM)
                return false;

            Vector3 radial = w - axis * axial;
            float r2 = radial.sqrMagnitude;
            float radiusMeters = InsideHeadRadiusMeters * radiusScale;
            float radiusSqr = radiusMeters * radiusMeters;
            if (r2 < radiusSqr)
            {
                radialSq = r2;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Strict zone pick, then sticky relaxed zone for the current hide target so border frames do not split stereo passes.
        /// </summary>
        private static void ResolveHeadHideTargetForCamera(Camera cam, out Atom bestAtom, out FreeControllerV3 bestHead)
        {
            bestAtom = null;
            bestHead = null;
            if (cam == null)
                return;

            SuperController sc = SuperController.singleton;
            if (sc == null || !_headProximityHide)
                return;

            Vector3 probe = ResolveHeadZoneProbeWorldPosition(sc, cam);
            FreeControllerV3 strictHead;
            Atom strictPerson = PickClosestPersonInHeadZone(probe, 1f, out strictHead);
            if (strictPerson != null)
            {
                bestAtom = strictPerson;
                bestHead = strictHead;
                return;
            }

            if (_hideHandlerPerson == null || !_handlersConfigured)
                return;

            if (ShouldSuppressForPassengerImprovedPoV(_hideHandlerPerson))
                return;

            FreeControllerV3 heldHead = _hideHandlerPerson.GetStorableByID("headControl") as FreeControllerV3;
            if (heldHead == null)
                return;

            float unusedRsq;
            if (!TryGetRadialSqInHeadZone(GetHeadZoneScratch(_hideHandlerPerson), heldHead, probe, HeadZoneRelaxRadiusScaleWhileHiding, out unusedRsq))
                return;

            bestAtom = _hideHandlerPerson;
            bestHead = heldHead;
        }

        private static Atom PickClosestPersonInHeadZone(Vector3 probeWorldPosition, float radiusScale, out FreeControllerV3 headOut)
        {
            headOut = null;
            SuperController sc = SuperController.singleton;
            if (sc == null)
                return null;

            Atom bestPerson = null;
            FreeControllerV3 bestHead = null;
            float bestRsq = float.MaxValue;

            foreach (Atom a in sc.GetAtoms())
            {
                if (a == null || a.type != "Person" || !a.gameObject.activeInHierarchy || a.hidden)
                    continue;
                if (ShouldSuppressForPassengerImprovedPoV(a))
                    continue;
                FreeControllerV3 head = a.GetStorableByID("headControl") as FreeControllerV3;
                if (head == null || head.control == null)
                    continue;
                float rsq;
                if (!TryGetRadialSqInHeadZone(GetHeadZoneScratch(a), head, probeWorldPosition, radiusScale, out rsq))
                    continue;
                if (rsq < bestRsq)
                {
                    bestRsq = rsq;
                    bestPerson = a;
                    bestHead = head;
                }
            }

            headOut = bestHead;
            return bestPerson;
        }

        private static bool ShouldSuppressForPassengerImprovedPoV(Atom person)
        {
            if (person == null)
                return false;

            JSONStorable improvedPoVStorable =
                FindPluginStorableByClassSuffix(person, "ImprovedPoV");
            if (improvedPoVStorable == null)
                return false;

            JSONStorableBool hideFaceBool =
                improvedPoVStorable.GetBoolJSONParam("Hide face");
            JSONStorableBool hideHairBool =
                improvedPoVStorable.GetBoolJSONParam("Hide hair");
            JSONStorableBool possessedOnlyBool =
                improvedPoVStorable.GetBoolJSONParam("Activate only when possessed");

            if (possessedOnlyBool == null || possessedOnlyBool.val)
                return false;

            bool hideFace = hideFaceBool != null && hideFaceBool.val;
            bool hideHair = hideHairBool != null && hideHairBool.val;
            return hideFace || hideHair;
        }

        private static JSONStorable FindPluginStorableByClassSuffix(
            Atom atom,
            string classSuffix)
        {
            if (atom == null || string.IsNullOrEmpty(classSuffix))
            {
                return null;
            }

            List<string> storableIds = atom.GetStorableIDs();
            if (storableIds == null)
            {
                return null;
            }

            for (int storableIndex = 0;
                storableIndex < storableIds.Count;
                storableIndex++)
            {
                string storableId = storableIds[storableIndex];
                if (string.IsNullOrEmpty(storableId))
                {
                    continue;
                }

                if (!storableId.StartsWith("plugin#", StringComparison.Ordinal))
                {
                    continue;
                }

                if (!storableId.EndsWith(classSuffix, StringComparison.Ordinal))
                {
                    continue;
                }

                return atom.GetStorableByID(storableId);
            }

            return null;
        }

        private static void RegisterHooks()
        {
            if (_hooksRegistered)
                return;
            Camera.onPreRender += OnPreRender;
            Camera.onPostRender += OnPostRender;
            _hooksRegistered = true;
        }

        private static void UnregisterHooks()
        {
            if (!_hooksRegistered)
                return;
            Camera.onPreRender -= OnPreRender;
            Camera.onPostRender -= OnPostRender;
            _hooksRegistered = false;
        }

        private static bool IsCameraUnderMirrorOrReflectionHierarchy(Camera cam)
        {
            if (cam == null)
                return false;
            Transform t = cam.transform;
            for (int depth = 0; depth < 48 && t != null; depth++)
            {
                string n = t.name;
                if (!string.IsNullOrEmpty(n))
                {
                    if (n.IndexOf("Mirror", StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                    if (n.IndexOf("Reflect", StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                    if (n.IndexOf("Reflection", StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                    if (n.IndexOf("Planar", StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }
                t = t.parent;
            }
            return false;
        }

        /// <summary>
        /// Only the HMD eye passes get the in-head material hide — not monitor output or mirror/reflection cameras,
        /// so you still see your head in mirrors while your own view stays clear.
        /// </summary>
        private static bool ShouldApplyHeadHideForThisCamera(Camera cam)
        {
            if (cam == null)
                return false;
            SuperController sc = SuperController.singleton;
            if (sc == null)
                return false;
            if (!_headProximityHide)
                return false;
            if (!sc.isOVR && !sc.isOpenVR && !XRSettings.enabled)
                return false;
            if (cam.name == "MonitorRig")
                return false;
            if (IsCameraUnderMirrorOrReflectionHierarchy(cam))
                return false;
            return cam.name == "CenterEyeAnchor" || cam.name == "Camera (eye)";
        }

        private static void RestoreHeadStraightFacing(Atom person, FreeControllerV3 head)
        {
            if (person == null || head == null || head.control == null)
                return;
            try
            {
                Vector3 up = head.GetUpPossessAxis();
                if (up.sqrMagnitude < 1e-12f)
                    up = head.control != null ? head.control.up : Vector3.up;
                up.Normalize();

                Vector3 fwd;
                FreeControllerV3 chest = person.GetStorableByID("chestControl") as FreeControllerV3;
                if (chest != null && chest.control != null)
                {
                    fwd = Vector3.ProjectOnPlane(chest.control.forward, up);
                    if (fwd.sqrMagnitude < 1e-10f)
                        fwd = Vector3.ProjectOnPlane(head.GetForwardPossessAxis(), up);
                }
                else
                    fwd = Vector3.ProjectOnPlane(head.GetForwardPossessAxis(), up);

                if (fwd.sqrMagnitude < 1e-10f)
                    return;
                fwd.Normalize();

                head.control.rotation = Quaternion.LookRotation(fwd, up);
            }
            catch (Exception e)
            {
                SuperController.LogError("EasyMateVrHeadCylinderHide: restore head facing failed: " + e.Message);
            }
        }

        private static void EnsureHideHandlersMatchZoneOwner(Atom best)
        {
            if (best == _hideHandlerPerson)
                return;

            RestoreHandlers();
            _handlersConfigured = false;
            _hideHandlerPerson = best;
            if (best != null)
            {
                GetHeadZoneScratch(best);
                _cachedSelector = best.GetComponentInChildren<DAZCharacterSelector>();
            }
            else
            {
                _cachedSelector = null;
            }
        }

        private static void OnPreRender(Camera cam)
        {
            if (!ShouldApplyHeadHideForThisCamera(cam))
                return;

            FreeControllerV3 bestHead;
            Atom best;
            ResolveHeadHideTargetForCamera(cam, out best, out bestHead);
            EnsureHideHandlersMatchZoneOwner(best);

            if (best == null || bestHead == null)
                return;

            if (!_handlersConfigured)
                TryConfigureHandlers();

            if (_skinHandler == null)
            {
                return;
            }

            try
            {
                _skinHandler?.BeforeRender();
                _femaleHairAlphaHandler?.BeforeRender();
                _accessoryClothingHandler?.BeforeRender();
            }
            catch (Exception e)
            {
                SuperController.LogError("EasyMateVrHeadCylinderHide pre-render: " + e);
            }
        }

        private static void OnPostRender(Camera cam)
        {
            if (!ShouldApplyHeadHideForThisCamera(cam))
                return;

            FreeControllerV3 bestHead;
            Atom best;
            ResolveHeadHideTargetForCamera(cam, out best, out bestHead);
            if (best == null || bestHead == null)
                return;
            if (best != _hideHandlerPerson)
                return;

            try
            {
                _skinHandler?.AfterRender();
                _femaleHairAlphaHandler?.AfterRender();
                _accessoryClothingHandler?.AfterRender();
            }
            catch (Exception e)
            {
                SuperController.LogError("EasyMateVrHeadCylinderHide post-render: " + e);
            }
        }

        private static void TryConfigureHandlers()
        {
            if (_handlersConfigured || _hideHandlerPerson == null)
                return;

            if (_cachedSelector == null)
            {
                _handlersConfigured = true;
                return;
            }

            float t = Time.time;

            SuperController scLoad = SuperController.singleton;
            if (scLoad != null && scLoad.isLoading)
            {
                _nextConfigureRetryTime = t + TryConfigureHandlersIntervalSeconds;
                return;
            }

            if (_cachedSelector.selectedCharacter?.skin == null)
            {
                if (t < _nextPollSkinNullTime)
                    return;
                _nextPollSkinNullTime = t + TryConfigureHandlersIntervalSeconds;
                return;
            }

            if (t < _nextConfigureRetryTime)
                return;

            DAZCharacter character = _cachedSelector.selectedCharacter;
            _skinHandler = new SnapSkinHandler();
            if (_skinHandler.Configure(character.skin) != SnapHandlerConfigurationResult.Success)
            {
                _skinHandler = null;
                _nextConfigureRetryTime = t + TryConfigureHandlersIntervalSeconds;
                return;
            }

            _nextConfigureRetryTime = -1f;

            if (character.isMale)
            {
                _hairUnequipRestore = SnapHairUnequipRestore.TryApply(_cachedSelector);
                _femaleHairAlphaHandler = null;
            }
            else
            {
                _hairUnequipRestore = null;
                _femaleHairAlphaHandler = SnapFemaleHairAlphaHandler.TryBuild(_cachedSelector);
            }

            _accessoryClothingHandler = SnapAccessoryClothingMaterialsHandler.TryBuild(_cachedSelector);

            _handlersConfigured = true;
        }

        private static void RestoreHandlers()
        {
            _skinHandler?.Restore();
            _skinHandler = null;
            _hairUnequipRestore?.Restore();
            _hairUnequipRestore = null;
            _femaleHairAlphaHandler?.Restore();
            _femaleHairAlphaHandler = null;
            _accessoryClothingHandler?.Restore();
            _accessoryClothingHandler = null;
        }

        private static readonly string[] PossessorPreviewMeshNames = { "Capsule", "Sphere1", "Sphere2" };

        private static void SetPossessorPreviewMeshesVisible(bool visible)
        {
            try
            {
                SuperController sc = SuperController.singleton;
                if (sc == null || sc.centerCameraTarget == null)
                    return;
                Transform root = sc.centerCameraTarget.transform;
                SetMatchingDescendantsActive(root, visible, PossessorPreviewMeshNames);
            }
            catch
            {
            }
        }

        private static void SetMatchingDescendantsActive(Transform t, bool visible, string[] names)
        {
            if (t == null || names == null)
                return;
            for (int n = 0; n < names.Length; n++)
            {
                if (t.name == names[n])
                {
                    t.gameObject.SetActive(visible);
                    break;
                }
            }

            for (int c = 0; c < t.childCount; c++)
                SetMatchingDescendantsActive(t.GetChild(c), visible, names);
        }

        /// <summary>Hides possessor alignment preview meshes under <see cref="SuperController.centerCameraTarget"/>.</summary>
        public static void HidePossessorAlignmentPreviewMeshes()
        {
            SetPossessorPreviewMeshesVisible(false);
        }

        public static class SnapHandlerConfigurationResult
        {
            public const int Success = 0;
            public const int CannotApply = 1;
            public const int TryAgainLater = 2;
        }

        /// <summary>
        /// Backs up each <see cref="DAZHairGroup"/>'s <c>active</c> flag, unequips hair via the selector (same as UI),
        /// then restores prior flags on <see cref="Restore"/>.
        /// </summary>
        private sealed class SnapHairUnequipRestore
        {
            private sealed class HairRow
            {
                public DAZHairGroup Hair;
                public bool WasActive;
            }

            private DAZCharacterSelector _selector;
            private List<HairRow> _rows;

            public static SnapHairUnequipRestore TryApply(DAZCharacterSelector selector)
            {
                if (selector?.hairItems == null)
                    return null;

                var rows = new List<HairRow>();
                foreach (DAZHairGroup h in selector.hairItems)
                {
                    if (h == null || h.name == "NoHair")
                        continue;

                    bool wasActive = h.active;
                    rows.Add(new HairRow { Hair = h, WasActive = wasActive });
                    if (wasActive)
                        selector.SetActiveHairItem(h, false, false);
                }

                return new SnapHairUnequipRestore { _selector = selector, _rows = rows };
            }

            public void Restore()
            {
                if (_rows == null || _selector == null)
                    return;

                for (int i = 0; i < _rows.Count; i++)
                {
                    HairRow r = _rows[i];
                    if (r == null || r.Hair == null)
                        continue;
                    if (r.WasActive)
                        _selector.SetActiveHairItem(r.Hair, true, false);
                }

                _rows = null;
                _selector = null;
            }
        }

        /// <summary>
        /// Female hair inside the head zone: matches ImprovedPoV <c>HairHandler</c> — mesh materials
        /// use <c>_AlphaAdjust</c> per eye; Sim2 / Custom hair also drives strand <c>_StandWidth</c>.
        /// SimHairGroup / SimHairGroup2 are skipped (same as ImprovedPoV).
        /// </summary>
        private sealed class SnapFemaleHairAlphaHandler : ISnapMaterialHandler
        {
            private sealed class AlphaMatRow
            {
                public Material material;
                public float originalAlphaAdjust;
            }

            private sealed class SimStrandRow
            {
                public Material strandMaterial;
                public string shaderPropertyName;
                public float hiddenValue;
                public float originalValue;
            }

            private List<AlphaMatRow> _alphaRows;
            private List<SimStrandRow> _strandRows;

            public static SnapFemaleHairAlphaHandler TryBuild(DAZCharacterSelector selector)
            {
                if (selector == null || selector.hairItems == null)
                    return null;

                Dictionary<int, AlphaMatRow> alphaById = new Dictionary<int, AlphaMatRow>();
                List<SimStrandRow> strandRows = new List<SimStrandRow>();

                for (int hi = 0; hi < selector.hairItems.Length; hi++)
                {
                    DAZHairGroup hair = selector.hairItems[hi];
                    if (hair == null || !hair.active || hair.name == "NoHair")
                        continue;

                    string hn = hair.name;
                    if (hn == "Sim2Hair" || hn == "Sim2HairMale" || hn == "CustomHairItem")
                    {
                        AccumulateScalpAlphaMaterials(hair, alphaById);
                        MeshRenderer strandRend = hair.GetComponentInChildren<MeshRenderer>();
                        Material strand = strandRend != null ? strandRend.material : null;
                        if (strand != null)
                        {
                            string propName = "_StandWidth";
                            strandRows.Add(new SimStrandRow
                            {
                                strandMaterial = strand,
                                shaderPropertyName = propName,
                                hiddenValue = 0f,
                                originalValue = strand.GetFloat(propName)
                            });
                        }
                    }
                    else if (hn == "SimHairGroup" || hn == "SimHairGroup2")
                    {
                        continue;
                    }
                    else
                    {
                        AccumulateSimpleHairAlphaMaterials(hair, alphaById);
                    }
                }

                if (alphaById.Count == 0 && strandRows.Count == 0)
                    return null;

                SnapFemaleHairAlphaHandler h = new SnapFemaleHairAlphaHandler();
                h._alphaRows = alphaById.Values.ToList();
                h._strandRows = strandRows;
                return h;
            }

            private static void AccumulateScalpAlphaMaterials(DAZHairGroup hair, Dictionary<int, AlphaMatRow> alphaById)
            {
                DAZSkinWrap[] wraps = hair.GetComponentsInChildren<DAZSkinWrap>();
                for (int i = 0; i < wraps.Length; i++)
                {
                    DAZSkinWrap w = wraps[i];
                    if (w == null || w.GPUmaterials == null)
                        continue;

                    Material[] mats = w.GPUmaterials;
                    for (int j = 0; j < mats.Length; j++)
                        AddAlphaMaterialUnique(mats[j], alphaById);
                }
            }

            private static void AccumulateSimpleHairAlphaMaterials(DAZHairGroup hair, Dictionary<int, AlphaMatRow> alphaById)
            {
                DAZMesh[] meshes = hair.GetComponentsInChildren<DAZMesh>();
                for (int i = 0; i < meshes.Length; i++)
                {
                    DAZMesh mesh = meshes[i];
                    if (mesh == null)
                        continue;

                    Material[] mats = mesh.materials;
                    if (mats == null)
                        continue;

                    for (int j = 0; j < mats.Length; j++)
                        AddAlphaMaterialUnique(mats[j], alphaById);
                }

                AccumulateScalpAlphaMaterials(hair, alphaById);
            }

            private static void AddAlphaMaterialUnique(Material m, Dictionary<int, AlphaMatRow> alphaById)
            {
                if (m == null)
                    return;
                int instanceId = m.GetInstanceID();
                if (alphaById.ContainsKey(instanceId))
                    return;

                alphaById[instanceId] = new AlphaMatRow
                {
                    material = m,
                    originalAlphaAdjust = m.GetFloat("_AlphaAdjust")
                };
            }

            public void BeforeRender()
            {
                ApplyStrandWidths(false);
                ApplyAlphaMaterials(false);
            }

            public void AfterRender()
            {
                ApplyStrandWidths(true);
                ApplyAlphaMaterials(true);
            }

            private void ApplyStrandWidths(bool restoreOriginal)
            {
                if (_strandRows == null)
                    return;

                for (int i = 0; i < _strandRows.Count; i++)
                {
                    SimStrandRow row = _strandRows[i];
                    if (row == null || row.strandMaterial == null)
                        continue;
                    float nextVal = restoreOriginal ? row.originalValue : row.hiddenValue;
                    row.strandMaterial.SetFloat(row.shaderPropertyName, nextVal);
                }
            }

            private void ApplyAlphaMaterials(bool restoreOriginal)
            {
                if (_alphaRows == null)
                    return;

                for (int i = 0; i < _alphaRows.Count; i++)
                {
                    AlphaMatRow row = _alphaRows[i];
                    if (row == null || row.material == null)
                        continue;
                    float adj = restoreOriginal ? row.originalAlphaAdjust : -1f;
                    row.material.SetFloat("_AlphaAdjust", adj);
                }
            }

            public void Restore()
            {
                AfterRender();
                _alphaRows = null;
                _strandRows = null;
            }
        }

        public interface ISnapMaterialHandler
        {
            void Restore();
            void BeforeRender();
            void AfterRender();
        }

        public class SnapSkinHandler : ISnapMaterialHandler
        {
            public class SkinShaderMaterialReference
            {
                public Material material;
                public Shader originalShader;
                public float originalAlphaAdjust;
                public float originalColorAlpha;
                public Color originalSpecColor;

                public static SkinShaderMaterialReference FromMaterial(Material material)
                {
                    var materialRef = new SkinShaderMaterialReference();
                    materialRef.material = material;
                    materialRef.originalShader = material.shader;
                    materialRef.originalAlphaAdjust = material.GetFloat("_AlphaAdjust");
                    materialRef.originalColorAlpha = material.GetColor("_Color").a;
                    materialRef.originalSpecColor = material.GetColor("_SpecColor");
                    return materialRef;
                }
            }

            public static readonly string[] MaterialsToHide =
            {
                "Lacrimals", "Pupils", "Lips", "Gums", "Irises", "Teeth", "Face", "Head", "InnerMouth", "Tongue",
                "EyeReflection", "Nostrils", "Cornea", "Eyelashes", "Sclera", "Ears", "Tear"
            };

            private static IList<Material> GetMaterialsToHide(DAZSkinV2 skin)
            {
                var materials = new List<Material>(MaterialsToHide.Length);
                foreach (Material material in skin.GPUmaterials)
                {
                    if (material == null)
                        continue;
                    if (!MaterialsToHide.Any(materialToHide => material.name.StartsWith(materialToHide, StringComparison.OrdinalIgnoreCase)))
                        continue;
                    materials.Add(material);
                }

                return materials;
            }

            /// <summary>Opaque shader name → transparent replacement name (<c>null</c> = no swap). Resolved with <see cref="Shader.Find"/> at configure time so VaM has registered shaders.</summary>
            private static readonly Dictionary<string, string> ReplacementShaderNames = new Dictionary<string, string>
            {
                { "Custom/Subsurface/GlossCullComputeBuff", "Custom/Subsurface/TransparentGlossSeparateAlphaComputeBuff" },
                { "Custom/Subsurface/GlossNMCullComputeBuff", "Custom/Subsurface/TransparentGlossNMSeparateAlphaComputeBuff" },
                { "Custom/Subsurface/GlossNMDetailCullComputeBuff", "Custom/Subsurface/TransparentGlossNMDetailNoCullSeparateAlphaComputeBuff" },
                { "Custom/Subsurface/CullComputeBuff", "Custom/Subsurface/TransparentSeparateAlphaComputeBuff" },
                { "Custom/Subsurface/TransparentGlossSeparateAlphaComputeBuff", null },
                { "Custom/Subsurface/TransparentGlossNMSeparateAlphaComputeBuff", null },
                { "Custom/Subsurface/TransparentSeparateAlphaComputeBuff", null },
                { "Custom/Subsurface/TransparentGlossNMDetailNoCullSeparateAlphaComputeBuff", null },
                { "Custom/Subsurface/TransparentGlossNoCullSeparateAlphaComputeBuff", null },
                { "Custom/Subsurface/TransparentGlossComputeBuff", null },
                { "Custom/Subsurface/TransparentComputeBuff", null },
                { "Custom/Subsurface/AlphaMaskComputeBuff", null },
                { "Marmoset/Transparent/Simple Glass/Specular IBLComputeBuff", null },
            };

            private static string NormalizeMaterialShaderName(string raw)
            {
                if (string.IsNullOrEmpty(raw))
                    return null;
                string s = raw.Trim().Trim('\'', '"');
                return s.Length == 0 ? null : s;
            }

            /// <summary>Optional opaque→transparent swap name for a skin material (<c>null</c> = none). Read-only.</summary>
            private static string GetReplacementShaderNameForSkinMaterial(Material material)
            {
                if (material == null)
                    return null;
                string shaderName = material.shader != null ? NormalizeMaterialShaderName(material.shader.name) : null;
                string replacementName = null;
                bool mapped = shaderName != null && ReplacementShaderNames.TryGetValue(shaderName, out replacementName);
                if (!mapped)
                {
                    if (shaderName != null && shaderName.IndexOf("Custom/Subsurface/Transparent", StringComparison.Ordinal) >= 0)
                        replacementName = null;
                    else
                        replacementName = null;
                }

                return replacementName;
            }

            private DAZSkinV2 _skin;
            private List<SkinShaderMaterialReference> _materialRefs;

            public int Configure(DAZSkinV2 skin)
            {
                _skin = skin;
                _materialRefs = null;

                IList<Material> hideSet = GetMaterialsToHide(skin);
                foreach (Material material in hideSet)
                {
                    if (material == null)
                        continue;
                    string replacementName = GetReplacementShaderNameForSkinMaterial(material);
                    if (!string.IsNullOrEmpty(replacementName) && Shader.Find(replacementName) == null)
                    {
                        _skin = null;
                        return SnapHandlerConfigurationResult.TryAgainLater;
                    }
                }

                _materialRefs = new List<SkinShaderMaterialReference>();

                foreach (Material material in hideSet)
                {
                    if (material == null)
                        continue;
                    SkinShaderMaterialReference materialInfo = SkinShaderMaterialReference.FromMaterial(material);
                    string shaderName = material.shader != null ? NormalizeMaterialShaderName(material.shader.name) : null;
                    string replacementName = null;
                    bool mapped = shaderName != null && ReplacementShaderNames.TryGetValue(shaderName, out replacementName);
                    Shader shader = null;
                    if (!mapped)
                        replacementName = null;

                    if (!string.IsNullOrEmpty(replacementName))
                    {
                        shader = Shader.Find(replacementName);
                        if (shader == null)
                        {
                            _materialRefs = null;
                            _skin = null;
                            return SnapHandlerConfigurationResult.TryAgainLater;
                        }
                    }

                    if (shader != null)
                        material.shader = shader;

                    _materialRefs.Add(materialInfo);
                }

                skin.BroadcastMessage("OnApplicationFocus", true);
                return SnapHandlerConfigurationResult.Success;
            }

            public void Restore()
            {
                if (_materialRefs == null)
                    return;
                foreach (SkinShaderMaterialReference row in _materialRefs)
                {
                    if (row == null || row.material == null)
                        continue;
                    if (row.originalShader != null)
                        row.material.shader = row.originalShader;
                    RestoreMaterialAlphaFromRef(row);
                }

                _materialRefs = null;
                if (_skin != null)
                    _skin.BroadcastMessage("OnApplicationFocus", true);
            }

            public void BeforeRender()
            {
                if (_materialRefs == null)
                    return;
                foreach (SkinShaderMaterialReference materialRef in _materialRefs)
                {
                    Material m = materialRef != null ? materialRef.material : null;
                    if (m == null)
                        continue;
                    m.SetFloat("_AlphaAdjust", -1f);
                    Color color = m.GetColor("_Color");
                    m.SetColor("_Color", new Color(color.r, color.g, color.b, 0f));
                    m.SetColor("_SpecColor", new Color(0f, 0f, 0f, 0f));
                }
            }

            public void AfterRender()
            {
                if (_materialRefs == null)
                    return;
                foreach (SkinShaderMaterialReference materialRef in _materialRefs)
                    RestoreMaterialAlphaFromRef(materialRef);
            }

            private static void RestoreMaterialAlphaFromRef(SkinShaderMaterialReference materialRef)
            {
                Material m = materialRef != null ? materialRef.material : null;
                if (m == null)
                    return;
                m.SetFloat("_AlphaAdjust", materialRef.originalAlphaAdjust);
                Color color = m.GetColor("_Color");
                m.SetColor("_Color", new Color(color.r, color.g, color.b, materialRef.originalColorAlpha));
                m.SetColor("_SpecColor", materialRef.originalSpecColor);
            }
        }

        /// <summary>Lowercase blob from <see cref="DAZClothingItem"/> display name and tags (same idea as Easy Mate <c>ClothingSearchBlob</c>).</summary>
        private static string AccessoryClothingSearchBlob(DAZClothingItem item)
        {
            if (item == null)
                return string.Empty;
            string s = " " + (item.displayName ?? "") + " " + (item.tags ?? "") + " ";
            if (item.tagsArray != null)
            {
                foreach (string t in item.tagsArray)
                {
                    if (!string.IsNullOrEmpty(t))
                        s += t + " ";
                }
            }

            return s.ToLowerInvariant();
        }

        /// <summary>
        /// Eyewear not always flagged <see cref="DAZClothingItem.ExclusiveRegion.Glasses"/> (e.g. &quot;Heatwave Sunglasses&quot;).
        /// Substrings avoid bare <c>glass</c> (hourglass, fiberglass, …).
        /// </summary>
        private static bool ClothingLooksLooselyLikeGlasses(DAZClothingItem item)
        {
            string blob = AccessoryClothingSearchBlob(item);
            string[] keys =
            {
                "glasses", "sunglass", "goggle", "monocle", "spectacle", "eyewear", "shades"
            };
            for (int i = 0; i < keys.Length; i++)
            {
                if (blob.IndexOf(keys[i], StringComparison.Ordinal) >= 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Active <see cref="DAZClothingItem"/> with <see cref="DAZClothingItem.ExclusiveRegion.Hat"/>, or glasses
        /// (<see cref="DAZClothingItem.ExclusiveRegion.Glasses"/> or loose name/tag match via <see cref="ClothingLooksLooselyLikeGlasses"/>).
        /// Same per-eye alpha trick as hair so VR eyes do not clip through props while mirrors stay unchanged.
        /// </summary>
        private sealed class SnapAccessoryClothingMaterialsHandler : ISnapMaterialHandler
        {
            private class AccessoryMaterialRef
            {
                public Material material;
                public bool hideViaAlphaAdjust;
                public float originalAlphaAdjust;
                public bool hideViaColorAlpha;
                public float originalColorAlpha;
            }

            private List<AccessoryMaterialRef> _refs;

            public static SnapAccessoryClothingMaterialsHandler TryBuild(DAZCharacterSelector selector)
            {
                if (selector?.clothingItems == null)
                    return null;

                var list = new List<AccessoryMaterialRef>();
                var seen = new HashSet<int>();

                foreach (DAZClothingItem item in selector.clothingItems)
                {
                    if (item == null || !item.active)
                        continue;
                    DAZClothingItem.ExclusiveRegion r = item.exclusiveRegion;
                    bool hat = r == DAZClothingItem.ExclusiveRegion.Hat;
                    bool glasses = r == DAZClothingItem.ExclusiveRegion.Glasses || ClothingLooksLooselyLikeGlasses(item);
                    if (!hat && !glasses)
                        continue;

                    Renderer[] rends = item.GetComponentsInChildren<Renderer>(true);
                    for (int ri = 0; ri < rends.Length; ri++)
                    {
                        Renderer rend = rends[ri];
                        if (rend == null)
                            continue;
                        Material[] mats;
                        try
                        {
                            mats = rend.materials;
                        }
                        catch
                        {
                            continue;
                        }

                        for (int mi = 0; mi < mats.Length; mi++)
                        {
                            Material m = mats[mi];
                            if (m == null)
                                continue;
                            if (!seen.Add(m.GetInstanceID()))
                                continue;

                            var row = new AccessoryMaterialRef { material = m };
                            if (m.HasProperty("_AlphaAdjust"))
                            {
                                row.hideViaAlphaAdjust = true;
                                row.originalAlphaAdjust = m.GetFloat("_AlphaAdjust");
                            }

                            if (m.HasProperty("_Color"))
                            {
                                row.hideViaColorAlpha = true;
                                row.originalColorAlpha = m.GetColor("_Color").a;
                            }

                            if (row.hideViaAlphaAdjust || row.hideViaColorAlpha)
                                list.Add(row);
                        }
                    }
                }

                if (list.Count == 0)
                    return null;
                return new SnapAccessoryClothingMaterialsHandler { _refs = list };
            }

            public void Restore()
            {
                if (_refs == null)
                    return;
                foreach (AccessoryMaterialRef r in _refs)
                {
                    if (r.material == null)
                        continue;
                    if (r.hideViaAlphaAdjust && r.material.HasProperty("_AlphaAdjust"))
                        r.material.SetFloat("_AlphaAdjust", r.originalAlphaAdjust);
                    if (r.hideViaColorAlpha && r.material.HasProperty("_Color"))
                    {
                        Color c = r.material.GetColor("_Color");
                        r.material.SetColor("_Color", new Color(c.r, c.g, c.b, r.originalColorAlpha));
                    }
                }

                _refs = null;
            }

            public void BeforeRender()
            {
                if (_refs == null)
                    return;
                foreach (AccessoryMaterialRef r in _refs)
                {
                    if (r.material == null)
                        continue;
                    if (r.hideViaAlphaAdjust)
                        r.material.SetFloat("_AlphaAdjust", -1f);
                    if (r.hideViaColorAlpha)
                    {
                        Color c = r.material.GetColor("_Color");
                        r.material.SetColor("_Color", new Color(c.r, c.g, c.b, 0f));
                    }
                }
            }

            public void AfterRender()
            {
                if (_refs == null)
                    return;
                foreach (AccessoryMaterialRef r in _refs)
                {
                    if (r.material == null)
                        continue;
                    if (r.hideViaAlphaAdjust)
                        r.material.SetFloat("_AlphaAdjust", r.originalAlphaAdjust);
                    if (r.hideViaColorAlpha)
                    {
                        Color c = r.material.GetColor("_Color");
                        r.material.SetColor("_Color", new Color(c.r, c.g, c.b, r.originalColorAlpha));
                    }
                }
            }
        }
    }
}
