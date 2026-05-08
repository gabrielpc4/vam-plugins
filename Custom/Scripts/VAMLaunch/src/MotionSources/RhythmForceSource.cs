using System.Collections.Generic;
using UnityEngine;

namespace VAMLaunchPlugin.MotionSources
{
    public class RhythmForceSource : IMotionSource
    {
        private const float LAUNCH_DIR_CHANGE_DELAY = 0.02f;

        public bool logMessages = false;
        
        private JSONStorableFloat _minPosition;
        private JSONStorableFloat _maxPosition;
        private JSONStorableFloat _speed;
        private JSONStorableFloat _delay;
        private JSONStorableStringChooser _targetRhythmForceAtomChooser;
        private JSONStorableBool _useBeatMagnitude;
        private JSONStorableBool _invertPosition;

        private UIDynamicPopup _chooseRhythmForceAtomPopup;

        private bool _moveUpwards = true;
        private float _dirChangeTimer;
        private float _dirChangeDuration;

        private FreeControllerV3 _rhythmForceAtomController;
        
        private RhythmForceProducerV2 _targetRhythmForcePattern;
        
        private LineDrawer _lineDrawer0;

        private float _lastForcePercent = 0;

        private string _desiredTargetPrefix = "";

        public class RhythmForceHistoryItem
        {

            public float Time { get; set; }
            public bool Direction { get; set; }
            public float Speed { get; set; }

            public RhythmForceHistoryItem(float Time, bool Direction, float Speed)
            {
                this.Time = Time;
                this.Direction = Direction;
                this.Speed = Speed;
            }
        }

        private List<RhythmForceHistoryItem> _rhythmForceHistory;

        public void Log(string message)
        {
            if (logMessages) SuperController.LogMessage(message);
        }

        public void OnInit(VAMLaunch plugin, string desiredTargetPrefix)
        {
            _desiredTargetPrefix = desiredTargetPrefix;

            InitOptionsUI(plugin);

            _moveUpwards = true;
            _dirChangeTimer = 0.0f;

            _rhythmForceHistory = new List<RhythmForceHistoryItem>();
        }

        public void SetInvert(bool newInvert)
        {
            _invertPosition.SetVal(newInvert);
        }

        public void OnInitStorables(VAMLaunch plugin)
        {
            _minPosition = new JSONStorableFloat("rhySourceMinPosition", 0.0f, 0.0f, 99.0f);
            plugin.RegisterFloat(_minPosition);
            _maxPosition = new JSONStorableFloat("rhySourceMaxPosition", 70.0f, 0.0f, 99.0f);
            plugin.RegisterFloat(_maxPosition);
            _useBeatMagnitude = new JSONStorableBool("rhyUseBeatMagnitude", true);
            plugin.RegisterBool(_useBeatMagnitude);
            _invertPosition = new JSONStorableBool("rhyInvertPosition", true);
            plugin.RegisterBool(_invertPosition);
            _speed = new JSONStorableFloat("rhySourceSpeed", 30.0f, 1.0f, 100.0f);
            plugin.RegisterFloat(_speed);
            _delay = new JSONStorableFloat("rhyDelay", 0.0f, 0.0f, 4.0f);
            plugin.RegisterFloat(_delay);

            _targetRhythmForceAtomChooser = new JSONStorableStringChooser("rhySourceTargetRhythmForceAtom",
                GetTargetRhythmForceAtomChoices(), "", "Target Force Producer",
                (name) =>
                {
                    Log("change rhythm force producer to: " + name);
                    _rhythmForceAtomController = null;
                    _targetRhythmForcePattern = null;

                    if (string.IsNullOrEmpty(name))
                    {
                        return;
                    }

                    var atom = SuperController.singleton.GetAtomByUid(name);
                    if (atom && atom.forceProducers.Length > 0)
                    {
                        Log("found force producer in: " + name);
                        _rhythmForceAtomController = atom.freeControllers[0];
                        _targetRhythmForcePattern = atom.GetComponentInChildren<RhythmForceProducerV2>();
                    }
                });
            plugin.RegisterStringChooser(_targetRhythmForceAtomChooser);
        }
        
        private List<string> GetTargetRhythmForceAtomChoices()
        {
            List<string> result = new List<string>();
            foreach (var uid in SuperController.singleton.GetAtomUIDs())
            {
                var atom = SuperController.singleton.GetAtomByUid(uid);
                if (atom != null && atom.forceProducers != null && atom.forceProducers.Length > 0)
                {
                    if (atom.GetComponentInChildren<RhythmForceProducerV2>() != null)
                        result.Add(uid);
                
                }
            }
            result.Add("None");

            return result;
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

            var toggle = plugin.CreateToggle(_useBeatMagnitude);
            toggle.label = "Faster with Strong Down Beats";

            toggle = plugin.CreateToggle(_invertPosition);
            toggle.label = "Invert";

            slider = plugin.CreateSlider(_delay, true);
            slider.label = "Delay (seconds)";
            slider.slider.onValueChanged.AddListener((v) =>
            {
                _delay.SetVal(v);
            });

            _chooseRhythmForceAtomPopup = plugin.CreateScrollablePopup(_targetRhythmForceAtomChooser);
            _chooseRhythmForceAtomPopup.popup.onOpenPopupHandlers += () =>
            {
                _targetRhythmForceAtomChooser.choices = GetTargetRhythmForceAtomChoices();
            };

            _targetRhythmForceAtomChooser.choices = GetTargetRhythmForceAtomChoices();
            if (string.IsNullOrEmpty(_targetRhythmForceAtomChooser.val))
            {
                Log("null or empty setting to : " + _targetRhythmForceAtomChooser.choices[0]);
                _targetRhythmForceAtomChooser.SetVal(_targetRhythmForceAtomChooser.choices[0]);
            }
            else
            {
                Log("json starting setting to : " + _targetRhythmForceAtomChooser.val);
                _targetRhythmForceAtomChooser.SetVal(_targetRhythmForceAtomChooser.val);
            }
        }

        private void DestroyOptionsUI(VAMLaunch plugin)
        {
            plugin.RemoveSlider(_minPosition);
            plugin.RemoveSlider(_maxPosition);
            plugin.RemoveSlider(_speed);
            plugin.RemoveSlider(_delay);
            plugin.RemoveToggle(_useBeatMagnitude);
            plugin.RemoveToggle(_invertPosition);
            plugin.RemovePopup(_chooseRhythmForceAtomPopup);
        }
        
        public bool OnUpdate(ref byte outPos, ref byte outSpeed)
        {

            /* if (_lastForcePercent != _targetRhythmForcePattern.targetForcePercent)
             {
                 _lastForcePercent = _targetRhythmForcePattern.targetForcePercent;
                 SuperController.LogMessage(string.Format("forcePercent:{0}, appliedForce:{1}, forceFactor:{2}",
                     _targetRhythmForcePattern.targetForcePercent, _targetRhythmForcePattern.appliedForce, _targetRhythmForcePattern.forceFactor));
             }*/

            _targetRhythmForcePattern.randomFactor = 0;



            if (_targetRhythmForcePattern.targetForcePercent != 0) { 
                bool willMoveUpwards = (_targetRhythmForcePattern.targetForcePercent > 0);
                if (_invertPosition.val) willMoveUpwards = !willMoveUpwards;
                if (_moveUpwards != willMoveUpwards)
                {

                    _moveUpwards = willMoveUpwards;

                    float forceSpeed = Mathf.Lerp(2.0f, 100.0f, _targetRhythmForcePattern.forceQuickness / 50.0f);
                                  
                    float adjustedSpeed = _speed.val / 10.0f * forceSpeed;

                    if (_useBeatMagnitude.val)
                        adjustedSpeed *= Mathf.Abs(_targetRhythmForcePattern.targetForcePercent) * 2.5f;

                    _rhythmForceHistory.Add(new RhythmForceHistoryItem(Time.realtimeSinceStartup, _moveUpwards, adjustedSpeed));
                    
                    /*SuperController.LogMessage(string.Format("speed:{0}, adj:{1}, chanceOfBurst:{2}",
                        forceSpeed, adjustedSpeed, _targetRhythmForcePattern.randomFactor));*/

                }
            }

            if (_rhythmForceHistory.Count > 0)
            {
                if (Time.realtimeSinceStartup >= _rhythmForceHistory[0].Time + _delay.val)
                {
                    outPos = _rhythmForceHistory[0].Direction ? (byte)_maxPosition.val : (byte)_minPosition.val;
                    outSpeed = (byte)Mathf.Clamp(_rhythmForceHistory[0].Speed, 0, 98);
                    
                    _rhythmForceHistory.RemoveAt(0);
                    return true;
                }
            }

            return false;
        }
        
        public void OnSimulatorUpdate(float prevPos, float newPos, float deltaTime)
        {
            if (_targetRhythmForcePattern == null)
            {
                if (!string.IsNullOrEmpty(_targetRhythmForceAtomChooser.val))
                {
                    _targetRhythmForceAtomChooser.SetVal("");
                }
                
                return;
            }
        }

        public void OnDestroy(VAMLaunch plugin)
        {
            _rhythmForceHistory.Clear();
            DestroyOptionsUI(plugin);
        }
    }
}