using System;
using System.Reflection;
using MeshVR;
using UnityEngine;
using UnityEngine.XR;

namespace geesp0t
{
    /// <summary>
    /// Per-hand VR grip press toggles between articulated VR hands (full collision-capable mode) and VaM’s
    /// sphere / kinematic hand mode (second menu option — discovered via reflection on enum motion properties).
    /// Hands stay enabled so sphere proxies remain visible; <see cref="HandModelControl.useCollision"/> is off when
    /// neither side is articulated (both spheres). Turning articulated mode <b>on</b> for a side is skipped while
    /// that side’s Person hand control is <see cref="FreeControllerV3.possessed"/>.
    /// When articulated, selects <b>Male2</b> (or <b>Male 2</b>) when available.
    /// <see cref="DisableVrHandModelsForSceneStart"/> starts both sides in sphere mode (compact); see also decompiled
    /// <c>MeshVR.HandModelControl</c> under <c>Reference/Assembly-CSharp-decompiled</c> if present.
    /// </summary>
    internal static class EasyMateGripHandVisibility
    {
        private static readonly string[] PreferredHandIds = { "Male2", "Male 2" };

        /// <summary>Articulated (finger) mode vs sphere / kinematic compact mode per side.</summary>
        private static bool _leftArticulated;

        private static bool _rightArticulated;

        /// <summary>Scene / plugin start: both hands sphere mode, no collisions.</summary>
        public static void DisableVrHandModelsForSceneStart()
        {
            _leftArticulated = false;
            _rightArticulated = false;
            ApplyBothControls(SuperController.singleton);
        }

        public static void LateTick(bool featureEnabled)
        {
            if (!featureEnabled)
                return;

            SuperController sc = SuperController.singleton;
            if (sc == null || sc.isLoading)
                return;
            if (!sc.isOVR && !sc.isOpenVR && !XRSettings.enabled)
                return;

            bool leftDown = EasyMateVrInput.PollLeftGripClickDown(sc);
            bool rightDown = EasyMateVrInput.PollRightGripClickDown(sc);

            if (leftDown)
                ToggleLeft(sc);
            if (rightDown)
                ToggleRight(sc);
        }

        private static void ToggleLeft(SuperController sc)
        {
            bool nextArticulated = !_leftArticulated;
            if (nextArticulated && AnyPersonLeftHandPossessed())
                return;
            _leftArticulated = nextArticulated;
            ApplyBothControls(sc);
        }

        private static void ToggleRight(SuperController sc)
        {
            bool nextArticulated = !_rightArticulated;
            if (nextArticulated && AnyPersonRightHandPossessed())
                return;
            _rightArticulated = nextArticulated;
            ApplyBothControls(sc);
        }

        private static void ApplyBothControls(SuperController sc)
        {
            if (sc == null)
                return;
            ApplySingleControl(sc.commonHandModelControl);
            ApplySingleControl(sc.alternateControllerHandModelControl);
        }

        private static void ApplySingleControl(HandModelControl h)
        {
            if (h == null)
                return;

            bool motionOk = HandMotionReflection.TryApplyMotionModes(h, _leftArticulated, _rightArticulated);
            if (motionOk)
            {
                // Sphere mode still needs hand visuals enabled on the control.
                h.leftHandEnabled = true;
                h.rightHandEnabled = true;
                // Collisions only when at least one articulated hand is active (global flag on HandModelControl).
                h.useCollision = _leftArticulated || _rightArticulated;
            }
            else
            {
                // No motion enum API (unlikely): fall back to legacy hide/show.
                h.leftHandEnabled = _leftArticulated;
                h.rightHandEnabled = _rightArticulated;
                if (_leftArticulated || _rightArticulated)
                {
                    if (!h.useCollision)
                        h.useCollision = true;
                }
                else
                    h.useCollision = false;
            }

            if (_leftArticulated)
            {
                EnsurePreferredHandModels(h, left: true, right: false);
            }

            if (_rightArticulated)
            {
                EnsurePreferredHandModels(h, left: false, right: true);
            }
        }

        private static bool AnyPersonLeftHandPossessed()
        {
            try
            {
                SuperController sc = SuperController.singleton;
                if (sc == null)
                    return false;
                foreach (Atom a in sc.GetAtoms())
                {
                    if (a == null || a.type != "Person" || !a.gameObject.activeInHierarchy)
                        continue;
                    FreeControllerV3 l = a.GetStorableByID("lHandControl") as FreeControllerV3;
                    if (l != null && l.possessed)
                        return true;
                }
            }
            catch
            {
            }

            return false;
        }

        private static bool AnyPersonRightHandPossessed()
        {
            try
            {
                SuperController sc = SuperController.singleton;
                if (sc == null)
                    return false;
                foreach (Atom a in sc.GetAtoms())
                {
                    if (a == null || a.type != "Person" || !a.gameObject.activeInHierarchy)
                        continue;
                    FreeControllerV3 r = a.GetStorableByID("rHandControl") as FreeControllerV3;
                    if (r != null && r.possessed)
                        return true;
                }
            }
            catch
            {
            }

            return false;
        }

        private static bool HandsArrayHasNamedModel(HandModelControl.Hand[] hands, string modelName)
        {
            if (hands == null || string.IsNullOrEmpty(modelName))
                return false;
            for (int i = 0; i < hands.Length; i++)
            {
                HandModelControl.Hand hand = hands[i];
                if (hand != null && hand.name == modelName)
                    return true;
            }

            return false;
        }

        private static string ResolvePreferredModelIdForSide(HandModelControl.Hand[] hands)
        {
            if (hands == null)
                return null;
            for (int p = 0; p < PreferredHandIds.Length; p++)
            {
                string id = PreferredHandIds[p];
                if (HandsArrayHasNamedModel(hands, id))
                    return id;
            }

            return null;
        }

        private static void EnsurePreferredHandModels(HandModelControl h, bool left, bool right)
        {
            if (h == null)
                return;

            if (left)
            {
                string id = ResolvePreferredModelIdForSide(h.leftHands);
                if (!string.IsNullOrEmpty(id) && !string.Equals(h.leftHandChoice, id, StringComparison.Ordinal))
                    h.leftHandChoice = id;
            }

            if (right)
            {
                string id = ResolvePreferredModelIdForSide(h.rightHands);
                if (!string.IsNullOrEmpty(id) && !string.Equals(h.rightHandChoice, id, StringComparison.Ordinal))
                    h.rightHandChoice = id;
            }
        }

        /// <summary>
        /// Resolves left/right enum motion properties on <see cref="HandModelControl"/> at runtime (VaM builds differ).
        /// Sphere option defaults to enum index 1 when no name contains “Sphere”, matching the usual menu order.
        /// </summary>
        private static class HandMotionReflection
        {
            private static bool _attempted;

            private static bool _ok;

            private static PropertyInfo _leftPi;

            private static PropertyInfo _rightPi;

            private static object _sphereVal;

            private static object _fullVal;

            public static bool TryApplyMotionModes(HandModelControl handControl, bool leftArticulated, bool rightArticulated)
            {
                if (!_attempted)
                    TryResolve(typeof(HandModelControl));

                if (!_ok || _leftPi == null || _rightPi == null || _sphereVal == null || _fullVal == null)
                    return false;

                try
                {
                    _leftPi.SetValue(handControl, leftArticulated ? _fullVal : _sphereVal, null);
                    _rightPi.SetValue(handControl, rightArticulated ? _fullVal : _sphereVal, null);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            private static void TryResolve(Type handControlType)
            {
                _attempted = true;
                _ok = false;

                try
                {
                    PropertyInfo leftCand = null;
                    PropertyInfo rightCand = null;

                    foreach (PropertyInfo p in handControlType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (!p.CanWrite || p.PropertyType == null || !p.PropertyType.IsEnum)
                            continue;

                        string n = p.Name;
                        bool startsLeft = n.StartsWith("left", StringComparison.OrdinalIgnoreCase);
                        bool startsRight = n.StartsWith("right", StringComparison.OrdinalIgnoreCase);
                        if (!startsLeft && !startsRight)
                            continue;

                        bool nameLooksMotion =
                            n.IndexOf("motion", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            n.IndexOf("physics", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            n.IndexOf("interaction", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            n.IndexOf("kinematic", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            n.IndexOf("mode", StringComparison.OrdinalIgnoreCase) >= 0;

                        if (!nameLooksMotion)
                            continue;

                        if (startsLeft && leftCand == null)
                            leftCand = p;
                        if (startsRight && rightCand == null)
                            rightCand = p;
                    }

                    if (leftCand == null || rightCand == null || leftCand.PropertyType != rightCand.PropertyType)
                        return;

                    if (!TryPickSphereFullEnums(leftCand.PropertyType))
                        return;

                    _leftPi = leftCand;
                    _rightPi = rightCand;
                    _ok = true;
                }
                catch
                {
                    _ok = false;
                }
            }

            private static bool TryPickSphereFullEnums(Type enumType)
            {
                string[] names = Enum.GetNames(enumType);
                if (names == null || names.Length == 0)
                    return false;

                int fullIdx = -1;
                int sphereIdx = -1;

                for (int i = 0; i < names.Length; i++)
                {
                    string nm = names[i];
                    if (nm.IndexOf("sphere", StringComparison.OrdinalIgnoreCase) >= 0)
                        sphereIdx = i;
                    if (nm.IndexOf("full", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        nm.IndexOf("finger", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        nm.IndexOf("articulated", StringComparison.OrdinalIgnoreCase) >= 0)
                        fullIdx = i;
                }

                if (fullIdx < 0)
                    fullIdx = 0;

                // VaM UI: sphere / kinematic hand is typically the second entry after full articulated.
                if (sphereIdx < 0)
                    sphereIdx = names.Length > 1 ? 1 : 0;

                if (sphereIdx == fullIdx)
                {
                    sphereIdx = fullIdx == 0 && names.Length > 1 ? 1 : 0;
                    if (sphereIdx == fullIdx && names.Length > 2)
                        sphereIdx = 2;
                }

                try
                {
                    _fullVal = Enum.ToObject(enumType, fullIdx);
                    _sphereVal = Enum.ToObject(enumType, sphereIdx);
                    return !_sphereVal.Equals(_fullVal);
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
