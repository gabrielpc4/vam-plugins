using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// While any controller is possessed, if the look camera moves farther than a threshold from the possessed
    /// character’s feet (distance in the plane perpendicular to <see cref="SuperController.navigationRig"/> up),
    /// calls <see cref="MainUIButtons.RequestClearAllPossession"/>.
    /// </summary>
    internal static class EasyMatePossessFootDistanceAutoRelease
    {
        private static float _cooldownUntil;

        public static void LateTick(bool enabled, float maxHorizontalMetersFromFeetMid)
        {
            if (!enabled || maxHorizontalMetersFromFeetMid <= 0f)
                return;
            if (Time.time < _cooldownUntil)
                return;

            SuperController sc = SuperController.singleton;
            if (sc == null || sc.isLoading)
                return;

            Atom subject;
            if (!TryFindAtomWithAnyPossessedController(sc, out subject))
                return;

            Vector3 feetMid;
            if (!TryGetFeetMidpointWorld(subject, out feetMid))
                return;

            Camera cam = sc.lookCamera;
            if (cam == null)
                return;

            Vector3 up = sc.navigationRig != null ? sc.navigationRig.up : Vector3.up;
            if (up.sqrMagnitude < 1e-10f)
                up = Vector3.up;
            up.Normalize();

            Vector3 horiz = Vector3.ProjectOnPlane(cam.transform.position - feetMid, up);
            float maxSq = maxHorizontalMetersFromFeetMid * maxHorizontalMetersFromFeetMid;
            if (horiz.sqrMagnitude <= maxSq)
                return;

            MainUIButtons.RequestClearAllPossession(
                null,
                advanceVrPalmHudGenderCycle: true);
            _cooldownUntil = Time.time + 0.35f;
        }

        private static bool TryFindAtomWithAnyPossessedController(SuperController sc, out Atom atom)
        {
            atom = null;
            try
            {
                foreach (Atom a in sc.GetAtoms())
                {
                    if (a == null || !a.gameObject.activeInHierarchy)
                        continue;
                    FreeControllerV3[] fcs = a.transform.GetComponentsInChildren<FreeControllerV3>(true);
                    for (int i = 0; i < fcs.Length; i++)
                    {
                        if (fcs[i] != null && fcs[i].possessed)
                        {
                            atom = a;
                            return true;
                        }
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        private static bool TryGetFeetMidpointWorld(Atom atom, out Vector3 feetMid)
        {
            feetMid = Vector3.zero;
            if (atom == null)
                return false;

            if (atom.type == "Person")
            {
                FreeControllerV3 l = atom.GetStorableByID("lFootControl") as FreeControllerV3;
                FreeControllerV3 r = atom.GetStorableByID("rFootControl") as FreeControllerV3;
                Vector3 pL = FreeControllerWorldPosition(l);
                Vector3 pR = FreeControllerWorldPosition(r);
                bool haveL = l != null;
                bool haveR = r != null;
                if (haveL && haveR)
                {
                    feetMid = (pL + pR) * 0.5f;
                    return true;
                }

                if (haveL)
                {
                    feetMid = pL;
                    return true;
                }

                if (haveR)
                {
                    feetMid = pR;
                    return true;
                }

                FreeControllerV3 pelvis = atom.GetStorableByID("pelvisControl") as FreeControllerV3;
                if (pelvis != null)
                {
                    feetMid = FreeControllerWorldPosition(pelvis);
                    return true;
                }
            }

            feetMid = atom.transform.position;
            return true;
        }

        private static Vector3 FreeControllerWorldPosition(FreeControllerV3 fc)
        {
            if (fc == null)
                return Vector3.zero;
            if (fc.control != null)
                return fc.control.position;
            if (fc.followWhenOff != null)
                return fc.followWhenOff.position;
            return fc.transform.position;
        }
    }
}
