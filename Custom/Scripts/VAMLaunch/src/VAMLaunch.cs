using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VAMLaunchPlugin.MotionSources;

namespace VAMLaunchPlugin
{
    public class VAMLaunch : MVRScript
    {
        private static VAMLaunch _instance;

        private bool logMessages = false;
        
        private const string SERVER_IP = "127.0.0.1";
        private const int SERVER_LISTEN_PORT = 15600;
        private const int SERVER_SEND_PORT = 15601;
        private const float NETWORK_LISTEN_INTERVAL = 0.033f;
        
        private VAMLaunchNetwork _network;
        private float _networkPollTimer;

        private byte _lastSentLaunchPos;

        private JSONStorableStringChooser _motionSourceChooser;
        private JSONStorableBool _pauseLaunchMessages;
        private JSONStorableBool _autoNetwork;
        private JSONStorableFloat _simulatorPosition;

        private float _simulatorTarget;
        private float _simulatorSpeed;

        private bool _initNetworkDone = false;

        private IMotionSource _currentMotionSource;
        private int _currentMotionSourceIndex = -1;
        private int _desiredMotionSourceIndex = 3;

        private string _scriptConnectionAtomName = "VAMLaunchScriptConnection"; //this will be used to communicate with other scripts 
        private Atom _scriptConnectionAtom = null;
        private Transform _scriptConnectionTransform = null;
        private string _desiredTargetPrefix = "";

        private bool wantToSetParams = false;
        private bool paramInvert = false;
        private bool updateMotionSource = false;

        private List<string> _motionSourceChoices = new List<string>
        {
            "Oscillate",
            "Pattern",
            "Zone",
            "CycleForce",
            "RhythmForce",
            "ScriptConnection"
        };

        private List<IMotionSource> _motionSources = new List<IMotionSource>
        {
            new OscillateSource(),
            new PatternSource(),
            new ZoneSource(),
            new CycleForceSource(),
            new RhythmForceSource(),
            new ScriptConnectionSource()
        };
        
        public override void Init()
        {
            if (_instance != null)
            {
                SuperController.LogError("You can only have one instance of VAM Launch active!");
                return;
            }
            
            if (containingAtom == null || containingAtom.type == "CoreControl")
            {
                SuperController.LogError("Please add VAM Launch to in scene atom!");
                return;
            }

            _instance = this;

            InitStorables();
            InitOptionsUI();
            InitActions();
            // InitNetwork();
        }

        private void InitNetwork()
        {
            _network = new VAMLaunchNetwork();
            _network.Init(SERVER_IP, SERVER_LISTEN_PORT, SERVER_SEND_PORT);
            Log("VAM Launch network connection initialized.");
        }
        
        private void InitStorables()
        {
            _motionSourceChooser = new JSONStorableStringChooser("motionSource", _motionSourceChoices, "",
                "Motion Source",
                (string name) => { if (name == "" || name == null) return; _desiredMotionSourceIndex = GetMotionSourceIndex(name); });
            _motionSourceChooser.choices = _motionSourceChoices;
            RegisterStringChooser(_motionSourceChooser);
            
            _pauseLaunchMessages = new JSONStorableBool("pauseLaunchMessages", false);
            RegisterBool(_pauseLaunchMessages);

            _autoNetwork = new JSONStorableBool("autoNetwork", true);
            RegisterBool(_autoNetwork);

            _simulatorPosition = new JSONStorableFloat("simulatorPosition", 0.0f, 0.0f, LaunchUtils.LAUNCH_MAX_VAL);
            RegisterFloat(_simulatorPosition);

            foreach (var ms in _motionSources)
            {
                ms.OnInitStorables(this);
            }
        }
        
        private void InitOptionsUI()
        {
            if (string.IsNullOrEmpty(_motionSourceChooser.val))
            {
                _motionSourceChooser.SetVal(_motionSourceChoices[3]);
            }

            var toggle = CreateToggle(_pauseLaunchMessages);
            toggle.label = "Pause Launch";

            toggle = CreateToggle(_autoNetwork);
            toggle.label = "Auto Network";

            var slider = CreateSlider(_simulatorPosition, false);
            slider.label = "Simulator";
            CreateScrollablePopup(_motionSourceChooser);

            CreateSpacer();
        }

        private void InitActions()
        {
            JSONStorableAction startLaunchAction = new JSONStorableAction("startLaunch", () =>
            {
                _pauseLaunchMessages.SetVal(false);
            });
            RegisterAction(startLaunchAction);
            
            JSONStorableAction stopLaunchAction = new JSONStorableAction("stopLaunch", () =>
            {
                _pauseLaunchMessages.SetVal(true);
            });
            RegisterAction(stopLaunchAction);
            
            JSONStorableAction toggleLaunchAction = new JSONStorableAction("toggleLaunch", () =>
            {
                _pauseLaunchMessages.SetVal(!_pauseLaunchMessages.val);
            });
            RegisterAction(toggleLaunchAction);

            JSONStorableAction toggleLaunchActivateAction = new JSONStorableAction("toggleLaunchActivate", () =>
            {
                _pauseLaunchMessages.SetVal(true);
            });
            RegisterAction(toggleLaunchActivateAction);

            JSONStorableAction toggleLaunchPauseAction = new JSONStorableAction("toggleLaunchPause", () =>
            {
                _pauseLaunchMessages.SetVal(!false);
            });
            RegisterAction(toggleLaunchPauseAction);

            JSONStorableAction launchNetworkStart = new JSONStorableAction("launchNetworkStart", () =>
            {
                if (_network == null)
                {
                    try
                    {
                        InitNetwork();
                    }
                    catch (System.Exception e)
                    {
                        SuperController.LogError("InitNetwork failed: " + e.ToString());
                    }
                }
            });
            RegisterAction(launchNetworkStart);

            JSONStorableAction launchNetworkStop = new JSONStorableAction("launchNetworkStop", () =>
            {
                if (_network != null)
                {
                    try
                    {
                        // SuperController.LogMessage("Shutting down VAM Launch network");
                        _network.Stop();
                        _network = null;
                    }
                    catch (System.Exception e)
                    {
                        SuperController.LogError(e.ToString());
                    }
                }
            });
            RegisterAction(launchNetworkStop);
        }

        private int GetMotionSourceIndex(string name)
        {
            return _motionSourceChoices.IndexOf(name);
        }

        private void Log(string theMessage)
        {
            if (logMessages) SuperController.LogMessage(theMessage);
        }
        
        private void UpdateMotionSource()
        {
            if (_desiredMotionSourceIndex != _currentMotionSourceIndex || updateMotionSource)
            {
                updateMotionSource = false;
                if (_currentMotionSource != null)
                {
                    Log("destroy: " + _currentMotionSource);
                    _currentMotionSource.OnDestroy(this);
                    _currentMotionSource = null;
                }
                Log("_desiredMotionSourceIndex " + _desiredMotionSourceIndex);

                if (_desiredMotionSourceIndex >= 0)
                {
                    Log("init: " + _desiredMotionSourceIndex);
                    _currentMotionSource = _motionSources[_desiredMotionSourceIndex];
                    _currentMotionSource.OnInit(this, _desiredTargetPrefix);
                }

                _currentMotionSourceIndex = _desiredMotionSourceIndex;
            }

            if (_currentMotionSource != null)
            {
                if (wantToSetParams)
                {
                    Log("set params");
                    wantToSetParams = false;
                    _currentMotionSource.SetInvert(paramInvert);
                }
                byte pos = 0;
                byte speed = 0;
                if (_currentMotionSource.OnUpdate(ref pos, ref speed))
                {
                    SendLaunchPosition(pos, speed);
                }
            }
        }
        
        public void OnDestroy()
        {
            if (_network != null)
            {
                Log("Shutting down VAM Launch network."); 
                _network.Stop();
                _network = null;
            }

            if (_instance == this)
            {
                _instance = null;
            }
        }

        public void Start()
        {
            _scriptConnectionAtom = SuperController.singleton.GetAtomByUid(_scriptConnectionAtomName);
            if (_scriptConnectionAtom == null)
            {
                StartCoroutine(CreateScriptConnectionAtom());
            }
        }

        private IEnumerator CreateScriptConnectionAtom()
        {
            yield return SuperController.singleton.AddAtomByType("Empty", _scriptConnectionAtomName);
            _scriptConnectionAtom = SuperController.singleton.GetAtomByUid(_scriptConnectionAtomName);
        }

        public void Update()
        {
            if (_scriptConnectionTransform == null)
            {
                _scriptConnectionAtom = SuperController.singleton.GetAtomByUid(_scriptConnectionAtomName);
                if (_scriptConnectionAtom != null)
                { 
                    _scriptConnectionTransform = _scriptConnectionAtom.transform;
                }
            } else if (_scriptConnectionTransform != null)
            {
                //should we change the source
                if (_scriptConnectionTransform.position.z > 0)
                {
                    //what are we being told to do?
                    _desiredTargetPrefix = "";
                    if (_scriptConnectionTransform.position.z <= 0.11f)
                    {
                        //switch to easy moan cycle force
                        _desiredTargetPrefix = "CycleForce_EM_";
                        wantToSetParams = true;
                        paramInvert = false;
                        updateMotionSource = true;
                        _motionSourceChooser.SetVal(_motionSourceChoices[3]);
                    }
                    else if (_scriptConnectionTransform.position.z <= 0.21f)
                    {
                        //switch to animation pattern for dollmaster
                        _desiredTargetPrefix = "Thrust AP";
                        updateMotionSource = true;
                        _motionSourceChooser.SetVal(_motionSourceChoices[1]);
                    }
                    else if (_scriptConnectionTransform.position.z <= 0.31f)
                    {
                        //switch to script connection, can be used for easy moan dildo
                        wantToSetParams = true; //not setting params
                        paramInvert = false;
                        updateMotionSource = true;
                        _motionSourceChooser.SetVal(_motionSourceChoices[5]);
                    }
                    else if (_scriptConnectionTransform.position.z <= 0.41f)
                    {
                        //switch to spankings cycle force
                        _desiredTargetPrefix = "CycleForce_Spank_";
                        updateMotionSource = true;
                        _motionSourceChooser.SetVal(_motionSourceChoices[3]);
                    }

                    _scriptConnectionTransform.position = new Vector3(_scriptConnectionTransform.position.x, _scriptConnectionTransform.position.y, -0.1f);
                }
            }

            UpdateMotionSource();


            if (!_initNetworkDone)
            {
                _initNetworkDone = true;
                if (_autoNetwork.val)
                {
                    try
                    {
                        InitNetwork();
                    }
                    catch (System.Exception e)
                    {
                        SuperController.LogError("InitNetwork failed: " + e.ToString());
                    }
                }
            }

            UpdateNetwork();
            UpdateSimulator();
        }

        private void UpdateSimulator()
        {
            var prevPos = _simulatorPosition.val;

            var newPos = Mathf.MoveTowards(prevPos, _simulatorTarget,
                LaunchUtils.PredictDistanceTraveled(_simulatorSpeed, Time.deltaTime));
            
            _simulatorPosition.SetVal(newPos);

            if (_currentMotionSource != null)
            {
                _currentMotionSource.OnSimulatorUpdate(prevPos, newPos, Time.deltaTime);
            }
        }

        private void SetSimulatorTarget(float pos, float speed)
        {
            _simulatorTarget = Mathf.Clamp(pos, 0.0f, LaunchUtils.LAUNCH_MAX_VAL);
            _simulatorSpeed = Mathf.Clamp(speed, 0.0f, LaunchUtils.LAUNCH_MAX_VAL);
        }
        
        // Not really used yet, but there just incase we want to do two way communication between server
        private void UpdateNetwork()
        {
            if (_network == null)
            {
                return;
            }

            _networkPollTimer -= Time.deltaTime;
            if (_networkPollTimer <= 0.0f)
            {
                ReceiveNetworkMessages();
                _networkPollTimer = NETWORK_LISTEN_INTERVAL - Mathf.Min(-_networkPollTimer, NETWORK_LISTEN_INTERVAL);
            }
        }

        private void ReceiveNetworkMessages()
        {
            byte[] msg = _network.GetNextMessage();
            if (msg != null && msg.Length > 0)
            {
                //Log(msg[0].ToString());
            }
        }

        
        private static byte[] _launchData = new byte[6];
        private void SendLaunchPosition(byte pos, byte speed)
        {
            SetSimulatorTarget(pos, speed);
            
            if (_network == null)
            {
                return;
            }

            if (!_pauseLaunchMessages.val)
            {
                _launchData[0] = pos;
                _launchData[1] = speed;

                float dist = Mathf.Abs(pos - _lastSentLaunchPos);
                float duration = LaunchUtils.PredictMoveDuration(dist, speed);
                    
                var durationData = BitConverter.GetBytes(duration);
                _launchData[2] = durationData[0];
                _launchData[3] = durationData[1];
                _launchData[4] = durationData[2];
                _launchData[5] = durationData[3];
                
                //Log(string.Format("Sending: P:{0}, S:{1}, D:{2}", pos, speed, duration));
                
                _network.Send(_launchData, _launchData.Length);

                _lastSentLaunchPos = pos;
            }
        }
    }
}