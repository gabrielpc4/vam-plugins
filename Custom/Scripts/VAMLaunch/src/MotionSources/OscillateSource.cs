using System.Collections.Generic;
using UnityEngine;

namespace VAMLaunchPlugin.MotionSources
{
    public class OscillateSource : IMotionSource
    {
        private const float LAUNCH_DIR_CHANGE_DELAY = 0.02f;

        private JSONStorableBool _predictLagFactor;
        private JSONStorableFloat _minPosition;
        private JSONStorableFloat _maxPosition;
        private JSONStorableFloat _speed;
        private JSONStorableFloat _animationOffset;
        private JSONStorableStringChooser _targetAnimationAtomChooser;

        private UIDynamicPopup _chooseAnimationAtomPopup;

        private bool _moveUpwards = true;
        private float _dirChangeLastTimtestamp;
        private float _dirChangeTimer;
        private float _dirChangeDuration;

        private FreeControllerV3 _pluginFreeController;
        private FreeControllerV3 _animationAtomController;
        
        private AnimationPattern _targetAnimationPattern;
        
        private LineDrawer _lineDrawer0;

        private string _desiredTargetPrefix = "";

        public void OnInit(VAMLaunch plugin, string desiredTargetPrefix)
        {
            _desiredTargetPrefix = desiredTargetPrefix;

            _pluginFreeController = plugin.containingAtom.GetStorableByID("control") as FreeControllerV3;
            
            InitOptionsUI(plugin);
            InitEditorGizmos();

            _moveUpwards = true;
            _dirChangeTimer = 0.0f;
            _dirChangeLastTimtestamp = 0.0f;
        }

        public void SetInvert(bool newInvert)
        {
        }

        public void OnInitStorables(VAMLaunch plugin)
        {
            _predictLagFactor = new JSONStorableBool("oscPredictLagFactor", true);
            plugin.RegisterBool(_predictLagFactor);
            _minPosition = new JSONStorableFloat("oscSourceMinPosition", 10.0f, 0.0f, 99.0f);
            plugin.RegisterFloat(_minPosition);
            _maxPosition = new JSONStorableFloat("oscSourceMaxPosition", 80.0f, 0.0f, 99.0f);
            plugin.RegisterFloat(_maxPosition);
            _speed = new JSONStorableFloat("oscSourceSpeed", 30.0f, 20.0f, 80.0f);
            plugin.RegisterFloat(_speed);
            _animationOffset = new JSONStorableFloat("oscAnimationOffset", 0.0f, 0.0f, 0.5f);
            plugin.RegisterFloat(_animationOffset);

            _targetAnimationAtomChooser = new JSONStorableStringChooser("oscSourceTargetAnimationAtom",
                GetTargetAnimationAtomChoices(), "", "Target Animation Pattern",
                (name) =>
                {
                    _animationAtomController = null;
                    _targetAnimationPattern = null;

                    if (string.IsNullOrEmpty(name) || name.Equals("None"))
                    {
                        return;
                    }

                    var atom = SuperController.singleton.GetAtomByUid(name);
                    if (atom && atom != null && atom.animationPatterns != null && atom.animationPatterns.Length > 0)
                    {
                        if (atom.freeControllers != null && atom.freeControllers.Length > 0) { 
                            //only draws a line, not actually required to work
                            _animationAtomController = atom.freeControllers[0];
                        }
                        _targetAnimationPattern = atom.animationPatterns[0];
                    }
                });
            plugin.RegisterStringChooser(_targetAnimationAtomChooser);
        }
        
        private List<string> GetTargetAnimationAtomChoices()
        {
            List<string> result = new List<string>();
            result.Add("None");
            foreach (var uid in SuperController.singleton.GetAtomUIDs())
            {
                var atom = SuperController.singleton.GetAtomByUid(uid);
                if (atom != null && atom.animationPatterns != null && atom.animationPatterns.Length > 0)
                {
                    result.Add(uid);
                }
            }

            return result;
        }

        private void InitOptionsUI(VAMLaunch plugin)
        {
            var toggle = plugin.CreateToggle(_predictLagFactor);
            toggle.label = "Predict lag factor and adjust launch speed";
            toggle.height = 80;

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
            
            _chooseAnimationAtomPopup = plugin.CreateScrollablePopup(_targetAnimationAtomChooser);
            _chooseAnimationAtomPopup.popup.onOpenPopupHandlers += () =>
            {
                _targetAnimationAtomChooser.choices = GetTargetAnimationAtomChoices();
            };
            
            slider = plugin.CreateSlider(_animationOffset, false);
            slider.label = "Animation Offset";
            slider.slider.onValueChanged.AddListener((v) =>
            {
                _animationOffset.SetVal(v);
            });
        }

        public bool PredictLagFactor
        {
            get
            {
                return (_predictLagFactor.val);
            }
        }

        private void DestroyOptionsUI(VAMLaunch plugin)
        {
            plugin.RemoveToggle(_predictLagFactor);
            plugin.RemoveSlider(_minPosition);
            plugin.RemoveSlider(_maxPosition);
            plugin.RemoveSlider(_speed);
            plugin.RemoveSlider(_animationOffset);
            plugin.RemovePopup(_chooseAnimationAtomPopup);
        }
        
        private void InitEditorGizmos()
        {
            _lineDrawer0 = new LineDrawer(_pluginFreeController.linkLineMaterial);
        }

        

        private VAMLaunchPlugin.FpsLagFactor lagFactor = new VAMLaunchPlugin.FpsLagFactor();

        public bool OnUpdate(ref byte outPos, ref byte outSpeed)
        {
            float fpsLagFactor = lagFactor.Update() * Time.timeScale;
            if (!PredictLagFactor)
                fpsLagFactor = 1f;

            _dirChangeTimer -= Time.deltaTime;
            if (_dirChangeLastTimtestamp + _dirChangeDuration + LAUNCH_DIR_CHANGE_DELAY <= Time.realtimeSinceStartup || _dirChangeTimer <= 0.0f)
            {
                _moveUpwards = !_moveUpwards;

                float speed = _speed.val * fpsLagFactor;
                if (speed < 10f)
                    speed = 10f;
                if (speed > LaunchUtils.LAUNCH_MAX_SPEED)
                    speed = LaunchUtils.LAUNCH_MAX_SPEED;
                float dist = _maxPosition.val - _minPosition.val;

                _dirChangeDuration = LaunchUtils.PredictMoveDuration(dist, speed) + LAUNCH_DIR_CHANGE_DELAY;
                _dirChangeTimer = _dirChangeDuration; // - Mathf.Min(_dirChangeDuration, -_dirChangeTimer);
                _dirChangeLastTimtestamp = Time.realtimeSinceStartup;

                outPos = _moveUpwards ? (byte)_maxPosition.val :  (byte)_minPosition.val;
                outSpeed = (byte)((int)speed);

                return true;
            }

            return false;
        }
        
        public void OnSimulatorUpdate(float prevPos, float newPos, float deltaTime)
        {
            if (_targetAnimationPattern == null)
            {
                if (!string.IsNullOrEmpty(_targetAnimationAtomChooser.val))
                {
                    _targetAnimationAtomChooser.SetVal("");
                }
                
                return;
            }

            if (_pluginFreeController.selected && SuperController.singleton.editModeToggle.isOn)
            {
                _lineDrawer0.SetLinePoints(_pluginFreeController.transform.position,
                    _animationAtomController.transform.position);
                _lineDrawer0.Draw();
            }
            
            var currentTime = _targetAnimationPattern.GetFloatJSONParam("currentTime");
            
            float totalTime = _targetAnimationPattern.GetTotalTime();
            
            float normOldTime = currentTime.val / totalTime;

            float normalizedLaunchPos = Mathf.InverseLerp(_minPosition.val, _maxPosition.val, newPos);

            float newTime;
            if (_moveUpwards)
            {
                newTime = Mathf.Lerp(0.0f, totalTime * 0.5f, normalizedLaunchPos);
            }
            else
            {
                newTime = Mathf.Lerp(totalTime, totalTime * 0.5f, normalizedLaunchPos);
            }

            newTime = (newTime + (totalTime * _animationOffset.val)) % totalTime;

            float normNewTime = newTime / totalTime;
            
            if (Mathf.Abs(normNewTime - normOldTime) > 0.05f)
            {
                currentTime.SetVal(newTime);
            }
            _targetAnimationPattern.SetFloatParamValue("speed", totalTime / (_dirChangeDuration * 2.0f));
            
        }

        public void OnDestroy(VAMLaunch plugin)
        {
            DestroyOptionsUI(plugin);
        }
    }
}