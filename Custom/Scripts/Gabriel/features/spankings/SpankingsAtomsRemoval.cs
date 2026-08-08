using System;
using MeshVR;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Scene atom UIDs created by <c>octopussy.Spankings</c>; removed when
    /// Spankings is stripped from Persons.
    /// </summary>
    public static class SpankingsAtomsRemoval
    {
        private static readonly string[] OwnedSceneAtomUids =
        {
            "HitAudioSource",
            "CheekLeft",
            "CheekRight"
        };

        /// <summary>
        /// Deletes Spankings-owned scene atoms if present (no-op when loading).
        /// </summary>
        public static void TryRemoveOwnedSceneAtoms()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null || sc.isLoading)
            {
                return;
            }

            int i;
            for (i = 0; i < OwnedSceneAtomUids.Length; i++)
            {
                string uid = OwnedSceneAtomUids[i];
                try
                {
                    Atom atom = sc.GetAtomByUid(uid);
                    if (atom != null)
                    {
                        sc.RemoveAtom(atom);
                    }
                }
                catch (Exception e)
                {
                    SuperController.LogError(
                        "Gabriel: remove Spankings scene atom \"" + uid +
                        "\": " + e.Message);
                }
            }
        }
    }
}
