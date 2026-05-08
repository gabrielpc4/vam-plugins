using System;

namespace geesp0t
{
    public partial class OnSceneStartup
    {
        private const bool exposureDebugLog = true;

        private int dbgLoadSerial = 0;

        private int dbgSkipLogForLoadSerial = -1;

        private int dbgApplyNullGlForLoadSerial = -1;

        private int dbgForceZeroLogForLoadSerial = -1;

        private int dbgStuckZeroLogForLoadSerial = -1;

        void LogExposureDbg(string message)
        {
            if (!exposureDebugLog)
            {
                return;
            }

            SuperController.LogMessage(
                "[OnSceneStartupDbg] " +
                DateTime.Now.ToString("HH:mm:ss.fff") +
                " " +
                message);
        }

        string DiagFormatExposureState()
        {
            JSONStorable globalLightingStorable = TryGetGlobalLightingStorable();
            if (globalLightingStorable == null)
            {
                return "gl=null backup=" + camExposureBackupCaptured +
                    " savedTarget=" + savedCamExposure.ToString("F4");
            }

            float camExposure =
                globalLightingStorable.GetFloatParamValue(camExposureParamName);
            return "camExp=" + camExposure.ToString("F4") + " backup=" +
                camExposureBackupCaptured + " savedTarget=" +
                savedCamExposure.ToString("F4");
        }

        static string DiagSettleBreakdown(
            SuperController superController,
            bool rawCombined,
            bool isLoadingNow)
        {
            if (superController == null)
            {
                return "sc=null";
            }

            return string.Format(
                "raw={0} load={1} ui={2} alt={3} geo={4} icon={5}",
                rawCombined,
                isLoadingNow,
                IsTransformActive(superController.loadingUI),
                IsTransformActive(superController.loadingUIAlt),
                IsTransformActive(superController.loadingGeometry),
                IsTransformActive(superController.loadingIcon));
        }
    }
}
