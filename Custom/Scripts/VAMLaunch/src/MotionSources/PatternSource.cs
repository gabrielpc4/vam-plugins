using System.Collections.Generic;
using UnityEngine;

namespace VAMLaunchPlugin.MotionSources
{
    public class PatternSource : IMotionSource
    {
        private struct MotionPoint
        {
            public float Time;
            public Vector3 Position;
        }

        private bool logMessages = false;

        private const int MOTION_POINTS_INITIAL_CAPACITY = 20;

		private JSONStorableBool _predictLagFactor;
        public JSONStorableFloat _minPosition;
        public JSONStorableFloat _maxPosition;
        private JSONStorableFloat _patternSpeedMultiplier;
        public JSONStorableStringChooser _targetAnimationAtomChooser;
        private JSONStorableStringChooser _samplePlaneChooser;
        private JSONStorableBool _includeMidPoints;
        private JSONStorableBool _invertPosition;
        private JSONStorableBool _useLocalSpace;

        private JSONStorableFloat _patternTime;
        private JSONStorableFloat _patternSpeed;

        private UIDynamicPopup _chooseAnimationAtomPopup;
        private UIDynamicPopup _chooseSamplePlanePopup;

        private FreeControllerV3 _pluginFreeController;
        private FreeControllerV3 _animationAtomController;

        private AnimationPattern _targetAnimationPattern;

        private int findAnimationPatternCount = 0;

        private List<MotionPoint> _motionPoints = new List<MotionPoint>(MOTION_POINTS_INITIAL_CAPACITY);
        private float _lastLaunchPos;
        private int _lastPointIndex;

        private Atom animationPatternAtom;

        private LineDrawer _lineDrawer0;

        private string lastAnimationName = "";

        private string lastReceiverName = "";

        private string _desiredTargetPrefix = "Thrust AP";
        private string animationPatternDesiredPrefix = "";

        bool isLoading = true;
        int finishedLoadingFrameCount = 0;


        private List<string> _samplePlaneChoices = new List<string>
        {
            "X",
            "Y",
            "Z"
        };

        private int _samplePlaneIndex;

        public void Log(string stringToLog)
        {
            if (logMessages) SuperController.LogMessage(stringToLog);
        }

        public void OnInit(VAMLaunch plugin, string desiredTargetPrefix)
        {
            _desiredTargetPrefix = desiredTargetPrefix;

            _pluginFreeController = plugin.containingAtom.GetStorableByID("control") as FreeControllerV3;

            InitOptionsUI(plugin);
            InitEditorGizmos();
        }

        public void SetInvert(bool newInvert)
        {
            Log("Set Invert");
            _invertPosition.SetVal(newInvert);
        }

        public void OnInitStorables(VAMLaunch plugin)
        {
            _predictLagFactor = new JSONStorableBool("patternPredictLagFactor", true);
            plugin.RegisterBool(_predictLagFactor);
            
            _minPosition = new JSONStorableFloat("patternSourceMinPosition", 0.0f, 0.0f, 99.0f);
            plugin.RegisterFloat(_minPosition);
            _maxPosition = new JSONStorableFloat("patternSourceMaxPosition", 70.0f, 0.0f, 99.0f);
            plugin.RegisterFloat(_maxPosition);

            _patternSpeedMultiplier = new JSONStorableFloat("patternSpeedMultiplier", 1.0f, 0.0f, 5.0f);
            plugin.RegisterFloat(_patternSpeedMultiplier);

            _targetAnimationAtomChooser = new JSONStorableStringChooser("patternSourceTargetAnimationAtom",
                GetTargetAnimationAtomChoices(), "", "Target Animation Pattern",
                (name) =>
                {
                    Log("SET VAL: " + _targetAnimationAtomChooser.val);
                    ChangeAnimationPattern(name);
                });
            plugin.RegisterStringChooser(_targetAnimationAtomChooser);

            _samplePlaneChooser = new JSONStorableStringChooser("patternSourceSamplePlane", _samplePlaneChoices, "",
                "Sample Plane", (name) =>
                {
                    for (int i = 0; i < _samplePlaneChoices.Count; i++)
                    {
                        if (_samplePlaneChoices[i] == name)
                        {
                            _samplePlaneIndex = i;
                            break;
                        }
                    }
                });
            plugin.RegisterStringChooser(_samplePlaneChooser);
            
            _includeMidPoints = new JSONStorableBool("patternSourceIncludeMidPoints", false);
            plugin.RegisterBool(_includeMidPoints);
            _invertPosition = new JSONStorableBool("patternSourceInvertPosition", false);
            plugin.RegisterBool(_invertPosition);
            _useLocalSpace = new JSONStorableBool("patternSourceUseLocalSpace", false);
            plugin.RegisterBool(_useLocalSpace);
        }

        private List<string> GetTargetAnimationAtomChoices()
        {
            animationPatternDesiredPrefix = "";

            bool wasHidden = SuperController.singleton.showHiddenAtoms;
            SuperController.singleton.showHiddenAtoms = true;
            List<string> result = new List<string>();
            foreach (var uid in SuperController.singleton.GetAtomUIDs())
            {
                Atom atom = SuperController.singleton.GetAtomByUid(uid);
                if (atom != null && atom.animationPatterns != null && atom.animationPatterns.Length > 0)
                {
                    Log("found possible target: " + uid);
                    if (atom.uid.StartsWith(_desiredTargetPrefix))
                    {
                        animationPatternDesiredPrefix = uid;
                        lastAnimationName = animationPatternDesiredPrefix; //if we want to set the last used, instead set to this desired new one
                        Log("found desired target: " + uid);
                    }
                    result.Add(uid);
                }
            }

            //IF WE HAVE HAVE MORE THAN ONE, REORDER THE LIST
            if (result.Count > 1)
            {
                List<string> orderedResults = new List<string>();
                if (animationPatternDesiredPrefix != "")
                {
                    Log("ordered target: " + animationPatternDesiredPrefix);
                    orderedResults.Add(animationPatternDesiredPrefix);
                }

                foreach (string aCycleForce in result)
                {
                    if (!orderedResults.Contains(aCycleForce))
                    {
                        Log("other target: " + animationPatternDesiredPrefix);
                        orderedResults.Add(aCycleForce);
                    }
                }

                SuperController.singleton.showHiddenAtoms = wasHidden;

                return orderedResults;
            }
            else
            {
                result.Add("None");

                SuperController.singleton.showHiddenAtoms = wasHidden;

                return result;
            }
        }

        private void ChangeAnimationPattern(string name)
        {
            _animationAtomController = null;
            _targetAnimationPattern = null;

            if (string.IsNullOrEmpty(name) || name.Equals("None"))
            {
                return;
            }

            animationPatternAtom = SuperController.singleton.GetAtomByUid(name);
            if (animationPatternAtom)
            {
                if (animationPatternAtom.animationPatterns.Length > 0)
                {
                    _animationAtomController = animationPatternAtom.freeControllers[0];
                    _targetAnimationPattern = animationPatternAtom.animationPatterns[0];
                    _patternTime = _targetAnimationPattern.GetFloatJSONParam("currentTime");
                    _patternSpeed = _targetAnimationPattern.GetFloatJSONParam("speed");
                    lastAnimationName = name;
                } else
                {
                    Log("VAM Launch " + name + " does not contain an animation pattern.");
                }
            } else
            {
                Log("VAM Launch selected pattern " + name + " not found.");
            }
        }

        private void InitOptionsUI(VAMLaunch plugin)
        {
            Log("InitOptionsUI");
            
            var toggle = plugin.CreateToggle(_predictLagFactor);
            toggle.label = "Predict lag factor and adjust launch speed";
            toggle.height = 80;

            if (string.IsNullOrEmpty(_targetAnimationAtomChooser.val))
            {
                Log("empty, set val: " + _targetAnimationAtomChooser.choices[0]);
                _targetAnimationAtomChooser.SetVal(_targetAnimationAtomChooser.choices[0]);
            }
            else
            {
                Log("remembering animation pattern set in JSON save file");
                _desiredTargetPrefix = _targetAnimationAtomChooser.val;
            }

            if (string.IsNullOrEmpty(_samplePlaneChooser.val))
            {
                _samplePlaneChooser.SetVal("Y");
            }

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

            slider = plugin.CreateSlider(_patternSpeedMultiplier, true);
            slider.label = "Pattern Speed Multiplier";

            _chooseAnimationAtomPopup = plugin.CreateScrollablePopup(_targetAnimationAtomChooser);
            _chooseAnimationAtomPopup.popup.onOpenPopupHandlers += () =>
            {
                _targetAnimationAtomChooser.choices = GetTargetAnimationAtomChoices();
            };

            _chooseSamplePlanePopup = plugin.CreateScrollablePopup(_samplePlaneChooser);
            _chooseSamplePlanePopup.popup.onOpenPopupHandlers += () =>
            {
                _samplePlaneChooser.choices = _samplePlaneChoices;
            };

            toggle = plugin.CreateToggle(_useLocalSpace);
            toggle.label = "Use Local Space";

            toggle = plugin.CreateToggle(_includeMidPoints);
            toggle.label = "Include Mid Points";

            toggle = plugin.CreateToggle(_invertPosition);
            toggle.label = "Invert";
        }

        private void DestroyOptionsUI(VAMLaunch plugin)
        {
			plugin.RemoveToggle(_predictLagFactor);
            plugin.RemoveSlider(_minPosition);
            plugin.RemoveSlider(_maxPosition);
            plugin.RemoveSlider(_patternSpeedMultiplier);
            plugin.RemovePopup(_chooseAnimationAtomPopup);
            plugin.RemovePopup(_samplePlaneChooser);
            plugin.RemoveToggle(_includeMidPoints);
            plugin.RemoveToggle(_invertPosition);
            plugin.RemoveToggle(_useLocalSpace);
        }

        public bool PredictLagFactor
        {
            get
            {
                return (_predictLagFactor.val);
            }
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

            if (SuperController.singleton.isLoading)
            {
                finishedLoadingFrameCount = 0;
                isLoading = true;
                return false;
            }

            if (isLoading && !SuperController.singleton.isLoading)
            {
                finishedLoadingFrameCount++;
            }

            if (finishedLoadingFrameCount >= 30)
            {
                finishedLoadingFrameCount = 0;
                isLoading = false;

                Log("Finished Loading");
                _targetAnimationAtomChooser.choices = GetTargetAnimationAtomChoices();
                
                if (_targetAnimationAtomChooser.choices.Count > 0 && _targetAnimationAtomChooser.choices[0] != "None")
                {
                    //do we have an animation pattern with the same name? 
                    int newIndex = _targetAnimationAtomChooser.choices.IndexOf(lastAnimationName);
                    if (newIndex < 0) newIndex = 0; //not found, just choose the first pattern found
                    Log("new index set " + newIndex + " with desired name: " + lastAnimationName);

                    _targetAnimationAtomChooser.SetVal(_targetAnimationAtomChooser.choices[newIndex]);
                    ChangeAnimationPattern(_targetAnimationAtomChooser.choices[newIndex]);
                    //Log("VAM Launch found a new animation pattern: " + _targetAnimationAtomChooser.choices[newIndex]);
                }

                return false;
            }

            _patternTime = _targetAnimationPattern.GetFloatJSONParam("currentTime");
            _patternSpeed = _targetAnimationPattern.GetFloatJSONParam("speed");


            Atom apAtom = _targetAnimationPattern.containingAtom;
            MoveProducer mp = apAtom.GetStorableByID("AnimatedObject") as MoveProducer;
            if (lastReceiverName == "")
            {
                lastReceiverName = mp.receiver.containingAtom.name;
            } else if (lastReceiverName != mp.receiver.containingAtom.name)
            {
                if (lastReceiverName == "Person#2" || mp.receiver.containingAtom.name == "Person#2")
                {
                    // a bit hackey but probably we are using dollmaster and have switched the thruster, so invert the vam launch response
                    _invertPosition.val = !_invertPosition.val;
                }
                lastReceiverName = mp.receiver.containingAtom.name;
            }

            if (_pluginFreeController.selected && SuperController.singleton.editModeToggle.isOn)
            {
                _lineDrawer0.SetLinePoints(_pluginFreeController.transform.position,
                    _animationAtomController.transform.position);
                _lineDrawer0.Draw();
            }
            
            if (_targetAnimationPattern.steps.Length <= 1)
            {
                return false;
            }

            float minPos, maxPos;
            GenerateMotionPointsFromPattern(_targetAnimationPattern, ref _motionPoints, out minPos, out maxPos);

            if (_includeMidPoints.val && _pluginFreeController.selected &&
                SuperController.singleton.editModeToggle.isOn)
            {
                for (int i = 0; i < _motionPoints.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        continue;
                    }

                    var boxMatrix = Matrix4x4.TRS(_motionPoints[i].Position, Quaternion.identity,
                        new Vector3(0.1f, 0.1f, 0.1f));

                    Graphics.DrawMesh(_animationAtomController.holdPositionMesh, boxMatrix,
                        _animationAtomController.linkLineMaterial,
                        _animationAtomController.gameObject.layer, null, 0, null, false, false);
                }
            }

            int p0, p1;
            if (GetMotionPointIndices(_patternTime.val, _motionPoints, out p0, out p1))
            {
                if (p0 != _lastPointIndex)
                {
                    float yFactor = Mathf.InverseLerp(minPos, maxPos, GetPositionForPlane(_motionPoints[p1].Position));
                    
                    float timeToNextPoint;
                    if (p1 > p0)
                    {
                        timeToNextPoint = _motionPoints[p1].Time - _patternTime.val;
                    }
                    else
                    {
                        timeToNextPoint = _targetAnimationPattern.GetTotalTime() - _patternTime.val +
                                            _motionPoints[p1].Time;
                    }

                    timeToNextPoint /= Mathf.Max(_patternSpeed.val * _patternSpeedMultiplier.val, 0.001f);

                    float launchFactor = _invertPosition.val ? 1.0f - yFactor : yFactor;
                    
                    float launchPos = Mathf.Lerp(_minPosition.val, _maxPosition.val, launchFactor);
                    // float launchSpeed = LaunchUtils.PredictMoveSpeed(_lastLaunchPos, launchPos, timeToNextPoint);
                    float launchSpeed = LaunchUtils.PredictMoveSpeed(_lastLaunchPos, launchPos, timeToNextPoint) * fpsLagFactor;

                    outPos = (byte) launchPos;
                    outSpeed = (byte) launchSpeed;
                    
                    _lastPointIndex = p0;
                    _lastLaunchPos = launchPos;

                    return true;
                }
            }
            
            return false;
        }

        private float GetMaxPosition(float currentMax, Vector3 pos)
        {
            float val = GetPositionForPlane(pos);
            return val > currentMax ? val : currentMax;
        }
        
        private float GetMinPosition(float currentMin, Vector3 pos)
        {
            float val = GetPositionForPlane(pos);
            return val < currentMin ? val : currentMin;
        }

        public float GetPositionForPlane(Vector3 pos)
        {
            if (_samplePlaneIndex == 0)
            {
                return pos.x;
            }
            if (_samplePlaneIndex == 1)
            {
                return pos.y;
            }
            if (_samplePlaneIndex == 2)
            {
                return pos.z;
            }

            return 0.0f;
        }

        private void GenerateMotionPointsFromPattern(AnimationPattern pattern, ref List<MotionPoint> points,
            out float outMinY, out float outMaxY)
        {
            outMinY = float.MaxValue;
            outMaxY = float.MinValue;

            points.Clear();
            for (int i = 0; i < pattern.steps.Length; i++)
            {
                MotionPoint point;

                point.Time = pattern.steps[i].timeStep;
                point.Position = pattern.steps[i].point.position;

                if (_useLocalSpace.val)
                {
                    point.Position = pattern.transform.InverseTransformPoint(point.Position);
                }

                points.Add(point);

                outMaxY = GetMaxPosition(outMaxY, point.Position);
                outMinY = GetMinPosition(outMinY, point.Position);

                if (_includeMidPoints.val)
                {
                    point.Time = pattern.steps[i].timeStep +
                                 pattern.steps[(i + 1) % pattern.steps.Length].transitionToTime * 0.5f;
                    point.Position = pattern.GetPositionFromPoint(i, 0.5f);

                    if (_useLocalSpace.val)
                    {
                        point.Position = pattern.transform.InverseTransformPoint(point.Position);
                    }

                    points.Add(point);

                    outMaxY = GetMaxPosition(outMaxY, point.Position);
                    outMinY = GetMinPosition(outMinY, point.Position);
                }
            }
        }

        private bool GetMotionPointIndices(float time, List<MotionPoint> points, out int p0, out int p1)
        {
            if (points.Count <= 1)
            {
                p0 = -1;
                p1 = -1;
                return false;
            }

            for (int i = 0; i < points.Count; i++)
            {
                if (time < points[i].Time)
                {
                    continue;
                }
                
                int nextPoint = (i + 1) % points.Count;
                if (nextPoint == 0 || time < points[nextPoint].Time)
                {
                    p0 = i;
                    p1 = nextPoint;
                    return true;
                }
            }

            p0 = -1;
            p1 = -1;
            return false;
        }
        
        public void OnSimulatorUpdate(float prevPos, float newPos, float deltaTime)
        {
            
        }

        public void OnDestroy(VAMLaunch plugin)
        {
            DestroyOptionsUI(plugin);
        }
    }
}