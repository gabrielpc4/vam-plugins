using System.Collections.Generic;
using MeshVR;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// One-shot disable of <see cref="DAZTongueControl.tongueCollision"/> on each
    /// Person so tongue RBs do not shove the jaw or penetrate the mouth mesh on
    /// fast head motion (VaM defaults leave tongue collision on).
    /// </summary>
    public static class PersonTongueCollisionDisable
    {
        private static readonly HashSet<string> ProcessedPersonUids =
            new HashSet<string>();

        public static void ResetForNewScene()
        {
            ProcessedPersonUids.Clear();
        }

        public static void ApplyToAllPersonAtoms(SuperController sc)
        {
            if (sc == null)
            {
                return;
            }

            List<Atom> atoms = sc.GetAtoms();
            int i;
            for (i = 0; i < atoms.Count; i++)
            {
                Atom a = atoms[i];
                if (a == null || a.type != "Person")
                {
                    continue;
                }

                TryDisableForPerson(a);
            }
        }

        /// <summary>
        /// Runs at most once per <paramref name="person"/>.uid until
        /// <see cref="ResetForNewScene"/>.
        /// </summary>
        public static void TryDisableForPerson(Atom person)
        {
            if (person == null || person.type != "Person")
            {
                return;
            }

            if (string.IsNullOrEmpty(person.uid))
            {
                return;
            }

            if (ProcessedPersonUids.Contains(person.uid))
            {
                return;
            }

            ProcessedPersonUids.Add(person.uid);

            DAZTongueControl[] tongues = person.GetComponentsInChildren<DAZTongueControl>(
                true);
            int t;
            for (t = 0; t < tongues.Length; t++)
            {
                DAZTongueControl tc = tongues[t];
                if (tc != null)
                {
                    tc.tongueCollision = false;
                }
            }
        }
    }
}
