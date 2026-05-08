using LFE.KeyboardShortcuts.Extensions;
using System;
using UnityEngine;
using UnityEngine.Animations;

namespace LFE.KeyboardShortcuts.Commands
{
    public class CameraPositionChange : Command
    {
        private Vector3 _direction;
        private float _unitsPerSecond;
        private Camera _windowCamera;
        public CameraPositionChange(Axis axis, float unitPerSecond)
        {
            RunPhase = CommandConst.RUNPHASE_FIXED_UPDATE;
            RepeatSpeed = 0;
            RepeatDelay = 0;
            _direction = Vector3.right;
            if(axis == Axis.X) { _direction = Vector3.right; }
            else if (axis == Axis.Y) { _direction = Vector3.up; }
            else if (axis == Axis.Z) { _direction = Vector3.forward; }
            _unitsPerSecond = unitPerSecond;
            _windowCamera = GetWindowCamera();
        }

        public override bool Execute(CommandExecuteEventArgs args)
        {
            if (args.KeyBinding.KeyChord.HasAxis)
            {
                // make sure the keybinding and value change are going the
                // same "direction" (for the case where the axis is involved)
                if (!MathUtilities.SameSign(args.Data, _unitsPerSecond)) { return false; }

                if(_direction == Vector3.forward) {
                    // invert the direction for forward/back if using an axis
                    _direction = Vector3.back;
                }
            }

            if (_windowCamera != null)
            {
                var multiplier = 1.0f;
                if (InputWrapper.GetKey(KeyCode.LeftShift) || InputWrapper.GetKey(KeyCode.RightShift) || args.KeyBinding.KeyChord.HasAxis)
                {
                    multiplier *= 3.0f;
                }
                _windowCamera.transform.Translate(_direction * Time.deltaTime * _unitsPerSecond * multiplier * Mathf.Abs(args.Data));
            }
            return true;
        }

        private Camera GetWindowCamera() {
            return CameraTarget.centerTarget.targetCamera;
        }
    }
}
