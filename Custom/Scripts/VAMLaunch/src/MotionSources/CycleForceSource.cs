using System.Collections.Generic;
using UnityEngine;

namespace VAMLaunchPlugin.MotionSources
{
    public class CycleForceSource : IMotionSource
    {
        private const float LAUNCH_DIR_CHANGE_DELAY = 0.02f;

        private JSONStorableFloat _minPosition;
        private JSONStorableFloat _maxPosition;
        private JSONStorableFloat _speed;
        private JSONStorableFloat _delay;
        private JSONStorableStringChooser _targetCycleForceAtomChooser;
        private JSONStorableBool _invertPosition;
        private JSONStorableBool _randomizePeriod;
        private JSONStorableBool _randomizeCycleRatio;
        private JSONStorableBool _randomizeQuickness;
        private JSONStorableBool _autoSelect;

        
        private UIDynamicPopup _chooseCycleForceAtomPopup;

        private bool _moveUpwards = true;
        private float _dirChangeTimer;
        private float _dirChangeDuration;

        private FreeControllerV3 _pluginFreeController;
        private FreeControllerV3 _cycleForceAtomController;

        private CycleForceProducerV2 _targetCycleForcePattern;

        private LineDrawer _lineDrawer0;

        private bool isLoading = false;
        private bool autoFindNow = false;
        private int findCycleForceFrameCount = 0;
        private string cycleForceHip = "";
        private string cycleForceHead = "";
        private string cycleForceDesiredPrefix = "CycleForce_EM";

        private string _desiredTargetPrefix = "";

        public class CycleFloatParamRandomizer
        {
            public float Period { get; set; }
            public float Quickness { get; set; }
            public float LowerValue { get; set; }
            public float UpperValue { get; set; }
            public float Timer { get; set; }
            public float CurrentValue { get; set; }
            public float TargetValue { get; set; }

            public CycleFloatParamRandomizer(float Period, float Quickness, float LowerValue, float UpperValue)
            {
                this.Period = Period;
                this.Quickness = Quickness;
                this.LowerValue = LowerValue;
                this.UpperValue = UpperValue;

                this.Timer = 0;
                this.CurrentValue = 0;
                this.TargetValue = 0;
            }

            public float Update(float deltaTime)
            {
                Timer -= deltaTime;
                if (Timer < 0.0f)
                {
                    // reset timer and set a new random target value
                    Timer = Period;
                    TargetValue = UnityEngine.Random.Range(LowerValue, UpperValue);
                }
                CurrentValue = Mathf.Lerp(CurrentValue, TargetValue, Time.deltaTime * Quickness);

                return CurrentValue;
            }
        }

        private CycleFloatParamRandomizer randomizer1;
        private CycleFloatParamRandomizer randomizer2;
        private CycleFloatParamRandomizer randomizer3;
        private CycleFloatParamRandomizer randomizer4;

        public class CycleForceHistoryItem
        {

            public float Time { get; set; }
            public bool Direction { get; set; }
            public float Speed { get; set; }

            public CycleForceHistoryItem(float Time, bool Direction, float Speed)
            {
                this.Time = Time;
                this.Direction = Direction;
                this.Speed = Speed;
            }
        }

        private List<CycleForceHistoryItem> _cycleForceHistory;

        public void OnInit(VAMLaunch plugin, string desiredTargetPrefix)
        {
            _desiredTargetPrefix = desiredTargetPrefix;

            _pluginFreeController = plugin.containingAtom.GetStorableByID("control") as FreeControllerV3;

            InitOptionsUI(plugin);
            InitEditorGizmos();

            _moveUpwards = true;
            _dirChangeTimer = 0.0f;

            _cycleForceHistory = new List<CycleForceHistoryItem>();

            //IF YOU WANT TO CHANGE THE VALUES OF THE RANDOMIZER, DO IT HERE, MAINLY THE LAST TWO VALUES IN EACH RANDOMIZER, THE MIN AND MAX
            randomizer1 = new CycleFloatParamRandomizer(0.5f, 10.0f, 0.3f, 1.2f); //CYCLE PERIOD
            randomizer2 = new CycleFloatParamRandomizer(0.5f, 10.0f, 0.5f, 6.0f); //RANDOMIZES THE PERIOD OF RANDOMIZER1 AND RANDOMIZER4, ADDING RANDOM CHANGES TO THE RANDOMIZERS
            randomizer3 = new CycleFloatParamRandomizer(0.5f, 1.0f, 0.25f, 0.75f); //CYCLE RATIO
            randomizer4 = new CycleFloatParamRandomizer(0.5f, 10.0f, 1.5f, 7.0f); //QUICKNESS

            isLoading = true;
        }

        public void SetInvert(bool newInvert)
        {
            //SuperController.LogMessage("set invert: " + newInvert);
            _invertPosition.SetVal(newInvert);
        }

        public void OnInitStorables(VAMLaunch plugin)
        {
            _minPosition = new JSONStorableFloat("cycSourceMinPosition", 0.0f, 0.0f, 99.0f);
            plugin.RegisterFloat(_minPosition);
            _maxPosition = new JSONStorableFloat("cycSourceMaxPosition", 70.0f, 0.0f, 99.0f);
            plugin.RegisterFloat(_maxPosition);
            _invertPosition = new JSONStorableBool("cycInvertPosition", true);
            plugin.RegisterBool(_invertPosition);
            _randomizePeriod = new JSONStorableBool("cycRandomizePeriod", false);
            plugin.RegisterBool(_randomizePeriod);
            _randomizeCycleRatio = new JSONStorableBool("cycRandomizeCycleRatio", false);
            plugin.RegisterBool(_randomizeCycleRatio);
            _randomizeQuickness = new JSONStorableBool("cycRandomizeQuickness", false);
            plugin.RegisterBool(_randomizeQuickness);
            _autoSelect = new JSONStorableBool("cycAutoSelect", true);
            plugin.RegisterBool(_autoSelect);
            _speed = new JSONStorableFloat("cycSourceSpeed", 30.0f, 1.0f, 100.0f);
            plugin.RegisterFloat(_speed);
            _delay = new JSONStorableFloat("cycDelay", 0.1f, 0.0f, 4.0f);
            plugin.RegisterFloat(_delay);

            _targetCycleForceAtomChooser = new JSONStorableStringChooser("cycSourceTargetCycleForceAtom",
                GetTargetCycleForceAtomChoices(), "", "Target Force Producer",
                (name) =>
                {
                    _cycleForceAtomController = null;
                    _targetCycleForcePattern = null;

                    if (string.IsNullOrEmpty(name))
                    {
                        return;
                    }

                    var atom = SuperController.singleton.GetAtomByUid(name);
                    if (atom && atom.forceProducers.Length > 0)
                    {
                        _cycleForceAtomController = atom.freeControllers[0];
                        _targetCycleForcePattern = atom.GetComponentInChildren<CycleForceProducerV2>();
                    }
                });


            plugin.RegisterStringChooser(_targetCycleForceAtomChooser);
        }

        public void RefreshForceProducers()
        {
            _targetCycleForceAtomChooser.choices = GetTargetCycleForceAtomChoices();
            _targetCycleForceAtomChooser.SetVal(_targetCycleForceAtomChooser.choices[0]);
        }

        private List<string> GetTargetCycleForceAtomChoices()
        {
            cycleForceHip = "";
            cycleForceHead = "";
            cycleForceDesiredPrefix = "";

            List<string> result = new List<string>();
            foreach (var uid in SuperController.singleton.GetAtomUIDs())
            {
                var atom = SuperController.singleton.GetAtomByUid(uid);
                if (atom != null && atom.forceProducers != null && atom.forceProducers.Length > 0)
                {
                    CycleForceProducerV2 cycleForceProducerV2 = atom.GetComponentInChildren<CycleForceProducerV2>();
                    if (cycleForceProducerV2 != null)
                    {
                        result.Add(uid);
                        if (atom.uid.StartsWith(_desiredTargetPrefix))
                        {
                            cycleForceDesiredPrefix = uid;
                            //SuperController.LogMessage("found desired target: " + uid);
                        }
                        else if (cycleForceProducerV2.receiver != null && cycleForceProducerV2.receiver.name == "hip")
                        {
                            cycleForceHip = uid;
                            //SuperController.LogMessage("found hip: " + uid);
                        }
                        else if (cycleForceProducerV2.receiver != null && cycleForceProducerV2.receiver.name == "head")
                        {
                            cycleForceHead = uid;
                            //SuperController.LogMessage("found head: " + uid);
                        }
                    }
                }
            }

            //IF WE HAVE HAVE MORE THAN ONE, REORDER THE LIST
            if (result.Count > 1)
            {
                List<string> orderedResults = new List<string>();
                if (cycleForceDesiredPrefix != "")
                {
                    autoFindNow = false; //found the specified cycleforce
                    orderedResults.Add(cycleForceDesiredPrefix);
                }
                
                if (cycleForceHip != "" && !orderedResults.Contains(cycleForceHip))
                {
                    if (cycleForceDesiredPrefix == "") autoFindNow = false; //found a good cycleforce and not required to find a certain cycleforce
                    orderedResults.Add(cycleForceHip);
                }

                if (cycleForceHead != "" && !orderedResults.Contains(cycleForceHead))
                {
                    if (cycleForceDesiredPrefix == "") autoFindNow = false; //found a good cycleforce and not required to find a certain cycleforce
                    orderedResults.Add(cycleForceHead);
                }

                foreach (string aCycleForce in result)
                {
                    if (!orderedResults.Contains(aCycleForce))
                    {
                        orderedResults.Add(aCycleForce);
                    }
                }

                orderedResults.Add("None");

                return orderedResults;
            } else
            {
                result.Add("None");

                return result;
            }
        }

        private void InitOptionsUI(VAMLaunch plugin)
        {

            if (string.IsNullOrEmpty(_targetCycleForceAtomChooser.val))
            {
                //nothing set, so autoselect
                _targetCycleForceAtomChooser.SetVal(_targetCycleForceAtomChooser.choices[0]);
                _autoSelect.SetVal(true);
            }

            _chooseCycleForceAtomPopup = plugin.CreateScrollablePopup(_targetCycleForceAtomChooser);
            _chooseCycleForceAtomPopup.popup.onOpenPopupHandlers += () =>
            {
                _targetCycleForceAtomChooser.choices = GetTargetCycleForceAtomChoices();
            };

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

            var toggle = plugin.CreateToggle(_autoSelect);
            toggle.label = "Autoselect 1st CF on Scene Load";
            toggle.toggle.onValueChanged.AddListener((v) =>
            {
                autoFindNow = true;
                findCycleForceFrameCount = 85;
            });

            toggle = plugin.CreateToggle(_invertPosition);
            toggle.label = "Invert";

            toggle = plugin.CreateToggle(_randomizePeriod);
            toggle.label = "Randomize CycleForce Period";

            toggle = plugin.CreateToggle(_randomizeCycleRatio);
            toggle.label = "Randomize CycleForce Ratio";

            toggle = plugin.CreateToggle(_randomizeQuickness);
            toggle.label = "Randomize CycleForce Quickness";

            slider = plugin.CreateSlider(_delay, true);
            slider.label = "Delay (seconds)";
            slider.slider.onValueChanged.AddListener((v) =>
            {
                _delay.SetVal(v);
            });
        }

        private void DestroyOptionsUI(VAMLaunch plugin)
        {
            plugin.RemoveSlider(_minPosition);
            plugin.RemoveSlider(_maxPosition);
            plugin.RemoveSlider(_speed);
            plugin.RemoveSlider(_delay);
            plugin.RemoveToggle(_invertPosition);
            plugin.RemoveToggle(_randomizePeriod);
            plugin.RemoveToggle(_randomizeCycleRatio);
            plugin.RemoveToggle(_randomizeQuickness);
            plugin.RemoveToggle(_autoSelect);

            plugin.RemovePopup(_chooseCycleForceAtomPopup);
        }

        private void InitEditorGizmos()
        {
            _lineDrawer0 = new LineDrawer(_pluginFreeController.linkLineMaterial);
        }

        public bool OnUpdate(ref byte outPos, ref byte outSpeed)
        {
            if (_autoSelect.val)
            {
                if (SuperController.singleton.isLoading)
                {
                    isLoading = true;
                    return false;
                }

                if (isLoading && !SuperController.singleton.isLoading)
                {
                    isLoading = false;
                    autoFindNow = true;
                    _targetCycleForceAtomChooser.choices = GetTargetCycleForceAtomChoices();
                    _targetCycleForceAtomChooser.SetVal(_targetCycleForceAtomChooser.choices[0]);

                    if (_targetCycleForceAtomChooser.choices[0].StartsWith("CycleForce_EM"))
                    {
                        _invertPosition.SetVal(false);
                    }
                    else
                    {
                        _invertPosition.SetVal(true);
                    }
                }

                if (!isLoading && autoFindNow)
                {
                    //find one!
                    findCycleForceFrameCount++;
                    if (findCycleForceFrameCount >= 30)
                    {
                        findCycleForceFrameCount = 0;

                        _targetCycleForceAtomChooser.choices = GetTargetCycleForceAtomChoices();
                        _targetCycleForceAtomChooser.SetVal(_targetCycleForceAtomChooser.choices[0]);

                        if (_targetCycleForceAtomChooser.choices[0].StartsWith("CycleForce_EM"))
                        {
                            _invertPosition.SetVal(false);
                        } else
                        {
                            _invertPosition.SetVal(true);
                        }

                        autoFindNow = false;
                    }
                }
            }

            if (!_targetCycleForcePattern.on) return false;

            //RANDOMIZING CYCLE FORCE
            if (_randomizePeriod.val)
            {
                randomizer1.Period = randomizer2.Update(Time.deltaTime);
                _targetCycleForcePattern.period = randomizer1.Update(Time.deltaTime);
            }
            if (_randomizePeriod.val)
                _targetCycleForcePattern.periodRatio = randomizer3.Update(Time.deltaTime);

            if (_randomizeQuickness.val)
            {
                if (_randomizePeriod.val)
                {
                    randomizer4.Period = randomizer1.Period;
                }
                else
                {
                    randomizer4.Period = randomizer2.Update(Time.deltaTime);
                }
                _targetCycleForcePattern.forceQuickness = randomizer4.Update(Time.deltaTime);
            }

            //LAUNCH CONTROL
            bool willMoveUpwards = (_targetCycleForcePattern.targetForcePercent > 0);
            if (_invertPosition.val) willMoveUpwards = !willMoveUpwards;
            if (_moveUpwards != willMoveUpwards)
            {

                /*SuperController.LogMessage(string.Format("forcePercent:{0}, appliedForce:{1}, forceFactor:{2}, periodRatio:{3}",
                    _targetCycleForcePattern.targetForcePercent, _targetCycleForcePattern.appliedForce, _targetCycleForcePattern.forceFactor, _targetCycleForcePattern.periodRatio));*/

                _moveUpwards = willMoveUpwards;

                float forceSpeed = Mathf.Lerp(0.01f, 100.0f, (_targetCycleForcePattern.forceQuickness / (_targetCycleForcePattern.period + 0.05f) / 100.0f));

                float forceFactor = Mathf.Lerp(0.01f, 4.0f, _targetCycleForcePattern.forceFactor / 1000.0f);

                float adjustedSpeed = _speed.val * forceSpeed * forceFactor * 0.2f;

                _cycleForceHistory.Add(new CycleForceHistoryItem(Time.realtimeSinceStartup, _moveUpwards, adjustedSpeed));

                /*SuperController.LogMessage(string.Format("add item time:{0}, direction:{1}, speed:{2}, time:{3}, delay:{4}, count:{5}",
                   Time.realtimeSinceStartup, _moveUpwards, adjustedSpeed, Time.realtimeSinceStartup, _delay.val, _cycleForceHistory.Count));*/

                /*SuperController.LogMessage(string.Format("speed:{0}, factor:{1}, adj:{2}, final:{3}",
                    forceSpeed, forceFactor, adjustedSpeed, outSpeed));*/

            }

            if (_cycleForceHistory.Count > 0)
            {
                if (Time.realtimeSinceStartup >= _cycleForceHistory[0].Time + _delay.val)
                {
                    outPos = _cycleForceHistory[0].Direction ? (byte)_maxPosition.val : (byte)_minPosition.val;
                    outSpeed = (byte)Mathf.Clamp(_cycleForceHistory[0].Speed, 0, 98);

                    _cycleForceHistory.RemoveAt(0);

                    /*SuperController.LogMessage(string.Format("use item time:{0}, direction:{1}, speed:{2}, time:{3}",
                       _cycleForceHistory[0].Time, _cycleForceHistory[0].Direction, _cycleForceHistory[0].Speed, Time.realtimeSinceStartup));*/
                    return true;
                }
            }

            return false;
        }

        public void OnSimulatorUpdate(float prevPos, float newPos, float deltaTime)
        {
            if (_targetCycleForcePattern == null)
            {
                if (!string.IsNullOrEmpty(_targetCycleForceAtomChooser.val))
                {
                    _targetCycleForceAtomChooser.SetVal("");
                }

                return;
            }

            if (_pluginFreeController.selected && SuperController.singleton.editModeToggle.isOn)
            {
                _lineDrawer0.SetLinePoints(_pluginFreeController.transform.position,
                    _cycleForceAtomController.transform.position);
                _lineDrawer0.Draw();
            }
        }

        public void OnDestroy(VAMLaunch plugin)
        {
            _cycleForceHistory.Clear();
            DestroyOptionsUI(plugin);
        }
    }
}