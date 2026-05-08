using System;
using System.Collections.Generic;
using System.Linq;
using MeshVR;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Cached female/male <c>Person</c> lists ordered by UID; invalidated on atom
    /// set changes. Also exposes a per-frame active-person possession snapshot
    /// so hot paths can share one scene scan.
    /// </summary>
    public static class PersonAtomCache
    {
        public struct FramePersonPossessionSnapshot
        {
            public bool AnyHeadOrHandPossessed;

            public bool AnyLeftHandPossessed;

            public bool AnyRightHandPossessed;
        }

        private static bool _personGenderListsCacheValid;

        private static List<Atom> _cachedFemalePersonsByUid;

        private static List<Atom> _cachedMalePersonsByUid;

        private static readonly List<Atom> _frameActivePersons =
            new List<Atom>();

        private static int _framePersonPossessionSnapshotFrame = -1;

        private static FramePersonPossessionSnapshot
            _framePersonPossessionSnapshot;

        public static void InvalidatePersonGenderCaches()
        {
            _personGenderListsCacheValid = false;
            _cachedFemalePersonsByUid = null;
            _cachedMalePersonsByUid = null;
        }

        private static void EnsurePersonGenderCaches()
        {
            SuperController sc;

            if (_personGenderListsCacheValid)
                return;

            sc = SuperController.singleton;
            if (sc == null)
            {
                _cachedFemalePersonsByUid = new List<Atom>();
                _cachedMalePersonsByUid = new List<Atom>();
                _personGenderListsCacheValid = true;
                return;
            }

            _cachedFemalePersonsByUid = sc.GetAtoms()
                .Where(x => x != null && x.type == "Person" && IsPersonFemale(x))
                .OrderBy(x => x.uid, StringComparer.Ordinal)
                .ToList();
            _cachedMalePersonsByUid = sc.GetAtoms()
                .Where(x => x != null && x.type == "Person" && !IsPersonFemale(x))
                .OrderBy(x => x.uid, StringComparer.Ordinal)
                .ToList();
            _personGenderListsCacheValid = true;
        }

        private static void OnPersonSceneAtomUIDsChanged(List<string> atomUids)
        {
            InvalidatePersonGenderCaches();
        }

        public static void RegisterPersonGenderCacheInvalidation()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null)
                return;

            sc.onAtomUIDsChangedHandlers -= OnPersonSceneAtomUIDsChanged;
            sc.onAtomUIDsChangedHandlers += OnPersonSceneAtomUIDsChanged;
        }

        public static void UnregisterPersonGenderCacheInvalidation()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null)
                return;

            sc.onAtomUIDsChangedHandlers -= OnPersonSceneAtomUIDsChanged;
        }

        public static List<Atom> GetPersonAtoms()
        {
            List<Atom> persons = new List<Atom>();
            SuperController sc = SuperController.singleton;

            if (sc == null)
                return persons;

            foreach (Atom at in sc.GetAtoms())
            {
                if (at != null && at.type == "Person")
                    persons.Add(at);
            }

            return persons;
        }

        public static DAZCharacterSelector TryGetCharacterSelector(Atom atom)
        {
            JSONStorable geometry = atom.GetStorableByID("geometry");
            return geometry as DAZCharacterSelector;
        }

        public static void PrimeFramePersonPossessionSnapshot(
            SuperController sc)
        {
            EnsureFramePersonPossessionSnapshot(sc);
        }

        public static FramePersonPossessionSnapshot
            GetFramePersonPossessionSnapshot()
        {
            return EnsureFramePersonPossessionSnapshot(
                SuperController.singleton);
        }

        public static List<Atom> GetActivePersonsThisFrame()
        {
            EnsureFramePersonPossessionSnapshot(SuperController.singleton);
            return _frameActivePersons;
        }

        public static bool IsPersonFemale(Atom atom)
        {
            if (atom == null || atom.type != "Person")
                return false;

            DAZCharacter dazCharacter = atom.GetComponentInChildren<DAZCharacter>();
            return dazCharacter != null && !dazCharacter.isMale;
        }

        public static bool IsMalePerson(Atom atom)
        {
            return atom != null && atom.type == "Person" && !IsPersonFemale(atom);
        }

        private static FramePersonPossessionSnapshot
            EnsureFramePersonPossessionSnapshot(SuperController sc)
        {
            List<Atom> atoms;
            int frame;
            int atomIndex;

            frame = Time.frameCount;
            if (_framePersonPossessionSnapshotFrame == frame)
            {
                return _framePersonPossessionSnapshot;
            }

            _framePersonPossessionSnapshotFrame = frame;
            _framePersonPossessionSnapshot = new FramePersonPossessionSnapshot();
            _frameActivePersons.Clear();

            if (sc == null || sc.isLoading)
            {
                return _framePersonPossessionSnapshot;
            }

            atoms = sc.GetAtoms();
            if (atoms == null)
            {
                return _framePersonPossessionSnapshot;
            }

            for (atomIndex = 0; atomIndex < atoms.Count; atomIndex++)
            {
                Atom atom = atoms[atomIndex];
                FreeControllerV3 headControl;
                FreeControllerV3 leftHandControl;
                FreeControllerV3 rightHandControl;

                if (atom == null || atom.type != "Person" ||
                    !atom.gameObject.activeInHierarchy)
                {
                    continue;
                }

                _frameActivePersons.Add(atom);

                headControl =
                    atom.GetStorableByID("headControl") as FreeControllerV3;
                if (headControl != null && headControl.possessed)
                {
                    _framePersonPossessionSnapshot.AnyHeadOrHandPossessed =
                        true;
                }

                leftHandControl =
                    atom.GetStorableByID("lHandControl") as FreeControllerV3;
                if (leftHandControl != null && leftHandControl.possessed)
                {
                    _framePersonPossessionSnapshot.AnyHeadOrHandPossessed =
                        true;
                    _framePersonPossessionSnapshot.AnyLeftHandPossessed = true;
                }

                rightHandControl =
                    atom.GetStorableByID("rHandControl") as FreeControllerV3;
                if (rightHandControl != null && rightHandControl.possessed)
                {
                    _framePersonPossessionSnapshot.AnyHeadOrHandPossessed =
                        true;
                    _framePersonPossessionSnapshot.AnyRightHandPossessed =
                        true;
                }
            }

            return _framePersonPossessionSnapshot;
        }

        public static List<Atom> FemalePersonsByUid()
        {
            EnsurePersonGenderCaches();
            return new List<Atom>(_cachedFemalePersonsByUid);
        }

        public static List<Atom> MalePersonsByUid()
        {
            EnsurePersonGenderCaches();
            return new List<Atom>(_cachedMalePersonsByUid);
        }
    }
}
