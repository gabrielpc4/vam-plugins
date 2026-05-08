using System;
using System.Collections.Generic;
using System.Linq;
using MeshVR;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Cached female/male <c>Person</c> lists ordered by UID; invalidated on atom
    /// set changes. Also exposes per-frame active-person possession and
    /// free-controller caches so hot paths can share one scene scan.
    /// </summary>
    public static class PersonAtomCache
    {
        public struct FramePersonPossessionSnapshot
        {
            public bool AnyHeadOrHandPossessed;

            public bool AnyLeftHandPossessed;

            public bool AnyRightHandPossessed;
        }

        public struct FramePersonControllers
        {
            public FreeControllerV3 HeadControl;

            public FreeControllerV3 LeftHandControl;

            public FreeControllerV3 RightHandControl;

            public FreeControllerV3 ChestControl;

            public FreeControllerV3 PelvisControl;
        }

        private static bool _personGenderListsCacheValid;

        private static List<Atom> _cachedFemalePersonsByUid;

        private static List<Atom> _cachedMalePersonsByUid;

        private static readonly List<Atom> _frameActivePersons =
            new List<Atom>();

        private static int _framePersonPossessionSnapshotFrame = -1;

        private static FramePersonPossessionSnapshot
            _framePersonPossessionSnapshot;

        private static int _framePersonControllersFrame = -1;

        private static readonly Dictionary<string, FramePersonControllers>
            _framePersonControllersByKey =
                new Dictionary<string, FramePersonControllers>();

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

        /// <summary>
        /// True when geometry has any active garment (no gender filter; caller uses
        /// e.g. <see cref="FemalePersonsByUid"/> when merging female-only helpers).
        /// </summary>
        public static bool PersonHasAnyActiveClothingOnGeometry(Atom atom)
        {
            int i;
            DAZCharacterSelector selector;
            DAZClothingItem[] items;

            if (atom == null || atom.type != "Person")
                return false;

            selector = TryGetCharacterSelector(atom);
            if (selector == null)
                return false;

            items = selector.clothingItems;
            if (items == null)
                return false;

            for (i = 0; i < items.Length; i++)
            {
                DAZClothingItem item = items[i];
                if (item != null && item.active)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Female Person with at least one active garment on the geometry
        /// selector (used for touch fall-off eligibility).
        /// </summary>
        public static bool PersonFemaleHasAnyActiveClothing(Atom atom)
        {
            if (!IsPersonFemale(atom))
                return false;

            return PersonHasAnyActiveClothingOnGeometry(atom);
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

        public static bool TryGetCachedFreeController(
            Atom atom,
            string storableId,
            out FreeControllerV3 controller)
        {
            FramePersonControllers controllers;

            controller = null;
            if (atom == null ||
                atom.type != "Person" ||
                string.IsNullOrEmpty(storableId))
            {
                return false;
            }

            if (!TryGetFramePersonControllers(atom, out controllers))
                return false;

            if (string.Equals(storableId, "headControl", StringComparison.Ordinal))
                controller = controllers.HeadControl;
            else if (string.Equals(
                storableId,
                "lHandControl",
                StringComparison.Ordinal))
            {
                controller = controllers.LeftHandControl;
            }
            else if (string.Equals(
                storableId,
                "rHandControl",
                StringComparison.Ordinal))
            {
                controller = controllers.RightHandControl;
            }
            else if (string.Equals(
                storableId,
                "chestControl",
                StringComparison.Ordinal) ||
                string.Equals(storableId, "chest", StringComparison.Ordinal))
            {
                controller = controllers.ChestControl;
            }
            else if (string.Equals(
                storableId,
                "pelvisControl",
                StringComparison.Ordinal))
            {
                controller = controllers.PelvisControl;
            }
            else
            {
                controller = atom.GetStorableByID(storableId) as
                    FreeControllerV3;
            }

            return controller != null;
        }

        public static bool TryGetFreeControllerWorldPosition(
            FreeControllerV3 controller,
            out Vector3 world)
        {
            world = Vector3.zero;
            if (controller == null)
                return false;

            if (controller.followWhenOff != null)
            {
                world = controller.followWhenOff.position;
                return true;
            }

            if (controller.follow != null)
            {
                world = controller.follow.position;
                return true;
            }

            if (controller.transform != null)
            {
                world = controller.transform.position;
                return true;
            }

            return false;
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
            ResetFramePersonControllersCache(frame);

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

                headControl = GetOrCacheFramePersonController(
                    atom,
                    "headControl");
                if (headControl != null && headControl.possessed)
                {
                    _framePersonPossessionSnapshot.AnyHeadOrHandPossessed =
                        true;
                }

                leftHandControl = GetOrCacheFramePersonController(
                    atom,
                    "lHandControl");
                if (leftHandControl != null && leftHandControl.possessed)
                {
                    _framePersonPossessionSnapshot.AnyHeadOrHandPossessed =
                        true;
                    _framePersonPossessionSnapshot.AnyLeftHandPossessed = true;
                }

                rightHandControl = GetOrCacheFramePersonController(
                    atom,
                    "rHandControl");
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

        private static bool TryGetFramePersonControllers(
            Atom atom,
            out FramePersonControllers controllers)
        {
            string key;

            controllers = new FramePersonControllers();
            if (atom == null || atom.type != "Person")
                return false;

            ResetFramePersonControllersCache(Time.frameCount);
            key = GetFramePersonControllerKey(atom);
            if (string.IsNullOrEmpty(key))
            {
                controllers = BuildFramePersonControllers(atom);
                return true;
            }

            if (_framePersonControllersByKey.TryGetValue(key, out controllers))
                return true;

            controllers = BuildFramePersonControllers(atom);
            _framePersonControllersByKey[key] = controllers;
            return true;
        }

        private static void ResetFramePersonControllersCache(int frame)
        {
            if (_framePersonControllersFrame == frame)
                return;

            _framePersonControllersFrame = frame;
            _framePersonControllersByKey.Clear();
        }

        private static FramePersonControllers BuildFramePersonControllers(
            Atom atom)
        {
            FramePersonControllers controllers =
                new FramePersonControllers();

            if (atom == null || atom.type != "Person")
                return controllers;

            controllers.HeadControl =
                atom.GetStorableByID("headControl") as FreeControllerV3;
            controllers.LeftHandControl =
                atom.GetStorableByID("lHandControl") as FreeControllerV3;
            controllers.RightHandControl =
                atom.GetStorableByID("rHandControl") as FreeControllerV3;
            controllers.ChestControl =
                atom.GetStorableByID("chestControl") as FreeControllerV3;
            if (controllers.ChestControl == null)
                controllers.ChestControl =
                    atom.GetStorableByID("chest") as FreeControllerV3;
            controllers.PelvisControl =
                atom.GetStorableByID("pelvisControl") as FreeControllerV3;

            return controllers;
        }

        private static FreeControllerV3 GetOrCacheFramePersonController(
            Atom atom,
            string storableId)
        {
            FramePersonControllers controllers;

            if (!TryGetFramePersonControllers(atom, out controllers))
                return null;

            if (string.Equals(storableId, "headControl", StringComparison.Ordinal))
                return controllers.HeadControl;
            if (string.Equals(storableId, "lHandControl", StringComparison.Ordinal))
                return controllers.LeftHandControl;
            if (string.Equals(storableId, "rHandControl", StringComparison.Ordinal))
                return controllers.RightHandControl;
            if (string.Equals(
                storableId,
                "chestControl",
                StringComparison.Ordinal) ||
                string.Equals(storableId, "chest", StringComparison.Ordinal))
            {
                return controllers.ChestControl;
            }
            if (string.Equals(
                storableId,
                "pelvisControl",
                StringComparison.Ordinal))
            {
                return controllers.PelvisControl;
            }

            return atom.GetStorableByID(storableId) as FreeControllerV3;
        }

        private static string GetFramePersonControllerKey(Atom atom)
        {
            if (atom == null)
                return null;

            if (!string.IsNullOrEmpty(atom.uid))
                return atom.uid;

            return atom.GetInstanceID().ToString();
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
