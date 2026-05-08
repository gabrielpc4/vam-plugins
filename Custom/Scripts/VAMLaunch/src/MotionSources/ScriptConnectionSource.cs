using System.Collections.Generic;
using UnityEngine;

namespace VAMLaunchPlugin.MotionSources
{
    public class ScriptConnectionSource : IMotionSource
    {
        private const float LAUNCH_DIR_CHANGE_DELAY = 0.02f;

        private JSONStorableFloat _minPosition;
        private JSONStorableFloat _maxPosition;
        private JSONStorableFloat _speed;
        private JSONStorableBool _invertPosition;
        protected JSONStorableString _receivedDataFromScriptConnection;

        private bool _moveUpwards = true;
        private float _dirChangeTimer;
        private float _dirChangeDuration;

        private FreeControllerV3 _pluginFreeController;
        private FreeControllerV3 _animationAtomController;

        private AnimationPattern _targetAnimationPattern;

        private LineDrawer _lineDrawer0;

        private string _scriptConnectionAtomName = "VAMLaunchScriptConnection";
        private Atom _scriptConnectionAtom = null;
        private Transform _scriptConnectionTransform = null;

        private float lastPosition = 0;

        private string _desiredTargetPrefix = "";

        public void OnInit(VAMLaunch plugin, string desiredTargetPrefix)
        {
            _desiredTargetPrefix = desiredTargetPrefix;

            _pluginFreeController = plugin.containingAtom.GetStorableByID("control") as FreeControllerV3;

            InitOptionsUI(plugin);
            InitEditorGizmos();

            _moveUpwards = true;
            _dirChangeTimer = 0.0f;
        }

        public void SetInvert(bool newInvert)
        {
        }

        public void OnInitStorables(VAMLaunch plugin)
        {
            _minPosition = new JSONStorableFloat("scriptConnectionMinPosition", 0.0f, 0.0f, 99.0f);
            plugin.RegisterFloat(_minPosition);
            _maxPosition = new JSONStorableFloat("scriptConnectionMaxPosition", 70.0f, 0.0f, 99.0f);
            plugin.RegisterFloat(_maxPosition);
            _speed = new JSONStorableFloat("scriptConnectionSourceSpeed", 1.0f, 0.0f, 4.0f);
            plugin.RegisterFloat(_speed);
            _invertPosition = new JSONStorableBool("scriptConnectionInvertPosition", true);
            plugin.RegisterBool(_invertPosition);
        }

        private void InitOptionsUI(VAMLaunch plugin)
        {
            var slider = plugin.CreateSlider(_minPosition, true);
            slider.label = "Min Position";
            slider.slider.onValueChanged.AddListener((v) =>
            {
                _minPosition.SetVal(Mathf.Min(_maxPosition.val - 20.0f, v));
            });

            slider = plugin.CreateSlider(_maxPosition, true);
            slider.label = "Max Position";
            slider.slider.onValueChanged.AddListener((v) =>
            {
                _maxPosition.SetVal(Mathf.Max(_minPosition.val + 20.0f, v));
            });

            slider = plugin.CreateSlider(_speed, true);
            slider.label = "Speed";
            slider.slider.onValueChanged.AddListener((v) =>
            {
                _speed.SetVal(v);
            });

            _receivedDataFromScriptConnection = new JSONStorableString("Received Data From Script Connection", "");
            plugin.CreateTextField(_receivedDataFromScriptConnection);
    }

        private void DestroyOptionsUI(VAMLaunch plugin)
        {
            plugin.RemoveSlider(_minPosition);
            plugin.RemoveSlider(_maxPosition);
            plugin.RemoveSlider(_speed);
            plugin.RemoveTextField(_receivedDataFromScriptConnection);
        }

        private void InitEditorGizmos()
        {
            _lineDrawer0 = new LineDrawer(_pluginFreeController.linkLineMaterial);
        }

        public bool OnUpdate(ref byte outPos, ref byte outSpeed)
        {
            _scriptConnectionAtom = SuperController.singleton.GetAtomByUid(_scriptConnectionAtomName);
            if (_scriptConnectionAtom == null)
            {
               // SuperController.LogError("To use the VAM Launch script connection you must have an empty atom with ID " + _scriptConnectionAtomName + " where the x position is set between 0 and 1.0 for launch position, and y position is the speed.");
            }
            else
            {
                _scriptConnectionTransform = _scriptConnectionAtom.transform;
            }

            if (_scriptConnectionTransform == null)
            {
                return false;
            }


            float newSpeed = Mathf.Lerp(0.0f, 100.0f, _speed.val * Mathf.Clamp01(_scriptConnectionTransform.position.y));
            _receivedDataFromScriptConnection.val = "Direction: " + _scriptConnectionTransform.position.x + ", Speed: " + _scriptConnectionTransform.position.y + ", Adjusted Speed: " + newSpeed;

            bool newMoveUpwards = false;
            if (_scriptConnectionTransform.position.x > 0)
            {
                newMoveUpwards = true;
            }
            if (_invertPosition.val) newMoveUpwards = !newMoveUpwards;

            if (_moveUpwards != newMoveUpwards)
            {
                _moveUpwards = newMoveUpwards;

                float dist = _maxPosition.val - _minPosition.val;
                _dirChangeDuration = LaunchUtils.PredictMoveDuration(dist, newSpeed) + LAUNCH_DIR_CHANGE_DELAY;
                _dirChangeTimer = _dirChangeDuration - Mathf.Min(_dirChangeDuration, -_dirChangeTimer);

                outPos = _moveUpwards ? (byte)_maxPosition.val : (byte)_minPosition.val;
                outSpeed = (byte)Mathf.Clamp(newSpeed, 0, 98);

                return true;
            }

            return false;
        }

        public void OnSimulatorUpdate(float prevPos, float newPos, float deltaTime)
        {
            if (_pluginFreeController.selected && SuperController.singleton.editModeToggle.isOn && _scriptConnectionTransform != null)
            {
                _lineDrawer0.SetLinePoints(_pluginFreeController.transform.position,
                    _scriptConnectionTransform.position);
                _lineDrawer0.Draw();
            }
        }

        public void OnDestroy(VAMLaunch plugin)
        {
            DestroyOptionsUI(plugin);
        }
    }
}