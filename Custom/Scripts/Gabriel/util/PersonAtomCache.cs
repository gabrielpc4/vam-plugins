using System;
using System.Collections.Generic;
using System.Linq;
using MeshVR;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Cached female/male <c>Person</c> lists ordered by UID; invalidated on atom
    /// set changes.
    /// </summary>
    public static class PersonAtomCache
    {
        private static bool _personGenderListsCacheValid;

        private static List<Atom> _cachedFemalePersonsByUid;

        private static List<Atom> _cachedMalePersonsByUid;

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
