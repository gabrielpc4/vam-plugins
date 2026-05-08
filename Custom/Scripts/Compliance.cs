using UnityEngine;

namespace Compliance
{
    // Compliance 2.1

    public class JointControl : MVRScript
    {
        /// <summary>
        /// VaM Specific Code
        /// </summary>

        private JSONStorableFloat CreateFloatSlider(string name, float defaultValue, float minValue, float maxValue)
        {
            JSONStorableFloat storable = new JSONStorableFloat(name, defaultValue, minValue, maxValue, true, true);
            storable.storeType = JSONStorableParam.StoreType.Full;
            CreateSlider(storable);
            RegisterFloat(storable);
            return storable;
        }

        private JSONStorableBool CreateBoolCheckbox(string name, bool defaultValue)
        {
            JSONStorableBool storable = new JSONStorableBool(name, defaultValue);
            storable.storeType = JSONStorableParam.StoreType.Full;
            RegisterBool(storable);
            CreateToggle(storable);
            return storable;
        }

        void Log(string message)
        {
            SuperController.LogMessage(message);
        }
        void Error(string message)
        {
            SuperController.LogError(message);
        }

        public override void Init()
        {
            try
            {
                jointControls = containingAtom.GetComponentsInChildren<FreeControllerV3>(true);

                //threshold = CreateFloatSlider("Position Threshold", 0.03f, 0.001f, 0.1f);
                //thresholdRot = CreateFloatSlider("Rotation Threshold", 5.0f, 0.1f, 10.0f);
                speed = CreateFloatSlider("Speed", 10, 1, 100);
                comply = CreateBoolCheckbox("Auto Comply Joints", true);
                balance = CreateBoolCheckbox("Balance", true);
                relax = CreateBoolCheckbox("Relax Arms", true);
                shoulderFollow = CreateBoolCheckbox("Hip Rotation Follows Shoulders", false);
                layThreshold = CreateFloatSlider("Laying Threshold", 0.2f, 0.1f, 1.0f);
                //straighten = CreateFloatSlider("Straighten Force", 0.005f, 0.001f, 0.02f);
                footHeight = CreateFloatSlider("Foot Trace Height", 0.03f, 0.03f, 0.5f);
            }
            catch (System.Exception e)
            {
                Error("Exception caught: " + e);
            }
        }

        /// <summary>
        /// COMPLIANCE CODE
        /// </summary>

        bool initialized = false;

        private FreeControllerV3[] jointControls;

        public JSONStorableFloat threshold;
        public JSONStorableFloat thresholdRot;
        public JSONStorableFloat speed;
        public JSONStorableFloat antigravity;
        public JSONStorableBool comply;
        public JSONStorableBool balance;
        public JSONStorableBool relax;
        public JSONStorableBool shoulderFollow;
        public JSONStorableFloat layThreshold;
        public JSONStorableFloat straighten;
        public JSONStorableFloat footHeight;

        FreeControllerV3 head;
        FreeControllerV3 chest;
        FreeControllerV3 hip;
        FreeControllerV3 footL;
        FreeControllerV3 footR;
        FreeControllerV3 kneeL;
        FreeControllerV3 kneeR;
        FreeControllerV3 shoulderL;
        FreeControllerV3 shoulderR;
        FreeControllerV3 elbowL;
        FreeControllerV3 elbowR;
        FreeControllerV3 handL;
        FreeControllerV3 handR;
        FreeControllerV3 hipL;
        FreeControllerV3 hipR;

        //Vector3 lfpos, rfpos;
        bool canWalk = false;
        bool canStand = false;
        bool canSit = false;

        bool wantsStand = false;

        int laySupports = 0;

        Vector3 lTarget, rTarget;
        Quaternion lRotation, rRotation;
        bool lUp, rUp;
        bool lPlanted, rPlanted;
        float stepDistance = 0;

        bool complyKneeL = false;
        bool complyKneeR = false;

        float lastHeadHeight = 0;

        float legLength = 0;

        Vector3 centerOfMass;
        Vector3 prevCoM;

        float walkInterp = 0;

        const float maxfootheight = 0.5f;
        const float raisehipheight = 0.75f;
        const float maxhipheight = 0.9f;
        const float minhipheight = 0.5f;
        const float maxfooterror = 0.1f;
        const float stepheight = 0.25f;
        const float maxlean = 0.1f;
        const float maxtilt = 0.2f;

        protected void FixedUpdate()
        {
            try
            {
                UpdateCompliance();
            }
            catch (System.Exception e)
            {
                Error("Exception caught: " + e);
                enabled = false;
            }
        }

        protected void Update()
        {
            try
            {
                UpdateStep();
            }
            catch (System.Exception e)
            {
                Error("Exception caught: " + e);
                enabled = false;
            }
        }

        private void UpdateCompliance()
        {
            if (!initialized)
            {
                foreach (var c in jointControls)
                {
                    if (c == null || c.followWhenOff == null || c.control == null)
                        continue;
                    
                    switch (c.name)
                    {
                        case "headControl":
                            head = c;
                            break;
                        case "chestControl":
                            chest = c;
                            break;
                        case "hipControl":
                            hip = c;
                            break;
                        case "lFootControl":
                            footL = c;
                            break;
                        case "rFootControl":
                            footR = c;
                            break;
                        case "lHandControl":
                            handL = c;
                            break;
                        case "rHandControl":
                            handR = c;
                            break;
                        case "lThighControl":
                            hipL = c;
                            break;
                        case "rThighControl":
                            hipR = c;
                            break;
                        case "lArmControl":
                            shoulderL = c;
                            break;
                        case "rArmControl":
                            shoulderR = c;
                            break;
                        case "lElbowControl":
                            elbowL = c;
                            break;
                        case "rElbowControl":
                            elbowR = c;
                            break;
                        case "lKneeControl":
                            kneeL = c;
                            break;
                        case "rKneeControl":
                            kneeR = c;
                            break;
                    }
                }

                legLength = 0;
                legLength += Vector3.Distance(hipL.followWhenOff.position, kneeL.followWhenOff.position);
                legLength += Vector3.Distance(footL.followWhenOff.position, kneeL.followWhenOff.position);

                initialized = true;
                
                head.currentPositionState = FreeControllerV3.PositionState.Comply;
                head.currentRotationState = FreeControllerV3.RotationState.Comply;

                chest.currentPositionState = FreeControllerV3.PositionState.Comply;
                chest.currentRotationState = FreeControllerV3.RotationState.Off;

                hip.currentPositionState = FreeControllerV3.PositionState.Comply;
                hip.currentRotationState = FreeControllerV3.RotationState.Comply;

                handL.currentPositionState = FreeControllerV3.PositionState.Comply;
                handL.currentRotationState = FreeControllerV3.RotationState.Comply;

                handR.currentPositionState = FreeControllerV3.PositionState.Comply;
                handR.currentRotationState = FreeControllerV3.RotationState.Comply;

                elbowL.currentPositionState = FreeControllerV3.PositionState.Comply;
                elbowL.currentRotationState = FreeControllerV3.RotationState.Off;

                elbowR.currentPositionState = FreeControllerV3.PositionState.Comply;
                elbowR.currentRotationState = FreeControllerV3.RotationState.Off;

                shoulderL.currentPositionState = FreeControllerV3.PositionState.Comply;
                shoulderL.currentRotationState = FreeControllerV3.RotationState.Off;

                shoulderR.currentPositionState = FreeControllerV3.PositionState.Comply;
                shoulderR.currentRotationState = FreeControllerV3.RotationState.Off;

                footL.currentPositionState = FreeControllerV3.PositionState.Comply;
                footL.currentRotationState = FreeControllerV3.RotationState.Comply;

                footR.currentPositionState = FreeControllerV3.PositionState.Comply;
                footR.currentRotationState = FreeControllerV3.RotationState.Comply;

                kneeL.currentPositionState = FreeControllerV3.PositionState.Comply;
                kneeL.currentRotationState = FreeControllerV3.RotationState.Off;

                kneeR.currentPositionState = FreeControllerV3.PositionState.Comply;
                kneeR.currentRotationState = FreeControllerV3.RotationState.Off;
            }

            if (balance.val && hip.currentPositionState == FreeControllerV3.PositionState.Comply)
            {
                // compute approximate center of mass
                const float w0 = 2.0f;      Vector3 v0 = hip.followWhenOff.position;
                const float w1 = 0.25f;     Vector3 v1 = kneeL.followWhenOff.position;
                const float w2 = 0.25f;     Vector3 v2 = kneeR.followWhenOff.position;
                const float w3 = 0.1f;      Vector3 v3 = elbowL.followWhenOff.position;
                const float w4 = 0.1f;      Vector3 v4 = elbowR.followWhenOff.position;
                const float w5 = 0.25f;     Vector3 v5 = head.followWhenOff.position;

                const float total = 1.0f / (w0+w1+w2+w3+w4+w5);

                centerOfMass = total *
                    (w0 * v0 +
                    w1 * v1 +
                    w2 * v2 +
                    w3 * v3 +
                    w4 * v4 +
                    w5 * v5);

                Vector3 footTargetBase = 0.5f * (footL.control.position + footR.control.position);
                Vector3 footBase = 0.5f * (footL.followWhenOff.position + footR.followWhenOff.position);
                footTargetBase.y = Mathf.Min(footL.control.position.y, footR.control.position.y);
                footBase.y = Mathf.Min(footL.followWhenOff.position.y, footR.followWhenOff.position.y);

                Vector3 lean = head.followWhenOff.position - hip.followWhenOff.position;
                lean.y = 0;
                Vector3 balance = footBase - centerOfMass;
                balance.y = 0;

                float upspeed = 0.0f;

                if (canStand)
                {
                    if (canWalk || hip.followWhenOff.position.y > footBase.y + legLength * raisehipheight)
                    {
                        // let hip fall during down step
                        if ((lPlanted && rPlanted) || lUp || rUp)
                        {
                            if (canWalk && hip.followWhenOff.position.y < footBase.y + legLength)
                                upspeed = 0.02f;
                        }
                        else
                            upspeed = -0.001f;
                    }

                    if (wantsStand)  // use the head to rise into an upright stand
                    {
                        head.control.position += (lean * 1.0f + Vector3.up * 0.5f) * Mathf.Clamp01(Time.fixedDeltaTime * speed.val);
                    }

                    if (canWalk)
                    {
                        walkInterp += Time.fixedDeltaTime;
                        walkInterp = Mathf.Clamp01(walkInterp);

                        if(hip.followWhenOff.position.y >= footBase.y + legLength)
                            wantsStand = false;

                        hip.control.position += (lean * walkInterp + Vector3.up * upspeed) * Mathf.Clamp01(Time.fixedDeltaTime * speed.val * 1.0f);

                        if (relax.val)
                        {
                            if (handL.currentPositionState == FreeControllerV3.PositionState.Comply)
                                handL.control.position += lean * Mathf.Clamp01(Time.fixedDeltaTime * speed.val * 0.5f);
                            if (handR.currentPositionState == FreeControllerV3.PositionState.Comply)
                                handR.control.position += lean * Mathf.Clamp01(Time.fixedDeltaTime * speed.val * 0.5f);
                        }
                    }
                    else
                    {
                        walkInterp -= Time.fixedDeltaTime;
                        walkInterp = Mathf.Clamp01(walkInterp);

                        if (Vector3.Dot(lean, hip.control.forward) > 0.0f && !wantsStand)
                        {
                            hip.control.position += (balance + Vector3.up * upspeed) * Mathf.Clamp01(Time.fixedDeltaTime * speed.val * 0.06f);
                        }
                        else
                        {
                            hip.control.position += (lean + Vector3.up * upspeed) * Mathf.Clamp01(Time.fixedDeltaTime * speed.val * 0.25f);
                        }
                    }

                    // hip rotation
                    Quaternion delta = Quaternion.identity;
                    if (shoulderFollow.val)
                    {
                        Vector3 shoulderL2R = shoulderR.followWhenOff.position - shoulderL.followWhenOff.position;
                        Vector3 hipL2R = hipR.followWhenOff.position - hipL.followWhenOff.position;

                        var angle = Vector3.SignedAngle(hipL2R, shoulderL2R, Vector3.up);

                        if(Mathf.Abs(angle) > 5.0f)
                        {
                            delta = Quaternion.AngleAxis(angle * Time.deltaTime * 2.0f, Vector3.up);
                        }
                    }
                    else
                    {
                        Vector3 localLean = hip.control.InverseTransformDirection(lean);
                        localLean.y = 0;

                        if (localLean.sqrMagnitude > 0.0001f)
                        {
                            localLean = localLean.normalized;

                            delta = Quaternion.AngleAxis(Mathf.Sign(localLean.z - 0.01f) * localLean.x * Time.deltaTime * 250.0f, Vector3.up);
                        }
                    }

                    hip.control.rotation = delta * hip.control.rotation;
                    //handL.control.rotation = delta * handL.control.rotation;
                    //handL.control.rotation = delta * handL.control.rotation;

                    if (relax.val)
                    {
                        if (handL.currentPositionState == FreeControllerV3.PositionState.Comply)
                            handL.control.position += (hipR.followWhenOff.position - handL.followWhenOff.position) * Mathf.Clamp01(Time.fixedDeltaTime * speed.val * 0.01f);
                        if (handR.currentPositionState == FreeControllerV3.PositionState.Comply)
                            handR.control.position += (hipR.followWhenOff.position - handR.followWhenOff.position) * Mathf.Clamp01(Time.fixedDeltaTime * speed.val * 0.01f);
                    }
                }               
                else
                {
                    walkInterp -= Time.fixedDeltaTime;
                    walkInterp = Mathf.Clamp01(walkInterp);

                    Vector3 falling = Vector3.down * (0.5f * Time.fixedDeltaTime);

                    if (relax.val)
                    {
                        if (handL.currentPositionState == FreeControllerV3.PositionState.Comply)
                            handL.control.position += falling;
                        if (handR.currentPositionState == FreeControllerV3.PositionState.Comply)
                            handR.control.position += falling;
                    }

                    if (canSit)
                    {
                        // try to stay upright
                        if (head.currentPositionState == FreeControllerV3.PositionState.Comply)
                        {
                            if (lean.sqrMagnitude > maxlean * maxlean)
                            {
                                float lmag = lean.magnitude;
                                head.control.position = head.followWhenOff.position - lean * ((lmag - maxlean) / lmag);
                            }
                        }
                    }
                    else
                    {
                        if (footL.currentPositionState == FreeControllerV3.PositionState.Comply)
                            footL.control.position += falling;
                        if (footR.currentPositionState == FreeControllerV3.PositionState.Comply)
                            footR.control.position += falling;
                    }
                }
            }
        }

        private void UpdateStep()
        {
            if (balance.val)
            {
                Vector3 footTargetBase = 0.5f * (footL.control.position + footR.control.position);
                Vector3 footBase = 0.5f * (footL.followWhenOff.position + footR.followWhenOff.position);
                footTargetBase.y = Mathf.Min(footL.control.position.y, footR.control.position.y);
                footBase.y = Mathf.Min(footL.followWhenOff.position.y, footR.followWhenOff.position.y);

                Vector3 velocity = (centerOfMass - prevCoM) / Time.deltaTime;
                prevCoM = centerOfMass;

                // a little calculus
                float t = Mathf.Sqrt(Mathf.Max(0.01f, centerOfMass.y - footBase.y) / (-0.5f * Physics.gravity.y));

                // the point where the center of mass would land if it simply fell to the floor with its current momentum
                Vector3 projectedCenterOfMass = centerOfMass + velocity * t;

                //Vector3 hipL2R = hipL.followWhenOff.position - hipR.followWhenOff.position;

                // bend in spine drives walking
                Vector3 lean = head.followWhenOff.position - hip.followWhenOff.position;
                lean.y = 0;
                Vector3 balance = footBase - centerOfMass;
                balance.y = 0;
                
                //Vector3 tilt = projectedCenterOfMass - footTargetBase;
                //tilt.y = 0;
                //
                //Vector3 tilt2 = projectedCenterOfMass - footBase;
                //tilt2.y = 0;
                //
                //Vector3 footLerr = footL.followWhenOff.position - footL.control.position;
                //footLerr.y = 0;
                //Vector3 footRerr = footR.followWhenOff.position - footR.control.position;
                //footRerr.y = 0;

                Vector3 leftToCoM = projectedCenterOfMass - footL.followWhenOff.position;
                leftToCoM.y = 0;
                Vector3 rightToCoM = projectedCenterOfMass - footR.followWhenOff.position;
                rightToCoM.y = 0;
                
                // compute support polygon
                Vector3 supportlf = footL.followWhenOff.position - footL.followWhenOff.right * 0.1f + footL.followWhenOff.forward * 0.1f;
                supportlf.y = 0;
                Vector3 supportlb = footL.followWhenOff.position - footL.followWhenOff.right * 0.1f - footL.followWhenOff.forward * 0.1f;
                supportlb.y = 0;

                Vector3 supportrf = footR.followWhenOff.position + footL.followWhenOff.right * 0.1f + footR.followWhenOff.forward * 0.1f;
                supportrf.y = 0;
                Vector3 supportrb = footR.followWhenOff.position + footL.followWhenOff.right * 0.1f - footR.followWhenOff.forward * 0.1f;
                supportrb.y = 0;

                //Debug.DrawLine(supportlf, supportrf);
                //Debug.DrawLine(supportlb, supportrb);
                //
                //Debug.DrawLine(supportlf, supportlb);
                //Debug.DrawLine(supportrf, supportrb);

                var supportFplane = new Plane(supportlf, supportrf, supportlf + Vector3.down);
                var supportBplane = new Plane(supportlb, supportrb, supportrb + Vector3.up);

                var supportLplane = new Plane(Vector3.Cross(footL.followWhenOff.forward, Vector3.down), footL.followWhenOff.position);
                var supportRplane = new Plane(Vector3.Cross(footR.followWhenOff.forward, Vector3.up), footR.followWhenOff.position);

                // how close are we to the support polygon, from the inside or outside?
                float balanceDistance = Mathf.Min(
                    supportFplane.GetDistanceToPoint(projectedCenterOfMass),
                    supportBplane.GetDistanceToPoint(projectedCenterOfMass),
                    supportLplane.GetDistanceToPoint(projectedCenterOfMass) + 0.1f,
                    supportRplane.GetDistanceToPoint(projectedCenterOfMass) + 0.1f);

                //Log("distance = " + distance);
                
                if (!canStand || !canSit)
                {
                    lTarget = footL.followWhenOff.position;
                    rTarget = footR.followWhenOff.position;
                    //lup = false;
                    //rup = false;
                }

                //bool unbalanced = !rup && !lup
                //    && (footLerr.sqrMagnitude > maxfoot * maxfoot || footRerr.sqrMagnitude > maxfoot * maxfoot);

                // standing state, tried to keep center of mass over the support polygon, with hysteresis
                canStand &= hip.followWhenOff.position.y > footBase.y + legLength * minhipheight
                    && balanceDistance > -0.15f;
                canStand |= hip.followWhenOff.position.y > footBase.y + legLength * minhipheight
                    && balanceDistance > -0.1f;

                if (canStand)
                {
                    if (hip.currentPositionState == FreeControllerV3.PositionState.Comply || rUp || lUp)
                    {
                        if(kneeL.currentPositionState == FreeControllerV3.PositionState.Comply)
                        {
                            kneeL.currentPositionState = FreeControllerV3.PositionState.Off;
                            kneeL.currentRotationState = FreeControllerV3.RotationState.Off;
                        }
                        if (kneeR.currentPositionState == FreeControllerV3.PositionState.Comply)
                        {
                            kneeR.currentPositionState = FreeControllerV3.PositionState.Off;
                            kneeR.currentRotationState = FreeControllerV3.RotationState.Off;
                        }

                        // cancel stand if head goes down, unreliable
                        if (!canWalk && wantsStand)
                        {
                            //if (head.followWhenOff.position.y + 0.05f < lastHeadHeight || head.currentPositionState != FreeControllerV3.PositionState.Comply)
                            //  wantsStand = false;
                        }

                        if (relax.val)
                        {
                            if (canWalk)
                            {
                                handL.RBComplyPositionSpring = 400;
                                handR.RBComplyPositionSpring = 400;
                                
                                handL.RBComplyRotationSpring = 150;
                                handR.RBComplyRotationSpring = 150;
                            }
                            else
                            {
                                handL.RBComplyPositionSpring = 1000;
                                handR.RBComplyPositionSpring = 1000;

                                handL.RBComplyRotationSpring = 400;
                                handR.RBComplyRotationSpring = 400;
                            }
                        }

                        head.RBComplyPositionSpring = 1000;

                        shoulderL.RBComplyPositionSpring = 100;
                        shoulderR.RBComplyPositionSpring = 100;
                        chest.RBComplyPositionSpring = 100;

                        hip.RBComplyPositionSpring = 5000;
                        hip.RBComplyRotationSpring = 250;

                        footL.RBComplyPositionSpring = 400;
                        footR.RBComplyPositionSpring = 400;

                        Vector3 hipDirection = hip.control.forward;
                        hipDirection.y = 0;
                        hipDirection.Normalize();

                        // take a step if the center of mass moves outside the support polygon
                        if (balanceDistance < -0.01f && lPlanted && rPlanted)
                        {
                            if (rightToCoM.sqrMagnitude > leftToCoM.sqrMagnitude)
                            {
                                Vector3 candidate = projectedCenterOfMass + leftToCoM;
                                candidate.y = footBase.y;

                                float d = supportLplane.GetDistanceToPoint(candidate);

                                if (d < 0.0f)
                                {
                                    // step with lead foot instead of crossing over
                                    candidate = projectedCenterOfMass + rightToCoM * 0.5f;
                                    candidate.y = footBase.y;

                                    stepDistance = Vector3.Distance(lTarget, candidate);
                                    // if (relax.val && canWalk && handR.currentPositionState == FreeControllerV3.PositionState.Comply)
                                    //  handR.control.position += candidate - lTarget;

                                    lTarget = candidate;
                                    lRotation = Quaternion.LookRotation(hipDirection + Vector3.down * 4.0f, Vector3.up);
                                    lUp = true;
                                    lPlanted = false;
                                }
                                else
                                {
                                    // keep it from getting too close to the other foot
                                    if (d < 0.1f)
                                        candidate = supportLplane.ClosestPointOnPlane(candidate) + supportLplane.normal * 0.1f;

                                    stepDistance = Vector3.Distance(rTarget, candidate);
                                    //if (relax.val && canWalk && handL.currentPositionState == FreeControllerV3.PositionState.Comply)
                                    //handL.control.position += candidate - lTarget;

                                    rTarget = candidate;
                                    rRotation = Quaternion.LookRotation(hipDirection + Vector3.down * 4.0f, Vector3.up);
                                    rUp = true;
                                    rPlanted = false;
                                }
                            }
                            else
                            {
                                Vector3 candidate = projectedCenterOfMass + rightToCoM * 0.5f;
                                candidate.y = footBase.y;

                                float d = supportRplane.GetDistanceToPoint(candidate);

                                if (d < 0.0f)
                                {
                                    // step with lead foot instead of crossing over
                                    candidate = projectedCenterOfMass + leftToCoM;
                                    candidate.y = footBase.y;

                                    stepDistance = Vector3.Distance(rTarget, candidate);
                                    //  if (relax.val && canWalk && handL.currentPositionState == FreeControllerV3.PositionState.Comply)
                                    //   handL.control.position += candidate - lTarget;

                                    rTarget = candidate;
                                    rRotation = Quaternion.LookRotation(hipDirection + Vector3.down * 4.0f, Vector3.up);
                                    rUp = true;
                                    rPlanted = false;
                                }
                                else
                                {
                                    // keep it from getting too close to the other foot
                                    if (d < 0.1f)
                                        candidate = supportRplane.ClosestPointOnPlane(candidate) + supportRplane.normal * 0.1f;

                                    stepDistance = Vector3.Distance(lTarget, candidate);
                                    // if (relax.val && canWalk && handR.currentPositionState == FreeControllerV3.PositionState.Comply)
                                    //   handR.control.position += candidate - lTarget;

                                    lTarget = candidate;
                                    lRotation = Quaternion.LookRotation(hipDirection + Vector3.down * 4.0f, Vector3.up);
                                    lUp = true;
                                    lPlanted = false;
                                }
                            }
                        }

                        if (footL.followWhenOff.position.y > footBase.y + 0.25f * stepDistance)
                        {
                            lUp = false;
                            lRotation = Quaternion.LookRotation(hipDirection + Vector3.down * 0.2f, Vector3.up);
                            footL.RBComplyRotationSpring = 350;
                        }
                        if (footR.followWhenOff.position.y > footBase.y + 0.25f * stepDistance)
                        {
                            rUp = false;
                            rRotation = Quaternion.LookRotation(hipDirection + Vector3.down * 0.2f, Vector3.up);
                            footR.RBComplyRotationSpring = 350;
                        }

                        // check if the foot is standing on something
                        RaycastHit hit;

                        if (!lPlanted && !lUp && footL.followWhenOffRB.SweepTest(Vector3.down, out hit, footHeight.val) && hit.rigidbody.isKinematic)
                            lPlanted = true;
                        if (!rPlanted && !rUp && footR.followWhenOffRB.SweepTest(Vector3.down, out hit, footHeight.val) && hit.rigidbody.isKinematic)
                            rPlanted = true;

                        // allow feet to be moved laterally if we are not stepping, but keep them on the floor
                        if (lPlanted && rPlanted)
                        {
                            lTarget = footL.control.position;
                            rTarget = footR.control.position;
                        }

                        lTarget.y = (lUp ? footBase.y + stepheight + 0.5f * stepDistance : footBase.y - 0.05f);
                        rTarget.y = (rUp ? footBase.y + stepheight + 0.5f * stepDistance : footBase.y - 0.05f);

                        footR.control.position = rTarget;
                        footL.control.position = lTarget;

                        if (!lPlanted)
                            footR.control.rotation = rRotation;
                        if (!rPlanted)
                            footL.control.rotation = lRotation;

                        //if(!rup)
                        //    footR.control.rotation = Quaternion.LookRotation(footR.control.forward, Vector3.up);
                        //if(!lup)
                        //    footL.control.rotation = Quaternion.LookRotation(footL.control.forward, Vector3.up);

                        // upright walking state, hips follow the head
                        canWalk = lean.sqrMagnitude < maxtilt * maxtilt
                            && hip.followWhenOff.position.y > footBase.y + legLength * maxhipheight
                            && balanceDistance > -0.01f;

                        if(canWalk)
                        {
                            kneeL.RBComplyPositionSpring = 0;
                            kneeR.RBComplyPositionSpring = 0;

                            kneeL.RBComplyPositionDamper = 0;
                            kneeR.RBComplyPositionDamper = 0;

                            kneeL.RBComplyRotationSpring = 0;
                            kneeR.RBComplyRotationSpring = 0;

                            kneeL.RBComplyRotationDamper = 0;
                            kneeR.RBComplyRotationDamper = 0;

                            elbowL.RBComplyPositionSpring = 0;
                            elbowR.RBComplyPositionSpring = 0;

                            elbowL.RBComplyRotationSpring = 0;
                            elbowR.RBComplyRotationSpring = 0;
                        }
                        else
                        {
                            kneeL.RBComplyPositionSpring = 500;
                            kneeR.RBComplyPositionSpring = 500;

                            elbowL.RBComplyPositionSpring = 100;
                            elbowR.RBComplyPositionSpring = 100;
                        }
                    }

                    // don't modify the feet if they are mid-step
                    if (canStand && !(lPlanted && rPlanted))
                    {
                        footL.PauseComply(1);
                        footR.PauseComply(1);
                    }
                }
                else
                {
                    if (kneeL.currentPositionState == FreeControllerV3.PositionState.Off)
                    {
                        kneeL.currentPositionState = FreeControllerV3.PositionState.Comply;
                    }
                    if (kneeR.currentPositionState == FreeControllerV3.PositionState.Off)
                    {
                        kneeR.currentPositionState = FreeControllerV3.PositionState.Comply;
                    }

                    if (relax.val)
                    {
                        handL.RBComplyPositionSpring = 500;
                        handR.RBComplyPositionSpring = 500;
                                                
                        handL.RBComplyRotationSpring = 400;
                        handR.RBComplyRotationSpring = 400;
                    }

                    Vector3 kneeDir = 0.5f * (kneeL.control.position + kneeR.control.position) - hip.control.position;

                    // sitting up state, tries to stay upright above hips
                    canSit = hip.currentPositionState == FreeControllerV3.PositionState.Comply
                        && (lean.sqrMagnitude < layThreshold.val * layThreshold.val)
                        && Vector3.Dot(kneeDir, hip.control.forward) > 0;

                    if (canSit)
                    {
                        // if the hips are high enough above the feet, try to stand up at next possible chance
                        if (hip.followWhenOff.position.y < footBase.y + legLength * raisehipheight)
                            wantsStand = true;  

                        hip.RBComplyPositionSpring = 100;
                        hip.RBComplyRotationSpring = 250;

                        footL.RBComplyPositionSpring = 0;

                        footR.RBComplyPositionSpring = 0;

                        head.RBComplyPositionSpring = 1000;
                        
                        shoulderL.RBComplyPositionSpring = 250;
                        shoulderR.RBComplyPositionSpring = 250;
                        chest.RBComplyPositionSpring = 250;

                        elbowL.RBComplyPositionSpring = 0;
                        elbowR.RBComplyPositionSpring = 0;

                        kneeL.RBComplyPositionSpring = 500;
                        kneeR.RBComplyPositionSpring = 500;
                    }
                    else
                    {
                        // fall if we don't have our forearms or forlegs supported by something rigid
                        RaycastHit hit;
                        laySupports = 0;

                        if (kneeL.followWhenOffRB.SweepTest(Vector3.down, out hit, 0.09f) && hit.rigidbody.isKinematic)
                        {
                            kneeL.RBComplyPositionSpring = 500;
                            laySupports++;
                        }
                        else
                        {
                            kneeL.RBComplyPositionSpring = 0;
                        }
                        if (kneeR.followWhenOffRB.SweepTest(Vector3.down, out hit, 0.09f) && hit.rigidbody.isKinematic)
                        {
                            kneeR.RBComplyPositionSpring = 500;
                            laySupports++;
                        }
                        else
                        {
                            kneeR.RBComplyPositionSpring = 0;
                        }
                        if (elbowL.followWhenOffRB.SweepTest(Vector3.down, out hit, 0.07f) && hit.rigidbody.isKinematic)
                        {
                            elbowL.RBComplyPositionSpring = 500;
                            laySupports++;
                        }
                        else
                        {
                            elbowL.RBComplyPositionSpring = 0;
                        }
                        if (elbowR.followWhenOffRB.SweepTest(Vector3.down, out hit, 0.07f) && hit.rigidbody.isKinematic)
                        {
                            elbowR.RBComplyPositionSpring = 500;
                            laySupports++;
                        }
                        else
                        {
                            elbowR.RBComplyPositionSpring = 0;
                        }

                        footL.RBComplyPositionSpring = 1500;
                        footR.RBComplyPositionSpring = 1500;

                        footL.RBComplyRotationSpring = 150;
                        footR.RBComplyRotationSpring = 150;

                        head.RBComplyPositionSpring = 50;
                        shoulderL.RBComplyPositionSpring = 0;
                        shoulderR.RBComplyPositionSpring = 0;

                        hip.RBComplyPositionSpring = (laySupports > 2 ? 1000 * laySupports : 100);
                        chest.RBComplyPositionSpring = (laySupports > 2 ? 500 * laySupports : 100);
                        head.RBComplyPositionSpring = (laySupports > 2 ? 1000 * laySupports : 100);

                        hip.RBComplyRotationSpring = 50;
                    }
                }
            }
            else
            {
                canStand = false;
                canWalk = false;
                canSit = false;
            }

            lastHeadHeight = head.followWhenOff.position.y;
        }
    }
}
