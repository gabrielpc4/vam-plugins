using System;
using UnityEngine;

namespace VAMLaunchPlugin
{
    public static class LaunchUtils
    {
        public const float LAUNCH_MAX_VAL = 99.0f;
        public const float LAUNCH_MIN_SPEED = 10.0f;
        public const float LAUNCH_MAX_SPEED = 90.0f;

        // Speed returns: the speed (in percent) to move the given distance (in percent) in the given duration.
        // https://github.com/funjack/launchcontrol/blob/master/protocol/funscript/functions.go#L10
        public static float PredictMoveSpeed(float prevPos, float nextPos, float durationSecs)
        {
            double durationNanoSecs = durationSecs * 1e9;
            
            double delta = nextPos - prevPos;
            double dist = Math.Abs(delta);

            double mil = (durationNanoSecs / 1e6) * 90 / dist;
            double speed = 25000.0 * Math.Pow(mil, -1.05);

            return Mathf.Clamp((float)speed, LAUNCH_MIN_SPEED, LAUNCH_MAX_SPEED);
        }

        // Duration: returns the time it will take to move the given distance (in percent) at the given speed (in percent.)
        // https://github.com/funjack/launchcontrol/blob/master/protocol/funscript/functions.go#L23
        public static float PredictMoveDuration(float dist, float speed)
        {
            if (dist <= 0.0f)
            {
                return 0.0f;
            }

            double mil = Math.Pow(speed / 25000, -0.95);
            double dur = (mil / (90 / dist)) / 1000;
            return (float) dur;
        }

        // Distance: returns the distance (in percent) that will be moved with the given speed (in percent) and duration.
        // https://github.com/funjack/launchcontrol/blob/master/protocol/funscript/functions.go#L34
        public static float PredictDistanceTraveled(float speed, float durationSecs)
        {
            if (speed <= 0.0f)
            {
                return 0.0f;
            }

            double durationNanoSecs = durationSecs * 1e9;
            
            double mil = Math.Pow((double)speed / 25000, -0.95);
            double diff = mil - durationNanoSecs / 1e6;
            double dist = 90 - (diff / mil * 90);

            return (float) dist;
        }
    }
}