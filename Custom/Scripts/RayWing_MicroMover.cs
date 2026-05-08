/*
    MicroMover script to add some human-like micro movements to a person. 
    Copyright (C) 2020  RayWing - https://www.reddit.com/user/RayWing

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace RayWing
{
    using System;
    using UnityEngine;
    using System.Linq;
    using System.Collections.Generic;

    // Science: Regular movement human hand (wrist) acceleration in world space is ~5m/s^2. Speed is up to ~3m/s.
    // Assumptions: elbow 2.5m/s^2 and 1.5m/s, foot 10m/s^2 and 6m/s, hips 2.5m/s^2 and 1.5m/s, knee (free movement) 5m/s^2 and 3 m/s.

    public class MicroMover : MVRScript
    {
        public static string pluginName = "MicroMover";
        public static string pluginVersion = "0.3";

        private static readonly float HIP_MOVEMENT_MIN_DEFAULT = 0f;
        private static readonly float HIP_MOVEMENT_MAX_DEFAULT = 0.03f;
        private static readonly float HIP_MOVEMENT_DURATION_DEFAULT = 0.6f;

        static readonly System.Random RANDOM = new System.Random();

        // How far can a motion overshoot relative to motion's distance. Clamped to OVERSHOOT_DISTANCE_MAX_ABSOLUTE for big motions.
        static readonly float OVERSHOOT_DISTANCE_FRACTION = 0.1f;
        static readonly float OVERSHOOT_DISTANCE_MAX_ABSOLUTE = 0.01f;
        // Which part of movement time is spent correcting the overshoot error. Clamped to CORRECTION_TIME_MAX_ABSOLUTE for long motions.
        static readonly float CORRECTION_TIME_FRACTION = 0.2f;
        static readonly float CORRECTION_TIME_MAX_ABSOLUTE = 0.5f;

        // Adjusts maximum possible perpendicular error of main move.
        static readonly float MOTION_PERPENDICULAR_ERROR_MULTIPLIER = 1f;

        protected readonly float BIG_BODY_PARTS_ACCELERATION = 2.5f;
        protected readonly float STANDING_STRAIGHT_HIP_DISTANCE_FROM_CENTER_MAX = 0.01f;
        protected readonly float HIP_MOVEMENT_DURATION_JITTER_FACTOR = 0.5f;

        protected readonly float HIP_MOVEMENT_MAX_DRIFT_FACTOR = 2f;

        // TODO check if center of mass is too far away from the foot-foot line and refuse to work if it is

        protected JSONStorableFloat hipMovementDistanceMin;
        protected JSONStorableFloat hipMovementDistanceMax;
        protected JSONStorableFloat hipMovementDurationSec;

        protected JSONStorableBool isHipMovementEnabled;
        protected JSONStorableBool isKneesMovementEnabled;
        protected JSONStorableBool isChestMovementEnabled;

        protected JSONStorableFloat bigBodyPartsAcceleration;
        protected JSONStorableBool isPersonMovementAndPoseLoadingBlocked;
        // If hip point's projection is up to this far from middle point between feet we consider a person standing straight
        protected JSONStorableFloat standingStraightDistanceFromCenterMax;
        protected JSONStorableFloat hipMovementDurationJitterFactor;

        static JSONStorableFloat movementPerpendicularErrorMultiplier;
        static JSONStorableFloat overshootDistanceFraction;
        static JSONStorableFloat overshootDistanceMax;
        static JSONStorableFloat overshootCorrectionTimeFraction;
        static JSONStorableFloat overshootCorrectionTimeMax;

        private List<JSONStorableFloat> storableFloats = new List<JSONStorableFloat>();

        private readonly float poseAdjustmentIntervalSec = 0.3f;
        private readonly float poseAdjustmentIntervalJitterFactor = 0.5f;

        private readonly string HIP_CONTROL = "hipControl";
        private readonly string LKNEE_CONTROL = "lKneeControl";
        private readonly string RKNEE_CONTROL = "rKneeControl";
        private readonly string LFOOT_CONTROL = "lFootControl";
        private readonly string RFOOT_CONTROL = "rFootControl";
        private readonly string CHEST_CONTROL = "chestControl";
        private readonly string LARM_CONTROL = "lArmControl";
        private readonly string RARM_CONTROL = "rArmControl";
        private readonly string LTHIGH_CONTROL = "lThighControl";
        private readonly string RTHIGH_CONTROL = "rThighControl";
        private readonly string MAIN_CONTROL = "control";

        private readonly string LEFT = "left";
        private readonly string RIGHT = "right";

        private bool isInitSuccessful = false;

        private float nextPoseAdjustmentTime;

        private Atom person;

        private Dictionary<string, Control> controls = new Dictionary<string, Control>();

        // IMPORTANT - DO NOT make custom enums. The dynamic C# complier crashes Unity when it encounters these for
        // some reason

        // IMPORTANT - DO NOT OVERRIDE Awake() as it is used internally by MVRScript - instead use Init() function which
        // is called right after creation
        public override void Init()
        {
            try
            {
                // put init code in here
                JSONStorable geometry = containingAtom.GetStorableByID("geometry");
                if (geometry == null)
                {
                    SuperController.LogError("The script only works if added to a Person");
                }

                person = containingAtom;

                InitInterface();
                InitControls();

                isInitSuccessful = true;
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        private void InitInterface()
        {
            hipMovementDistanceMin = CreateStorableFloatWithSlider("Hip movement distance min", HIP_MOVEMENT_MIN_DEFAULT, MinHipDistanceCallback, 0f, 1f, false);
            hipMovementDistanceMax = CreateStorableFloatWithSlider("Hip movement distance max", HIP_MOVEMENT_MAX_DEFAULT, MaxHipDistanceCallback, 0f, 1f, true);
            hipMovementDurationSec = CreateStorableFloatWithSlider("Hip movement duration seconds", HIP_MOVEMENT_DURATION_DEFAULT, 0.01f, 10f, false);

            isHipMovementEnabled = new JSONStorableBool("Move hip", true, val => SetMovementEnabled(val, HIP_CONTROL));
            isHipMovementEnabled.storeType = JSONStorableParam.StoreType.Full;
            RegisterBool(isHipMovementEnabled);
            CreateToggle(isHipMovementEnabled);

            isKneesMovementEnabled = new JSONStorableBool("Move knees", true, val => SetMovementEnabled(val, LKNEE_CONTROL, RKNEE_CONTROL));
            isKneesMovementEnabled.storeType = JSONStorableParam.StoreType.Full;
            RegisterBool(isKneesMovementEnabled);
            CreateToggle(isKneesMovementEnabled);

            isChestMovementEnabled = new JSONStorableBool("Move chest", true, val => SetMovementEnabled(val, CHEST_CONTROL));
            isChestMovementEnabled.storeType = JSONStorableParam.StoreType.Full;
            RegisterBool(isChestMovementEnabled);
            CreateToggle(isChestMovementEnabled);

            CreateSpacer();
            JSONStorableString debugControlsText = new JSONStorableString("Debug controls text", "Debug controls below. Expect weird effects.");
            UIDynamicTextField debugControlsTextField = CreateTextField(debugControlsText);
            debugControlsTextField.height = 30;

            isPersonMovementAndPoseLoadingBlocked = new JSONStorableBool("Block person movement/pose load", false);
            isPersonMovementAndPoseLoadingBlocked.storeType = JSONStorableParam.StoreType.Full;
            RegisterBool(isPersonMovementAndPoseLoadingBlocked);
            CreateToggle(isPersonMovementAndPoseLoadingBlocked);

            hipMovementDurationJitterFactor = CreateStorableFloatWithSlider("Movement duration extension", HIP_MOVEMENT_DURATION_JITTER_FACTOR, 0f, 10f, false);
            bigBodyPartsAcceleration = CreateStorableFloatWithSlider("Movement acceleration", BIG_BODY_PARTS_ACCELERATION, 0.01f, 10f, false);
            movementPerpendicularErrorMultiplier = CreateStorableFloatWithSlider("Perpendicular error multiplier", MOTION_PERPENDICULAR_ERROR_MULTIPLIER, 0f, 10f, false);
            overshootDistanceFraction = CreateStorableFloatWithSlider("Overshoot distance factor", OVERSHOOT_DISTANCE_FRACTION, 0f, 1f, false);
            overshootDistanceMax = CreateStorableFloatWithSlider("Overshoot distance max", OVERSHOOT_DISTANCE_MAX_ABSOLUTE, 0f, 1f, false);
            overshootCorrectionTimeFraction = CreateStorableFloatWithSlider("Overshoot correction time factor", CORRECTION_TIME_FRACTION, 0.01f, 0.99f, false);
            overshootCorrectionTimeMax = CreateStorableFloatWithSlider("Overshoot correction time max", CORRECTION_TIME_MAX_ABSOLUTE, 0.01f, 10f, false);
            standingStraightDistanceFromCenterMax = CreateStorableFloatWithSlider("isStandingStraight hip distance max", STANDING_STRAIGHT_HIP_DISTANCE_FROM_CENTER_MAX, 0.01f, 1f, false);
        }

        private void SetMovementEnabled(bool val, params string[] controlIds)
        {
            foreach (string controlId in controlIds)
            {
                controls[controlId].isMovementEnabled = val;
            }
        }

        private JSONStorableFloat CreateStorableFloatWithSlider(string name, float defaultValue, float min, float max, bool isRightSideSlider)
        {
            JSONStorableFloat result = new JSONStorableFloat(name, defaultValue, min, max);
            SetUpFloatStorable(result, isRightSideSlider);
            return result;
        }

        private JSONStorableFloat CreateStorableFloatWithSlider(string name, float defaultValue, JSONStorableFloat.SetJSONFloatCallback callback, float min, float max, bool isRightSideSlider)
        {
            JSONStorableFloat result = new JSONStorableFloat(name, defaultValue, callback, min, max);
            SetUpFloatStorable(result, isRightSideSlider);
            return result;
        }

        private void SetUpFloatStorable(JSONStorableFloat storable, bool isRightSideSlider)
        {
            storable.storeType = JSONStorableParam.StoreType.Full;
            CreateSlider(storable, isRightSideSlider);
            RegisterFloat(storable);
            storableFloats.Add(storable);
        }

        // OnDestroy is where you should put any cleanup
        // if you registered objects to supercontroller or atom, you should unregister them here
        void OnDestroy()
        {
            DeregisterBool(isHipMovementEnabled);
            DeregisterBool(isKneesMovementEnabled);
            DeregisterBool(isChestMovementEnabled);

            foreach (JSONStorableFloat storableFloat in storableFloats)
            {
                DeregisterFloat(storableFloat);
            }
        }

        private void InitControls()
        {
            InitControl(HIP_CONTROL, isHipMovementEnabled.val);
            InitControl(LARM_CONTROL, false);
            InitControl(RARM_CONTROL, false);
            InitControl(LKNEE_CONTROL, isKneesMovementEnabled.val);
            InitControl(RKNEE_CONTROL, isKneesMovementEnabled.val);
            InitControl(LFOOT_CONTROL, false);
            InitControl(RFOOT_CONTROL, false);
            InitControl(LTHIGH_CONTROL, false);
            InitControl(RTHIGH_CONTROL, false);
            InitControl(CHEST_CONTROL, isChestMovementEnabled.val);

            controls.Add(MAIN_CONTROL, new Control(MAIN_CONTROL, person.mainController, false));
        }

        private void InitControl(string name, bool isMovementEnabled)
        {
            var controller = person.freeControllers.First(fc => fc.name == name);
            controls.Add(name, new Control(name, controller, isMovementEnabled));
        }

        private void MinHipDistanceCallback(JSONStorableFloat newMinDistance)
        {
            if (hipMovementDistanceMax.val < newMinDistance.val)
            {
                hipMovementDistanceMax.SetVal(newMinDistance.val);
            }
        }

        private void MaxHipDistanceCallback(JSONStorableFloat newMaxDistance)
        {
            if (hipMovementDistanceMin.val > newMaxDistance.val)
            {
                hipMovementDistanceMin.SetVal(newMaxDistance.val);
            }
        }

        // Start is called once before Update or FixedUpdate is called and after Init()
        void Start()
        {
            if (!isInitSuccessful)
            {
                return;
            }
            try
            {
                ScheduleNextPoseAdjustment(Time.fixedTime);
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        // Update is called with each rendered frame by Unity
        void Update()
        {
        }

        // FixedUpdate is called with each physics simulation frame by Unity
        void FixedUpdate()
        {
            if (!isInitSuccessful)
            {
                return;
            }
            if (IsAffectedControlSelected() || IsPersonMoved() || IsPersonRotated())
            {
                StopAllMovements();
                RereadPositionsAndRotations(controls[MAIN_CONTROL]);
                return;
            }
            try
            {
                float now = Time.fixedTime;
                Control hipControl = controls[HIP_CONTROL];
                Movement hipMovement = hipControl.movement;
                if (hipMovement == null)
                {
                    foreach (KeyValuePair<string, Control> pair in controls)
                    {
                        Control control = pair.Value;
                        if (IsControlMoved(control))
                        {
                            RereadPositionsAndRotations(control);
                        }
                    }
                    InitiateNextMovements(now);
                }
                foreach (KeyValuePair<string, Control> pair in controls)
                {
                    Control control = pair.Value;
                    if (control.movement != null)
                    {
                        var isComplete = ExecuteMovementFixedFrame(control);
                        RereadAdjustedPositionAndRotation(control);
                        if (isComplete)
                        {
                            control.movement = null;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        private bool IsPersonMoved()
        {
            if (isPersonMovementAndPoseLoadingBlocked.val)
            {
                return false;
            }
            return IsControlMoved(controls[MAIN_CONTROL]);
        }

        private bool IsPersonRotated()
        {
            if (isPersonMovementAndPoseLoadingBlocked.val)
            {
                return false;
            }
            return IsControlRotated(controls[MAIN_CONTROL]);
        }

        private bool IsAffectedControlSelected()
        {
            foreach (KeyValuePair<string, Control> pair in controls)
            {
                Control control = pair.Value;
                if (control.isMovementEnabled && control.controller.selected)
                {
                    return true;
                }
            }
            return false;
        }

        private void StopAllMovements()
        {
            foreach (KeyValuePair<string, Control> pair in controls)
            {
                Control control = pair.Value;
                if (control.isMovementEnabled)
                {
                    control.movement = null;
                }
            }
        }

        private void RereadAdjustedPositionAndRotation(Control control)
        {
            control.adjustedPosition = control.controller.transform.position;
            control.adjustedRotation = control.controller.transform.rotation;
        }

        private void RereadPositionsAndRotations(Control control)
        {
            control.originalPosition = control.controller.transform.position;
            control.originalRotation = control.controller.transform.rotation;
            control.adjustedPosition = control.controller.transform.position;
            control.adjustedRotation = control.controller.transform.rotation;
        }

        private bool ExecuteMovementFixedFrame(Control control)
        {
            var now = Time.fixedTime;
            Vector3 translation = control.movement.GetTranslation(now, control.controller.transform.position);
            control.controller.transform.Translate(translation, Space.World);
            return now >= control.movement.endTime;
        }

        private void ScheduleNextPoseAdjustment(float nowSec)
        {
            float delay = poseAdjustmentIntervalSec * (1 + poseAdjustmentIntervalJitterFactor * (float)RANDOM.NextDouble());
            nextPoseAdjustmentTime = nowSec + delay;
        }

        private bool IsControlMoved(Control control)
        {
            return control.adjustedPosition != control.controller.transform.position;
        }

        private bool IsControlRotated(Control control)
        {
            return control.adjustedRotation != control.controller.transform.rotation;
        }

        private void InitiateNextMovements(float now)
        {
            float targetTime = now + hipMovementDurationSec.val * (1 + (hipMovementDurationJitterFactor.val * (float)RANDOM.NextDouble()));

            ISolver kneeSolver = new AccelerationWithMaxSpeedSolver(bigBodyPartsAcceleration.val / 2); // Knees are just following the hip here.
            Vector3 hipMovementVector = GetHipMovementVector();
            if (isHipMovementEnabled.val)
            {
                ISolver hipSolver = new AccelerationWithMaxSpeedAndOvershootSolver(bigBodyPartsAcceleration.val);
                controls[HIP_CONTROL].movement = new Movement(controls[HIP_CONTROL].controller.transform.position, controls[HIP_CONTROL].controller.transform.position + hipMovementVector, now, targetTime, hipSolver);
            }
            if (isKneesMovementEnabled.val)
            {
                controls[LKNEE_CONTROL].movement = new Movement(controls[LKNEE_CONTROL].controller.transform.position, controls[LKNEE_CONTROL].controller.transform.position + hipMovementVector / 2, now, targetTime, kneeSolver);
                controls[RKNEE_CONTROL].movement = new Movement(controls[RKNEE_CONTROL].controller.transform.position, controls[RKNEE_CONTROL].controller.transform.position + hipMovementVector / 2, now, targetTime, kneeSolver);
            }
            if (isChestMovementEnabled.val)
            {
                var chestMovementVector = hipMovementVector * (0.5f); // TODO can we do better here?
                ISolver chestSolver = new AccelerationWithMaxSpeedAndOvershootSolver(bigBodyPartsAcceleration.val);
                controls[CHEST_CONTROL].movement = new Movement(controls[CHEST_CONTROL].controller.transform.position, controls[CHEST_CONTROL].controller.transform.position + chestMovementVector, now, targetTime, chestSolver);
            }
        }

        private Vector3 GetHipMovementVector()
        {
            var left = GetHipMovementVector(LEFT);
            var right = GetHipMovementVector(RIGHT);
            var hipControl = controls[HIP_CONTROL];
            var drift = hipControl.adjustedPosition - hipControl.originalPosition;
            var driftAfterLeft = drift + left;
            var driftAfterRight = drift + right;
            var towardsOriginal = driftAfterLeft.sqrMagnitude < driftAfterRight.sqrMagnitude ? left : right;
            var awayFromOriginal = towardsOriginal == left ? right : left;
            double threshold = 0.5;
            if (hipMovementDistanceMax.val != 0) {
                var maxDriftMagnitude = hipMovementDistanceMax.val * HIP_MOVEMENT_MAX_DRIFT_FACTOR;
                threshold = 0.5 + 0.5 * drift.magnitude / maxDriftMagnitude;
            }
            if (RANDOM.NextDouble() < threshold)
            {
                return towardsOriginal;
            }
            else
            {
                return awayFromOriginal;
            }
        }

        private Vector3 ZeroY(Vector3 vector3)
        {
            return new Vector3(vector3.x, 0, vector3.z);
        }

        private string GetRandomSide(int previousLeft, int previousRight)
        {
            var randomDouble = RANDOM.NextDouble();
            double threshold = 0.5;
            if (previousLeft != 0 && previousRight != 0) {
                threshold = (double) previousLeft / (previousLeft + previousRight);
            }
            return randomDouble < threshold ? RIGHT : LEFT;
        }

        private Vector3 GetHipMovementVector(string direction)
        {
            var leftToRightThigh = controls[RTHIGH_CONTROL].controller.transform.position - controls[LTHIGH_CONTROL].controller.transform.position;
            var leftToRightShoulder = controls[RARM_CONTROL].controller.transform.position - controls[LARM_CONTROL].controller.transform.position;
            var leftToRight = leftToRightThigh + leftToRightShoulder;
            var movementDirection = direction == RIGHT ? leftToRight.normalized : leftToRight.normalized * (-1f);
            var movementDirectionDecreased = movementDirection * leftToRightThigh.magnitude;
            var directionWithRandomizedMagnitude = movementDirectionDecreased * (hipMovementDistanceMin.val + (hipMovementDistanceMax.val - hipMovementDistanceMin.val) * (float)RANDOM.NextDouble());
            return directionWithRandomizedMagnitude;
        }

        private class Movement
        {

            private static readonly ISolver DEFAULT_SOLVER = LinearSolver.INSTANCE;

            public readonly Vector3 startPosition;
            public readonly Vector3 endPosition;
            public readonly float startTime;
            public readonly float endTime;
            public readonly float duration;
            public readonly Vector3 fullTranslation;
            public readonly ISolver solver;

            public Movement(Vector3 startPosition, Vector3 endPosition, float startTime, float endTime) : this(startPosition, endPosition, startTime, endTime, DEFAULT_SOLVER)
            {
            }

            public Movement(Vector3 startPosition, Vector3 endPosition, float startTime, float endTime, ISolver solver)
            {
                this.startPosition = startPosition;
                this.endPosition = endPosition;
                this.startTime = startTime;
                this.endTime = endTime;
                this.duration = endTime - startTime;
                this.fullTranslation = endPosition - startPosition;
                this.solver = solver;
            }

            public Vector3 GetTranslation(float nowTime, Vector3 currentPosition)
            {
                return solver.GetTranslation(nowTime, startTime, duration, startPosition, fullTranslation, currentPosition);
            }
        }

        private class Control
        {

            public readonly string controlName;
            public readonly FreeControllerV3 controller;
            public Vector3 originalPosition;
            public Vector3 adjustedPosition;
            public Quaternion originalRotation;
            public Quaternion adjustedRotation;
            public Movement movement;
            public bool isMovementEnabled;

            public Control(string controlName, FreeControllerV3 controller, bool isMovementEnabled)
            {
                this.controlName = controlName;
                this.controller = controller;
                this.isMovementEnabled = isMovementEnabled;

                this.originalPosition = controller.transform.position;
                this.adjustedPosition = this.originalPosition;

                this.originalRotation = controller.transform.rotation;
                this.adjustedRotation = this.originalRotation;
            }
        }

        private interface ISolver
        {

            Vector3 GetTranslation(float nowTime, float startTime, float duration, Vector3 startPosition, Vector3 fullTranslation, Vector3 currentPosition);

        }

        private class LinearSolver : ISolver
        {
            public static readonly ISolver INSTANCE = new LinearSolver();

            public Vector3 GetTranslation(float nowTime, float startTime, float duration, Vector3 startPosition, Vector3 fullTranslation, Vector3 currentPosition)
            {
                if (nowTime > startTime + duration)
                {
                    return Vector3.zero;
                }
                var progress = Math.Min(1.0f, (nowTime - startTime) / duration);
                return (fullTranslation * progress) - (currentPosition - startPosition);
            }
        }

        private class AccelerationWithMaxSpeedAndOvershootSolver : ISolver
        {

            private readonly float acceleration;

            private float mainMotionCompletionTimeAbsolute;
            private float mainMotionDuration;
            private float correctionDuration;
            private Vector3 mainMotionTranslation;
            private Vector3 mainMotionEndPosition;
            private Vector3 correctionTranslation;

            private ISolver mainMotionSolver;
            private ISolver correctionSolver;

            private bool isInitialised = false;

            public AccelerationWithMaxSpeedAndOvershootSolver(float acceleration)
            {
                this.acceleration = acceleration;
                mainMotionSolver = new AccelerationWithMaxSpeedSolver(acceleration);
                correctionSolver = new AccelerationWithMaxSpeedSolver(acceleration);
            }

            public Vector3 GetTranslation(float nowTime, float startTime, float duration, Vector3 startPosition, Vector3 fullTranslation, Vector3 currentPosition)
            {
                if (!isInitialised)
                {
                    Initialize(nowTime, startTime, duration, startPosition, fullTranslation);
                }
                if (IsMainMotion(nowTime))
                {
                    return mainMotionSolver.GetTranslation(nowTime, startTime, mainMotionDuration, startPosition, mainMotionTranslation, currentPosition);
                }
                else
                {
                    return correctionSolver.GetTranslation(nowTime, mainMotionCompletionTimeAbsolute, correctionDuration, mainMotionEndPosition, correctionTranslation, currentPosition);
                }
            }

            private void Initialize(float nowTime, float startTime, float duration, Vector3 startPosition, Vector3 fullTranslation)
            {
                var overshootAdjustmentForSlowMotions = Clamp(1 - ((duration - 0.4f) / 3), 0f, 1f);
                var overshootDistance = Math.Min(overshootDistanceMax.val, fullTranslation.magnitude * overshootDistanceFraction.val) * overshootAdjustmentForSlowMotions;
                correctionDuration = Math.Min(overshootCorrectionTimeMax.val, duration * overshootCorrectionTimeFraction.val);
                mainMotionDuration = duration - correctionDuration;
                mainMotionCompletionTimeAbsolute = startTime + mainMotionDuration;
                mainMotionTranslation = fullTranslation + fullTranslation.normalized * overshootDistance;
                mainMotionTranslation += PerpendicularComponent(mainMotionTranslation, overshootDistance);
                mainMotionEndPosition = startPosition + mainMotionTranslation;
                correctionTranslation = startPosition + fullTranslation - mainMotionEndPosition;
                isInitialised = true;
            }

            private float Clamp(float value, float minInclusive, float maxInclusive)
            {
                if (value < minInclusive)
                {
                    return minInclusive;
                }
                else if (value > maxInclusive)
                {
                    return maxInclusive;
                }
                else
                {
                    return value;
                }
            }

            private Vector3 PerpendicularComponent(Vector3 perpendicularTo, float maximumMagnitude)
            {
                var magnitude = maximumMagnitude * (float)RANDOM.NextDouble() * movementPerpendicularErrorMultiplier.val;
                Vector3 randomVector = UnityEngine.Random.insideUnitSphere;
                return Vector3.Cross(perpendicularTo, randomVector).normalized * magnitude;
            }

            private bool IsMainMotion(float nowTime)
            {
                return nowTime <= mainMotionCompletionTimeAbsolute;
            }
        }


        private class AccelerationWithMaxSpeedSolver : ISolver
        {
            private static readonly float FALLBACK_MAX_SPEED = 0.1f;

            private readonly float acceleration;
            private float maxSpeed;

            private bool isInitialized = false;

            public AccelerationWithMaxSpeedSolver(float acceleration)
            {
                this.acceleration = acceleration;
            }

            public Vector3 GetTranslation(float nowTime, float startTime, float duration, Vector3 startPosition, Vector3 fullTranslation, Vector3 currentPosition)
            {
                if (nowTime > startTime + duration)
                {
                    return Vector3.zero;
                }
                if (!isInitialized)
                {
                    maxSpeed = DetermineMaxSpeed(acceleration, duration, fullTranslation);
                    isInitialized = true;
                }
                var halfDuration = duration / 2;
                float accelerationTime = Math.Min(maxSpeed / acceleration, halfDuration);
                float decelerationTime = accelerationTime;
                float constantSpeedTime = Math.Max(duration - accelerationTime - decelerationTime, 0);
                float achievedMaxSpeed = Math.Min(maxSpeed, acceleration * accelerationTime);
                var sinceStartTime = nowTime - startTime;
                float distanceMeters;
                if (sinceStartTime < accelerationTime)
                {
                    // ACCELERATION
                    distanceMeters = acceleration * sinceStartTime * sinceStartTime / 2;
                }
                else if (sinceStartTime < accelerationTime + constantSpeedTime)
                {
                    // CONSTANT_SPEED
                    distanceMeters = acceleration * accelerationTime * accelerationTime / 2 + maxSpeed * (sinceStartTime - accelerationTime);
                }
                else
                {
                    // DECELERATION
                    var sinceDecelerationStartTime = sinceStartTime - (accelerationTime + constantSpeedTime);
                    distanceMeters = acceleration * accelerationTime * accelerationTime / 2
                        + maxSpeed * constantSpeedTime
                        + (achievedMaxSpeed * sinceDecelerationStartTime - acceleration * sinceDecelerationStartTime * sinceDecelerationStartTime / 2);
                }
                var translation = (fullTranslation.normalized * distanceMeters) - (currentPosition - startPosition);
                return translation;
            }

            private float DetermineMaxSpeed(float acceleration, float duration, Vector3 fullTranslation)
            {
                var distance = fullTranslation.magnitude;
                var b = (-1) * acceleration * duration;
                var c = acceleration * distance;
                var sqrtpart = (b * b) - (4 * c);

                var maxSpeed = ((-1) * b - (float)Math.Sqrt(sqrtpart)) / 2;
                var constantSpeedTime = (distance - (maxSpeed * maxSpeed) / acceleration) / maxSpeed;
                if (constantSpeedTime > 0 && maxSpeed > 0)
                {
                    return maxSpeed;
                }
                else
                {
                    maxSpeed = ((-1) * b + (float)Math.Sqrt(sqrtpart)) / 2;
                    constantSpeedTime = (distance - (maxSpeed * maxSpeed) / acceleration) / maxSpeed;
                    if (constantSpeedTime > 0 && maxSpeed > 0)
                    {
                        return maxSpeed;
                    }
                }

                return FALLBACK_MAX_SPEED;
            }
        }
    }
}
