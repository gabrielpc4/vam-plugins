using System;
using UnityEngine;

namespace VAMLaunchPlugin
{
    public class FpsLagFactor
    {
        private float _smoothedUnscaledDeltaTime = 0f;
        private float _lagFactorLastRealtime = 0f;

        public float Factor = 1f;

        public float Update()
        {
            float realtimeSinceStartup = Time.realtimeSinceStartup;
            float realTimeDelta = realtimeSinceStartup - _lagFactorLastRealtime;
            _smoothedUnscaledDeltaTime = (_smoothedUnscaledDeltaTime * 4f + Time.unscaledDeltaTime) * 0.2f;
            _lagFactorLastRealtime = realtimeSinceStartup;
            if (realTimeDelta < 1f && realTimeDelta > 0.001f && _smoothedUnscaledDeltaTime <= realTimeDelta) // failsafes to prevent stupid data
            {
                Factor = _smoothedUnscaledDeltaTime / realTimeDelta;
            }
            else
            {
                Factor = 1f;
            }
            return Factor;
        }
    }
}