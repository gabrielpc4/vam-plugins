using System;
using System.Collections.Generic;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Before passenger VR hand possession, turns <b>Possessable</b> off
    /// everywhere except <c>lHandControl</c> and <c>rHandControl</c> on the
    /// target <c>Person</c>; then restores prior
    /// <see cref="FreeControllerV3.possessable"/> / <c>canGrab*</c> values.
    /// </summary>
    internal static class PassengerPossessableNarrow
    {
        private sealed class FreeControllerPossessableBackup
        {
            internal FreeControllerV3 Controller;

            internal bool Possessable;

            internal bool CanGrabPosition;

            internal bool CanGrabRotation;
        }

        private static List<FreeControllerPossessableBackup> _backups;

        public static void Restore()
        {
            if (_backups == null || _backups.Count == 0)
            {
                _backups = null;
                return;
            }

            for (int backupIndex = 0; backupIndex < _backups.Count; backupIndex++)
            {
                FreeControllerPossessableBackup entry = _backups[backupIndex];
                FreeControllerV3 controller = entry.Controller;
                if (controller == null)
                {
                    continue;
                }

                try
                {
                    controller.possessable = entry.Possessable;
                    controller.canGrabPosition = entry.CanGrabPosition;
                    controller.canGrabRotation = entry.CanGrabRotation;
                }
                catch (Exception exception)
                {
                    SuperController.LogError(
                        "Easy Mate passenger possessable restore: " +
                        exception.Message);
                }
            }

            _backups = null;
        }

        /// <summary>
        /// Backs up every Person <see cref="FreeControllerV3"/> in the scene,
        /// then disables possession grab except the hands on
        /// <paramref name="targetPerson"/>.
        /// </summary>
        public static void ApplyForTargetPerson(Atom targetPerson)
        {
            Restore();

            SuperController sc = SuperController.singleton;
            if (sc == null || targetPerson == null || targetPerson.type != "Person")
            {
                return;
            }

            FreeControllerV3 leftHandControl =
                targetPerson.GetStorableByID("lHandControl") as FreeControllerV3;
            FreeControllerV3 rightHandControl =
                targetPerson.GetStorableByID("rHandControl") as FreeControllerV3;

            List<Atom> sceneAtoms = sc.GetAtoms();
            if (sceneAtoms == null)
            {
                return;
            }

            _backups = new List<FreeControllerPossessableBackup>();

            for (int atomIndex = 0; atomIndex < sceneAtoms.Count; atomIndex++)
            {
                Atom atom = sceneAtoms[atomIndex];
                if (atom == null || atom.type != "Person")
                {
                    continue;
                }

                FreeControllerV3[] freeControllers =
                    atom.GetComponentsInChildren<FreeControllerV3>(true);
                if (freeControllers == null)
                {
                    continue;
                }

                for (int fcIndex = 0; fcIndex < freeControllers.Length; fcIndex++)
                {
                    FreeControllerV3 controller = freeControllers[fcIndex];
                    if (controller == null)
                    {
                        continue;
                    }

                    FreeControllerPossessableBackup backup =
                        new FreeControllerPossessableBackup();
                    backup.Controller = controller;
                    backup.Possessable = controller.possessable;
                    backup.CanGrabPosition = controller.canGrabPosition;
                    backup.CanGrabRotation = controller.canGrabRotation;
                    _backups.Add(backup);

                    bool allowForPassengerHands =
                        atom == targetPerson &&
                        (controller == leftHandControl ||
                            controller == rightHandControl);

                    SetControllerPossessableGate(controller, allowForPassengerHands);
                }
            }
        }

        private static void SetControllerPossessableGate(
            FreeControllerV3 controller,
            bool allowPossessGrab)
        {
            if (controller == null)
            {
                return;
            }

            try
            {
                if (allowPossessGrab)
                {
                    controller.possessable = true;
                    controller.canGrabPosition = true;
                    controller.canGrabRotation = true;
                }
                else
                {
                    controller.possessable = false;
                    controller.canGrabPosition = false;
                    controller.canGrabRotation = false;
                }
            }
            catch (Exception exception)
            {
                SuperController.LogError(
                    "Easy Mate passenger possessable gate: " +
                    exception.Message);
            }
        }
    }
}
